using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using WaveLib;

namespace WaveLib
{
	public class WaveSynthesizer(Random random, IGrid<Cell> cells, IEnumerable<Pattern> patterns, IEnumerable<int> tiles, Func<int, int> weightSelector)
	{
		public IGrid<Cell> Grid { get; } = cells;

		public void Reset()
		{
			foreach (var cell in cells)
				cell.Reset(tiles);
		}
		
		public bool TryNextCell(out GridPosition<Cell> pos)
		{
			var priority = cells.Traverse()
				.Where(it => !it.Item.IsCollapsed)
				.Where(it => !it.Item.IsExhausted)
				.GroupBy(it => it.Item.Entropy)
				.OrderBy(it => it.Key);

			if (priority.Count() == 0)
			{
				pos = default;
				return false;
			}
			else
			{
				pos = priority.First().ElementAtRandom(random);
				return true;
			}
		}

		void Refurbish(GridPosition<Cell> pos)
		{
			pos.Item.Reset(tiles);
			foreach (var (_, _, neighbor) in cells.TraverseNeighbors(pos.X, pos.Y))
				neighbor.Reset(tiles);
		}

		void Propagate(int sub, int x, int y)
		{
			var neighbours = cells.TraverseNeighbors(x, y);
			///.Where(it => it.cell.IsCollapsed);

			foreach (var (dx, dy, cell) in neighbours)
				cell.Reduce(RemainingPatterns(sub, dx, dy)); // SuperState.IntersectWith(RemainingPatterns(obj: sub, dx: -dx, dy: -dy));

			var exhausted = neighbours
				.Where(pos => pos.Item.IsExhausted);

			//foreach (var (cell, dx, dy) in exhausted)
			//	Refurbish(cell, x + dx, y + dy);
		}

		public bool CollapseNext()
		{
			if (TryNextCell(out var pos))
			{
				var (x, y, cell) = pos;
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
