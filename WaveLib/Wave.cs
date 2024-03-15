using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;
using WaveLib;

namespace WaveLib
{

	public class WaveAnalyzer()
	{
		public IEnumerable<Rule> Analyze(IGrid<int> grid)
		{
			//var rules = new Dictionary<Rule, int>();
			var rules = new SortedSet<Rule>();

			foreach (var (subject, x, y) in grid.Indexed())
			{
				foreach (var (@object, dx, dy) in grid.Neighbouring(x, y))
				{
					rules.Add(new Rule(subject, @object, dx, dy));
					//var rule = new Rule(cell, neightbour, dx, dy);
					//if (!rules.TryAdd(rule, 1))
					//	rules[rule]++;
				}
			}

			return rules;
		}
	}

	public class WaveSynthezizer(Dictionary<Rule, double> rules)
	{
		
	}

	public class WavePropagator(Random random, IGrid<Cell> cells, IEnumerable<Rule> rules, IEnumerable<int> tiles, Func<int, int> weightSelector)
	{
		public void Reset()
		{
			foreach (var cell in cells)
			{
				cell.SuperState.Clear();
				cell.SuperState.UnionWith(tiles);
				cell.IsCollapsed = false;
			}
		}
		
		public (Cell cell, int x, int y)? NextCell()
		{
			//if (cells.All(it => it.IsCollapsed))
			//	return null;

			var priority = cells.Indexed()
				.Where(it => !it.cell.IsCollapsed)
				.GroupBy(it => it.cell.SuperState.Count)
				.OrderBy(it => it.Key);

			// Collapse
			return priority.FirstOrDefault()?.ElementAtRandom(random);
		}

		int Collapse(Cell cell)
		{
			cell.IsCollapsed = true;
			var state = cell.SuperState.ElementAtRandom(random);
			cell.SuperState.Clear();
			cell.SuperState.Add(state);
			return state;
		}

		void Refurbish(Cell cell, int x, int y)
		{
			cell.SuperState.UnionWith(tiles);
			cell.IsCollapsed = false;
			foreach (var (neighbour, dx, dy) in cells.Neighbouring(x, y))
			{
				neighbour.SuperState.UnionWith(tiles);
				neighbour .IsCollapsed = false;
			}
		}

		void Propagate(int state, int x, int y)
		{
			var neighbours = cells.Neighbouring(x, y);
				///.Where(it => it.cell.IsCollapsed);

			foreach (var (cell, dx, dy) in neighbours)
				cell.SuperState.IntersectWith(RemainingObjects(state, dx, dy));

			var exhausted = neighbours
				.Where(it => it.cell.SuperState.Count == 0);

			foreach (var (cell, dx, dy) in exhausted)
				Refurbish(cell, x + dx, y + dy);
		}

		public bool CollapseNext()
		{
			if (NextCell() is (Cell cell, int x, int y))
			{
				Debug.WriteLine($"Collapsing index: [{x}, {y}], cell: ${cell}");
				var state = Collapse(cell);
				Debug.WriteLine($"Propagating from index: [{x}, {y}], state: {state}");
				Propagate(state, x, y);
				return true;
			}
			return false;
		}

		IEnumerable<int> RemainingObjects(int subject, int dx, int dy)
		{
			var remaining = rules
				.Where(it => it.DeltaX == dx && it.DeltaY == dy)
				.Where(it => it.Subject == subject)
				.Select(it => it.Object);
			Debug.WriteLine($"Remaining Objects for delta: [{dx}, {dy}], {{ {remaining.JoinToString(", ")} }}");
			return remaining;
		}
	}

	public record Rule(int Subject, int Object, int DeltaX, int DeltaY) : IComparable<Rule>
	{
		public int CompareTo(Rule? other)
		{
			if (other == null)
				return -1;

			if (other == this) 
				return 0;

			int result;

			result = Subject.CompareTo(other.Subject);
			if (result != 0)
				return result;

			result = Object.CompareTo(other.Object);
			if (result != 0)
				return result;

			result = DeltaX.CompareTo(other.DeltaX);
			if (result != 0)
				return result;

			return DeltaY.CompareTo(other.DeltaY);
		}

		public override string ToString()
		{
			var relation = (DeltaX, DeltaY) switch
			{
				(1, 0) => "right",
				(-1, 0) => "left ",
				(0, 1) => "above",
				(0, -1) => "below",
				_ => "?",
			};

			return $"'{(char)Subject}' [ {relation} ] '{(char)Object}'";
		}
	}

	public class Cell
	{
		public ISet<int> SuperState { get; } = new HashSet<int>();
		public int State { get => SuperState.Single(); }
		public bool IsCollapsed { get; set; }
	}

	public static class Extensions
	{
		public static T ElementAtRandom<T>(this IEnumerable<T> sequence, Random random)
		{
			return sequence.ElementAt(random.Next(0, sequence.Count()));
		}

		public static T ElementAtRandom<T>(this IEnumerable<T> sequence, Random random, Func<T, int> weightSelector)
		{
			var totalWeight = sequence.Sum(it => weightSelector(it));
			var rand = random.Next(0, totalWeight);
			var acc = 0;
			foreach (var item in sequence)
			{
				acc += weightSelector(item);
				if (rand <= acc)
					return item;
			}
			throw new Exception();
		}

		public static string JoinToString<T>(this IEnumerable<T> sequence, string separator)
		{
			return JoinToString(sequence, separator, it => it.ToString());
		}

		public static string JoinToString<T>(this IEnumerable<T> sequence, string separator, Func<T, string> stringSelector)
		{
			return string.Join(separator, sequence.Select(stringSelector));
		}

	}
}
