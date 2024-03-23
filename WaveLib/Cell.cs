namespace WaveLib
{
	public class Cell
	{
		ISet<int> SuperState { get; } = new HashSet<int>();

		public int StateId { get; private set; } = -1;
		public bool IsCollapsed { get => StateId != -1; }
		public bool IsExhausted { get => SuperState.Count == 0; }
		public int Entropy { get => SuperState.Count; }

		public bool Reduce(IEnumerable<int> tiles)
		{
			SuperState.IntersectWith(tiles);
			return IsExhausted;
		}

		public int Collapse(Random random, Func<int, int> weightSelector)
		{
			StateId = SuperState.ElementAtRandom(random, weightSelector);
			SuperState.Clear();
			return StateId;
		}

		public void Reset(IEnumerable<int> tiles)
		{
			StateId = -1;
			SuperState.Clear();
			SuperState.UnionWith(tiles);
		}
	}
}
