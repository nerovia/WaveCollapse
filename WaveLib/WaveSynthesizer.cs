using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using WaveLib;

namespace WaveLib
{
	public class WaveSynthesizer(int width, int height, Pattern[] patterns, Random random)
	{
		Func<int, int> weightSelector = id => 1;
		ISet<int> subjectIds = new BitSet32(patterns.Select(it => it.Subject).Distinct());
		public IGrid<Cell> Grid { get; } = WaveLib.Grid.Create<Cell>(width, height, _ => new());

		public void Reset()
		{
			foreach (var cell in Grid)
				cell.Reset(subjectIds);
		}
		
		public bool TryNextCell(out GridPosition<Cell> pos)
		{
			var priority = Grid.Traverse()
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
			pos.Item.Reset(subjectIds);
			foreach (var (_, _, neighbor) in Grid.TraverseOffsets(pos.X, pos.Y, Pattern.Offsets))
				neighbor.Reset(subjectIds);
		}

		void Propagate(int sub, int x, int y)
		{
			var neighbours = Grid.TraverseOffsets(x, y, Pattern.Offsets);
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
				//Debug.WriteLine($"Collapsing index: [{x}, {y}], cell: ${cell}");
				var state = cell.Collapse(random, weightSelector);
				//Debug.WriteLine($"Propagating from index: [{x}, {y}], state: {state}");
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
			//Debug.WriteLine($"Remaining Objects for delta: [{dx}, {dy}], {{ {remaining.JoinToString(", ")} }}");
			return remaining;
		}
	}
}
