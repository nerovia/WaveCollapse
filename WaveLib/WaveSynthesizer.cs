using System.Data;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using WaveLib;

namespace WaveLib
{
	public class WaveSynthesizer(Random random, IGrid<Cell> cells, IEnumerable<Pattern> patterns, IEnumerable<int> tiles, Func<int, int> weightSelector)
	{
		public void Reset()
		{
			foreach (var cell in cells)
				cell.Reset(tiles);
		}
		
		public (Cell cell, int x, int y)? NextCell()
		{
			var priority = cells.Indexed()
				.Where(it => !it.cell.IsCollapsed)
				.Where(it => !it.cell.IsExhausted)
				.GroupBy(it => it.cell.Entropy)
				.OrderBy(it => it.Key);
			return priority.FirstOrDefault()?.ElementAtRandom(random);
		}

		void Refurbish(Cell cell, int x, int y)
		{
			cell.Reset(tiles);
			foreach (var (neighbour, dx, dy) in cells.Neighbouring(x, y))
				neighbour.Reset(tiles);
		}

		void Propagate(int sub, int x, int y)
		{
			var neighbours = cells.Neighbouring(x, y);
			///.Where(it => it.cell.IsCollapsed);

			foreach (var (cell, dx, dy) in neighbours)
				cell.Reduce(RemainingPatterns(sub, dx, dy)); // SuperState.IntersectWith(RemainingPatterns(obj: sub, dx: -dx, dy: -dy));

			var exhausted = neighbours
				.Where(it => it.cell.IsExhausted);

			//foreach (var (cell, dx, dy) in exhausted)
			//	Refurbish(cell, x + dx, y + dy);
		}

		public bool CollapseNext()
		{
			if (NextCell() is (Cell cell, int x, int y))
			{
				Debug.WriteLine($"Collapsing index: [{x}, {y}], cell: ${cell}");
				var state = cell.Collapse(random, weightSelector);
				Debug.WriteLine($"Propagating from index: [{x}, {y}], state: {state}");
				Propagate(state, x, y);
				return true;
			}
			return false;
		}

		IEnumerable<int> RemainingPatterns(int subject, int dx, int dy)
		{
			var remaining = patterns
				.Where(it => it.DeltaX == dx && it.DeltaY == dy)
				.Where(it => it.Subject == subject)
				.Select(it => it.Object);
			Debug.WriteLine($"Remaining Objects for delta: [{dx}, {dy}], {{ {remaining.JoinToString(", ")} }}");
			return remaining;
		}
	}
}
