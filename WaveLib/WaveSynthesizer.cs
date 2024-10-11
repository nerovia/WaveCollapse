using System.Diagnostics.SymbolStore;

namespace WaveLib
{
	public class WaveSynthesizer(int width, int height, IEnumerable<Constraint> constraints, Random random)
	{
		readonly HashSet<int> tileIds = constraints.Select(it => it.SubjectId).ToHashSet();
		readonly IEnumerable<IGrouping<GridOffset, Constraint>> offsetGroups = constraints.GroupBy(it => it.Delta);
		readonly Func<int, int> WeightSelector = id => 1;

		public Random Random { get; set; } = random;
		public IGrid<Cell> Grid { get; } = WaveLib.Grid.Create<Cell>(width, height, _ => new());
				
		IEnumerable<GridPosition<Cell>>? Priority()
		{
			return (from pos in Grid.Traverse()
					where pos.Cell.Status == CellStatus.Undetermined
					group pos by pos.Cell.SuperState.Count).MinBy(it => it.Key);
		}

		public void Reset()
		{
			foreach (var cell in Grid)
				cell.Reset(tileIds);
		}

		public bool CollapseNext(Action<GridPosition<Cell>>? callback = null)
		{
			var next = Priority()?.ElementAtRandom(Random);
			if (next.HasValue)
			{
				var (x, y, cell) = next.Value;
				var subId = cell.Collapse(Random, WeightSelector);
				callback?.Invoke(next.Value);

				foreach (var grouping in offsetGroups)
				{
					var delta = grouping.Key;
					var pos = Grid.AtWrap(x + delta.X, y + delta.Y);
					pos.Cell.Constrain(from constraint in grouping where constraint.SubjectId == subId select constraint.ObjectId);
					callback?.Invoke(pos);
				}
				return true;
			}
			return false;
		}
	}
}
