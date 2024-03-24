using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Formats.Tar;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WaveLib
{
	public class WaveSchema
	{
		public readonly HashSet<int> TileIds = [];
		public readonly HashSet<Pattern> Patterns = [];
		public readonly List<GridOffset> Offsets = [];

		public WaveSchema With(int subId, int objId, int dx, int dy)
		{
			if (Add(subId, objId, dx, dy))
				Add(objId, subId, -dx, -dy);
			return this;
		}

		bool Add(int subId, int objId, int dx, int dy)
		{
			if (Patterns.Add(new(subId, objId, dx, dy)))
			{
				TileIds.Add(subId);
				Offsets.Add(new(dx, dy));
				return true;
			}
			return false;
		}

		public class TileSetBuilder<T>
		{
			readonly List<T> tileSet = new();
			readonly Dictionary<T, int> reverse = new();

			public int Add(T tile)
			{
				if (!reverse.TryGetValue(tile, out var tileId))
				{
					tileId = tileSet.Count;
					reverse.Add(tile, tileId);
					tileSet.Add(tile);
				}
				return tileId;
			}

			public T[] Build() => tileSet.ToArray();
		}

		public static (WaveSchema, T[]) Analyze<T>(IGrid<T> grid, IEnumerable<GridOffset> offsets) where T : notnull
		{
			var schema = new WaveSchema();
			var tileSet = new TileSetBuilder<T>();

			foreach (var (x, y, sub) in grid.Traverse())
			{
				foreach (var ((dx, dy), (_, _, obj)) in grid.TraverseOffsets(x, y, offsets))
				{
					var subId = tileSet.Add(sub);
					var objId = tileSet.Add(obj);
					schema.Add(subId, objId, dx, dy);
				}
			}

			return new(schema, tileSet.Build());
		}

		public static (WaveSchema, string[]) Parse(Stream stream)
		{
			using var reader = new StreamReader(stream);
			var tileSet = new TileSetBuilder<string>();
			var schema = new WaveSchema();
			var cls0 = reader.ReadToEnd();
			try
			{
				foreach (var c0 in cls0.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
				{
					Debug.WriteLine(new { c0 });
					var cls1 = c0.Split(' ', 2);
					var sub = cls1[0];
					var subId = tileSet.Add(sub);

					foreach (var c1 in cls1[1].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
					{
						Debug.WriteLine(new { c1 });
						var cls2 = c1.Split(' ', 2);
						GridOffset[] offsets = cls2[0] switch // Regex.Match(, @"\[(\S+?)\]").Groups[1].Value switch
						{
							"l" => [(-1, 0)],
							"r" => [(1, 0)],
							"b" => [(0, 1)],
							"t" => [(0, -1)],
							"tl" => [(-1, -1)],
							"tr" => [(1, -1)],
							"bl" => [(-1, 1)],
							"br" => [(1, 1)],
							"-" => [(1, 0), (-1, 0)],
							"|" => [(0, 1), (0, -1)],
							"\\" => [(-1, -1), (1, 1)],
							"/" => [(-1, 1), (1, -1)],
							"x" => [(-1, -1), (1, 1), (-1, 1), (1, -1)],
							"+" => [(1, 0), (-1, 0), (0, 1), (0, -1)],
							"*" => [(1, 0), (-1, 0), (0, 1), (0, -1), (-1, -1), (1, 1), (-1, 1), (1, -1)],
							_ => throw new ArgumentException()
						};

						foreach (var c2 in cls2[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
						{
							Debug.WriteLine(new { c2 });
							var obj = c2; //Regex.Match(c2, @"<(\S+?)>").Groups[1].Value;
							var objId = tileSet.Add(obj);

							foreach (var offset in offsets)
							{
								schema.With(subId, objId, offset.X, offset.Y);
							}
						}
					}
				}

				return (schema, tileSet.Build());
			}
			catch
			{
				Debug.WriteLine("Something went wrong");
				throw;
			}

		}

		public static class Stencil
		{
			public static readonly GridOffset[] Plus = [(1, 0), (0, 1), (-1, 0), (0, -1)];
			public static readonly GridOffset[] Full = [(1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1)];
		}
	}
}
