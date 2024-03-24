namespace WaveLib
{
	public class Cell
	{
		HashSet<int> SuperState { get; } = [];

		public int TileId { get; private set; } = -1;
		public bool IsCollapsed { get => TileId != -1; }
		public bool IsExhausted { get => SuperState.Count == 0; }
		public int Entropy { get => SuperState.Count; }

		public bool Reduce(IEnumerable<int> tiles)
		{
			SuperState.IntersectWith(tiles);
			return IsExhausted;
		}

		public int Collapse(Random random, Func<int, int> weightSelector)
		{
			TileId = SuperState.ElementAtRandom(random, weightSelector);
			SuperState.Clear();
			return TileId;
		}

		public void Reset(IEnumerable<int> tiles)
		{
			TileId = -1;
			SuperState.Clear();
			SuperState.UnionWith(tiles);
		}
	}
}
