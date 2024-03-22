namespace WaveLib
{
	public class Cell
	{
		ISet<int> SuperState { get; } = new HashSet<int>();

		public int State { get; private set; } = -1;
		public bool IsCollapsed { get => State != -1; }
		public bool IsExhausted { get => SuperState.Count == 0; }
		public int Entropy { get => SuperState.Count; }

		public bool Reduce(IEnumerable<int> tiles)
		{
			SuperState.IntersectWith(tiles);
			return IsExhausted;
		}

		public int Collapse(Random random, Func<int, int> weightSelector)
		{
			State = SuperState.ElementAtRandom(random, weightSelector);
			SuperState.Clear();
			return State;
		}

		public void Reset(IEnumerable<int> tiles)
		{
			State = -1;
			SuperState.Clear();
			SuperState.UnionWith(tiles);
		}
	}
}
