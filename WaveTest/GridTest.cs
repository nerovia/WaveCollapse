using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System.Diagnostics;
using System.Linq;
using WaveLib;

namespace WaveTest
{
	[TestClass]
	public class GridTest
	{
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void TestTraverser()
		{
			// Evaluate the Enumerable twice
			var traversal = Enumerable.Range(0, 100);
			var values = traversal.ToList();
			Assert.IsTrue(values.SequenceEqual(traversal));
		}

		[TestMethod]
		public void TestTraversal()
		{
			Assert.IsTrue(CheckTraversal(Grid.Create<int>(5, 5)));
			Assert.IsTrue(CheckTraversal(Grid.Create<int>(5, 10)));
			Assert.IsTrue(CheckTraversal(Grid.Create<int>(10, 5)));
		}

		[TestMethod]
		public void TestRangeTraversal()
		{
			var grid = Grid.Create<int>(5, 5);
			Assert.IsTrue(CheckTraversal(grid, GridRange.FromRadius(2, 2, 1)));
			Assert.IsTrue(CheckTraversal(grid, GridRange.FromRadius(0, 0, 2)));
			Assert.IsTrue(CheckTraversal(grid, new(3..6, 3..6)));
		}

		bool CheckTraversal<T>(IGrid<T> grid) => CheckTraversal(grid, GridRange.All);

		bool CheckTraversal<T>(IGrid<T> grid, GridRange range)
		{
			var (x0, xn, y0, yn) = range.GetIndices(grid.Width, grid.Height);
			var values = new List<GridPosition<T>>();
			for (int j = y0; j < yn; j++)
				for (int i = x0; i < xn; i++)
					values.Add(new(i, j, grid[i, j]));
			var traversal = grid.TraverseRange(range);
			var intersect = traversal.Except(values);

			TestContext.WriteLine($"Check Traversal ({range}):");
			TestContext.WriteLine($"[ {string.Join(", ", traversal)} ]");
			TestContext.WriteLine($"[ {string.Join(", ", values)   } ]");
			TestContext.WriteLine($"[ {string.Join(", ", intersect)} ]");
			TestContext.WriteLine("");

			return intersect.Count() == 0;
		}

	}
}