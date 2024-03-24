using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveLib
{
	public class WaveSchema
	{
		public readonly HashSet<int> TileIds = [];
		public readonly HashSet<Pattern> Patterns = [];
		public readonly List<(int, int)> Offsets = [];

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
				Offsets.Add((dx, dy));
				return true;
			}
			return false;
		}

		public static (WaveSchema, T[]) Analyze<T>(IGrid<T> grid, IEnumerable<(int, int)> offsets) where T : notnull
		{
			var schema = new WaveSchema();
			var tileSet = new List<T>();
			var reverse = new Dictionary<T, int>();

			int GetTileId(T tile)
			{
				if (!reverse!.TryGetValue(tile, out var tileId))
				{
					tileId = tileSet!.Count;
					reverse.Add(tile, tileId);
					tileSet.Add(tile);
				}
				return tileId;
			}

			foreach (var (x, y, sub) in grid.Traverse())
			{
				foreach (var (dx, dy, obj) in grid.TraverseOffsets(x, y, offsets))
				{
					var subId = GetTileId(sub);
					var objId = GetTileId(obj);
					schema.With(subId, objId, dx, dy);
				}
			}

			return new(schema, [.. tileSet]);
		}

		public static class Stencil
		{
			public static readonly (int, int)[] Plus = [(1, 0), (0, 1), (-1, 0), (0, -1)];
			public static readonly (int, int)[] Full = [(1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1)];
		}
	}
}
