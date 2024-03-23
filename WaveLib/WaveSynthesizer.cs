using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using WaveLib;

namespace WaveLib
{
	public class WaveSynthesizer(int width, int height, WaveSchema schema, Random random)
	{
		Func<int, int> weightSelector = id => 1;
		public IGrid<Cell> Grid { get; } = WaveLib.Grid.Create<Cell>(width, height, _ => new());
		public List<GridPosition<Cell>> Changes { get; } = new();

		public void Reset()
		{
			foreach (var cell in Grid)
				cell.Reset(schema.TileIds);
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

		void Propagate(int sub, int x, int y)
		{
			var neighbours = Grid.TraverseOffsets(x, y, schema.Offsets);

			foreach (var (dx, dy, cell) in neighbours)
			{
				cell.Reduce(RemainingPatterns(sub, dx, dy));
				Changes.Add(new(dx + x, dy + y, cell));
			}
		}

		public bool CollapseNext()
		{
			if (TryNextCell(out var pos))
			{
				Changes.Clear();
				Changes.Add(pos);
				var (x, y, cell) = pos;
				var state = cell.Collapse(random, weightSelector);
				Propagate(state, x, y);
				return true;
			}
			return false;
		}

		IEnumerable<int> RemainingPatterns(int subId, int dx, int dy)
		{
			var remaining = schema.Patterns
				.Where(it => it.Satisfies(subId, dx, dy))
				.Select(it => it.ObjectId);
			return remaining;
		}
	}
}
