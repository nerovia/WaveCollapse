using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveLib
{
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

	public static class ExtensionsGrid
	{
		public static IEnumerable<(T cell, int dx, int dy)> Neighbouring<T>(this IGrid<T> grid, int x, int y)
		{
			foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
				if (grid.ContainsIndex(x + dx, y + dy))
					yield return (grid[x + dx, y + dy], dx, dy);
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
