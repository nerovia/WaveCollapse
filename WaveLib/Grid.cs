using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveLib
{
	/// <summary>
	/// An abstraction for accessing indicies in of a grid.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IGrid<T> : IEnumerable<T>
	{
		int Width { get; }
		int Height { get; }
		T this[int x, int y] { get; }
	}

	public class Grid<T>(int width, int height) : IGrid<T>
	{
		public Grid(int width, int height, Func<int, T> init) : this(width, height)
		{
			for (int i = 0; i < Cells.Length; i++)
				Cells[i] = init(i);
		}

		public Grid(int width, int height, Func<int, int, T> init) : this(width, height)
		{
			for (int i = 0, y = 0; y < Height; y++)
				for (int x = 0; x < Width; x++, i++)
					Cells[i] = init(x, y);
		}

		public T[] Cells { get; } = new T[width * height];
		public int Width { get; } = width;
		public int Height { get; } = height;

		public T this[int x, int y]
		{
			get => Cells[x + y * Width];
		}

		public IEnumerator<T> GetEnumerator() => Cells.AsEnumerable().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => Cells.GetEnumerator();
	}

	public static class Grid
	{
		public static Grid<T> FromArray<T>(T[,] values)
		{
			var grid = new Grid<T>(
				values.GetLength(1), 
				values.GetLength(0),
				(x, y) => values[y, x]);
			return grid;
		}
	}

	internal class SelectGrid<TSource, TResult>(IGrid<TSource> source, Func<TSource, TResult> selector) : IGrid<TResult>
	{
		public TResult this[int x, int y] => selector(source[x, y]);

		public int Width => source.Width;

		public int Height => source.Height;

		public IEnumerator<TResult> GetEnumerator() => source.Select(selector).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	class NeighbourEnumerator<T>(IGrid<T> grid, int x, int y) : IEnumerator<(T cell, int dx, int dy)>, IEnumerable<(T, int, int)>
	{
		const int MIN = -2;
		const int MAX = 2;

		int dx = MIN - 1;
		int dy = MIN;

		public (T, int, int) Current => (grid[x + dx, y + dy], dx, dy);

		object? IEnumerator.Current => Current;

		public void Dispose() { }

		public IEnumerator<(T, int, int)> GetEnumerator() => this;

		public bool MoveNext()
		{

			while (!grid.ContainsIndex(++dx + x, dy + y))
			{
				if (++dx > MAX)
				{
					dx = MIN;
					dy++;
				}
				
				if (dy > MAX)
					return false;
			}

			return true;
		}

		public void Reset()
		{
			dx = MIN;
			dy = MIN;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}

	public static class GridExtensions
	{
		public static IEnumerable<(T cell, int dx, int dy)> Neighbouring<T>(this IGrid<T> grid, int x, int y)
		{
			
			return new NeighbourEnumerator<T>(grid, x, y);
			//foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
			//	if (grid.ContainsInNdex(x + dx, y + dy))
			//		yield return (grid[x + dx, y + dy], dx, dy);
		}

		public static bool ContainsIndex<T>(this IGrid<T> grid, int x, int y)
		{
			return x >= 0 && y >= 0 && x < grid.Width && y < grid.Height;
		}

		public static IEnumerable<(T cell, int x, int y)> Indexed<T>(this IGrid<T> grid)
		{
			for (int y = 0; y < grid.Height; y++)
				for (int x = 0; x < grid.Width; x++)
					yield return (grid[x, y], x, y);
		}

		public static IGrid<TResult> Select<TSource, TResult>(this IGrid<TSource> grid, Func<TSource, TResult> selector)
		{
			return new SelectGrid<TSource, TResult>(grid, selector);
		}
	}
}
