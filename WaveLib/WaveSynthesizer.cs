namespace WaveLib
{
	public class WaveSynthesizer(int width, int height, WaveSchema schema, Random random)
	{
		readonly HashSet<int> tileIds = schema.Select(it => it.SubjectId).ToHashSet();
		readonly IEnumerable<IGrouping<GridOffset, Constraint>> constraints = schema.GroupBy(it => it.Delta);
		readonly Func<int, int> WeightSelector = id => 1;

		public readonly Random Random = random;
		public IGrid<Cell> Grid { get; } = WaveLib.Grid.Create<Cell>(width, height, _ => new());
				
		IEnumerable<GridPosition<Cell>>? Priority()
		{
			return (from pos in Grid.Traverse()
					where pos.Cell.State == CellState.SuperPosition
					group pos by pos.Cell.SuperPosition.Count).MinBy(it => it.Key);
		}

		public void Reset()
		{
			foreach (var cell in Grid)
				cell.Reset(tileIds);
		}

		public bool CollapseNext(ICollection<GridPosition<Cell>>? changes = null)
		{
			var next = Priority()?.ElementAtRandom(Random);
			if (next.HasValue)
			{
				var (x, y, cell) = next.Value;
				var subId = cell.Collapse(Random, WeightSelector);
				changes?.Add(next.Value);

				foreach (var grouping in constraints)
				{
					var delta = grouping.Key;
					if (Grid.TryAt(x + delta.X, y + delta.Y, out var pos))
					{
						pos.Cell.Constrain(from constraint in grouping where constraint.SubjectId == subId select constraint.ObjectId);
						changes?.Add(pos);
					}
				}
				return true;
			}
			return false;
		}
	}
}
