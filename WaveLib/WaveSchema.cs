using System.Collections;
using System.Diagnostics;

namespace WaveLib
{
	public class WaveSchema : IEnumerable<Constraint>
	{
		readonly HashSet<Constraint> constraints = [];

		public Dictionary<Constraint, int> Weights { get; } = new();

		public IEnumerator<Constraint> GetEnumerator() => constraints.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => constraints.GetEnumerator();

		public bool Add(int subId, int objId, GridOffset delta)
		{
			var item = new Constraint(subId, objId, delta);
			if (constraints.Add(item))
			{
				constraints.TryGetValue(item, out var constraint);
				constraints.Add(new(objId, subId, -delta));
				return false;
			}
			return false;
		}

		public static (WaveSchema, T[]) Analyze<T>(IGrid<T> grid, IEnumerable<GridOffset> stencil) where T : notnull
		{
			var schema = new WaveSchema();
			var tileSet = new TileSetBuilder<T>();

			foreach (var (x, y, sub) in grid.Traverse())
			{
				foreach (var delta in stencil)
				{
					if (grid.TryGet(x + delta.X, y + delta.Y, out var obj))
					{
						var subId = tileSet.Add(sub);
						var objId = tileSet.Add(obj);
						schema.constraints.Add(new(subId, objId, delta));
					}
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
						GridOffset[] stencil = cls2[0] switch
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
							var obj = c2;
							var objId = tileSet.Add(obj);

							foreach (var delta in stencil)
							{
								schema.Add(subId, objId, delta);
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

	public class TileSetBuilder<T> where T : notnull
	{
		readonly List<T> tileSet = [];
		readonly Dictionary<T, int> reverse = [];

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
}
