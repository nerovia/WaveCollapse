namespace WaveLib
{
	public enum CellState
	{
		SuperPosition,
		Collapsed,
		Exhausted,
	}

	public class Cell
	{
		int position = -1;
		CellState cellState;
		readonly HashSet<int> superPosition = [];

		public CellState State { get => cellState; }
		public IReadOnlySet<int> SuperPosition { get => superPosition; }
		public int Position { get => position; }

		internal void Constrain(IEnumerable<int> superPosition)
		{
			if (cellState == CellState.SuperPosition)
			{
				this.superPosition.IntersectWith(superPosition);
				if (this.superPosition.Count == 0)
					cellState = CellState.Exhausted;
			}
		}

		internal int Collapse(Random random, Func<int, int> weightSelector)
		{
			if (cellState == CellState.SuperPosition)
			{
				position = superPosition.ElementAtRandom(random, weightSelector);
				superPosition.Clear();
				cellState = CellState.Collapsed;
			}
			return position;
		}

		internal void Reset(IEnumerable<int> superPosition)
		{
			cellState = CellState.SuperPosition;
			position = -1;
			this.superPosition.Clear();
			this.superPosition.UnionWith(superPosition);
		}
	}
}
