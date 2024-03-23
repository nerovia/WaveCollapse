using System.Linq;

namespace WaveLib
{
	public record class WaveAnalysisResult<T>(Pattern[] Patterns, T[] TileSet) where T : notnull;

	public class WaveAnalyzer()
	{
		public static WaveAnalysisResult<T> Analyze<T>(IGrid<T> grid) where T : notnull
		{
			//var rules = new Dictionary<Rule, int>();
			var patterns = new HashSet<Pattern>();
			var tiles = new Queue<T>();
			var ids = new Dictionary<T, int>();

			int GetTileId(T tile)
			{
				if (!ids!.TryGetValue(tile, out var tileId))
				{
					tileId = tiles!.Count;
					ids.Add(tile, tiles.Count);
					tiles.Enqueue(tile);
				}
				return tileId;
			}

			foreach (var (x, y, sub) in grid.Traverse())
			{
				foreach (var (dx, dy, obj) in grid.TraverseOffsets(x, y, Pattern.Offsets))
				{
					var subId = GetTileId(sub);
					var objId = GetTileId(obj);
					patterns.Add(new Pattern(subId, objId, dx, dy));
				}
			}

			return new(patterns.ToArray(), tiles.ToArray());
		}
	}
}
