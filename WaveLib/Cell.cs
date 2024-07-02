namespace WaveLib
{
	public enum CellStatus
	{
		Undetermined,
		Collapsed,
		Exhausted,
	}

	public class Cell
	{
		int position = -1;
		CellStatus cellState;
		readonly HashSet<int> superPosition = [];

		public CellStatus Status { get => cellState; }
		public IReadOnlySet<int> SuperState { get => superPosition; }
		public int State { get => position; }

		internal void Constrain(IEnumerable<int> superPosition)
		{
			if (cellState == CellStatus.Undetermined)
			{
				this.superPosition.IntersectWith(superPosition);
				if (this.superPosition.Count == 0)
					cellState = CellStatus.Exhausted;
			}
		}

		internal int Collapse(Random random, Func<int, int> weightSelector)
		{
			if (cellState == CellStatus.Undetermined)
			{
				position = superPosition.ElementAtRandom(random, weightSelector);
				superPosition.Clear();
				cellState = CellStatus.Collapsed;
			}
			return position;
		}

		internal void Reset(IEnumerable<int> superPosition)
		{
			cellState = CellStatus.Undetermined;
			position = -1;
			this.superPosition.Clear();
			this.superPosition.UnionWith(superPosition);
		}
	}
}
