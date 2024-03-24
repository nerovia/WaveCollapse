using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using WaveLib;

namespace WaveLib
{
	public class WaveSynthesizer(int width, int height, WaveSchema schema, Random random)
	{
		public readonly WaveSchema Schema = schema;
		public readonly Random Random = random;
		public readonly Func<int, int> WeightSelector = id => 1;
		public IGrid<Cell> Grid { get; } = WaveLib.Grid.Create<Cell>(width, height, _ => new());
		public List<GridPosition<Cell>> Changes { get; } = [];

		public void Reset()
		{
			Changes.Clear();
			foreach (var cell in Grid)
				cell.Reset(Schema.TileIds);
		}
		
		public bool TryNextCell(out GridPosition<Cell> pos)
		{
			var priority = Grid.Traverse()
				.Where(it => !it.Item.IsCollapsed)
				.Where(it => !it.Item.IsExhausted)
				.GroupBy(it => it.Item.Entropy)
				.OrderBy(it => it.Key);

			if (priority.Any())
			{
				pos = priority.First().ElementAtRandom(Random);
				return true;
			}
			else
			{
				pos = default;
				return false;
			}
		}

		void Propagate(int sub, int x, int y)
		{
			var neighbours = Grid.TraverseOffsets(x, y, Schema.Offsets);

			foreach (var ((dx, dy), pos) in neighbours)
			{
				pos.Item.Reduce(RemainingPatterns(sub, dx, dy));
				Changes.Add(pos);
			}
		}

		public void Collapse(int x, int y) => Collapse(Grid.At(x, y));

		void Collapse(GridPosition<Cell> pos)
		{
			Changes.Add(pos);
			var state = pos.Item.Collapse(Random, WeightSelector);
			Propagate(state, pos.X, pos.Y);
		}

		public bool CollapseNext()
		{
			if (TryNextCell(out var pos))
			{
				Changes.Clear();
				Collapse(pos);
				return true;
			}
			return false;
		}

		IEnumerable<int> RemainingPatterns(int subId, int dx, int dy)
		{
			var remaining = Schema.Patterns
				.Where(it => it.Satisfies(subId, dx, dy))
				.Select(it => it.ObjectId);
			return remaining;
		}
	}
}
