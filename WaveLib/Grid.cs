﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WaveLib
{
	public record struct GridPosition<T>(int X, int Y, T Item);

	public interface IGrid<T> : IEnumerable<T>
	{
		int Width { get; }
		int Height { get; }
		ref T this[int x, int y] { get; }
	}

	public static class Grid
	{
		private class ArrayGrid<T>(int width, int height) : IGrid<T>
		{
			public T[] Data { get; } = new T[width * height];
			public int Width { get; } = width;
			public int Height { get; } = height;
			public ref T this[int x, int y] { get => ref Data[x + y * Width]; }

			public IEnumerator<T> GetEnumerator() => Data.AsEnumerable().GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();
		}

		public static IGrid<T> Create<T>(int width, int height) => new ArrayGrid<T>(width, height);

		public static IGrid<T> Create<T>(int width, int height, Func<GridPosition<T>, T> initializer) => Create<T>(width, height).Fill(initializer);

		public static IGrid<T> From<T>(T[,] data) => Create<T>(data.GetLength(1), data.GetLength(0)).Fill(pos => data[pos.Y, pos.X]);

		public static GridPosition<T> At<T>(this IGrid<T> self, int x, int y) => new(x, y, self[x, y]);

		public static IGrid<T> Fill<T>(this IGrid<T> self, T value) => self.Fill(_ => value);

		public static IGrid<T> Fill<T>(this IGrid<T> self, Func<GridPosition<T>, T> selector)
		{
			for (int y = 0; y < self.Height; y++)
				for (int x = 0; x < self.Width; x++)
					self[x, y] = selector(self.At(x, y));
			return self;
		}

		public static IGrid<TResult> Convert<T, TResult>(this IGrid<T> self, Func<T, TResult> converter)
		{
			return Create<TResult>(self.Width, self.Height).Fill(pos => converter(self[pos.X, pos.Y]));
		}

		public static IEnumerable<GridPosition<T>> Traverse<T>(this IGrid<T> self) => new GridTraversal<T>(self, GridRange.All);

		public static IEnumerable<GridPosition<T>> TraverseRange<T>(this IGrid<T> self, GridRange range) => new GridTraversal<T>(self, range);

		public static IEnumerable<GridPosition<T>> TraverseOffsets<T>(this IGrid<T> self, int x, int y, IEnumerable<(int x, int y)> offsets) => new GridOffsetTraversal<T>(self, x, y, offsets);
	}

	public record struct GridRange(Range RangeX, Range RangeY)
	{
		public static GridRange All => new(Range.All, Range.All);

		public static GridRange FromRadius(int x, int y, int r)
		{
			var x0 = Math.Max(x - r, 0);
			var xn = x + r + 1;
			var y0 = Math.Max(y - r, 0);
			var yn = y + r + 1;
			return new GridRange(x0..xn, y0..yn);
		}
	
		public readonly (int x0, int xn, int y0, int yn) GetIndices(int width, int height)
		{
			var x0 = RangeX.Start.GetOffset(width);
			var xn = RangeX.End.GetOffset(width);
			var y0 = RangeY.Start.GetOffset(height);
			var yn = RangeY.End.GetOffset(height);
			return (Math.Max(x0, 0), Math.Min(xn, width), Math.Max(y0, 0), Math.Min(yn, height));
		}
	}

	class GridTraversal<T>(IGrid<T> grid, GridRange range) : IEnumerable<GridPosition<T>>
	{
		(int x0, int xn, int y0, int yn) indices = range.GetIndices(grid.Width, grid.Height);

		public IEnumerator<GridPosition<T>> GetEnumerator() => new GridTraverser<T>(grid, indices.x0, indices.xn, indices.y0, indices.yn);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	class GridTraverser<T>(IGrid<T> grid, int x0, int xn, int y0, int yn) : IEnumerator<GridPosition<T>>
	{
		int xi = x0 - 1;
		int yi = y0;

		public GridPosition<T> Current => grid.At(xi, yi);

		object? IEnumerator.Current => Current;

		public void Dispose() { }

		public bool MoveNext()
		{
			if (++xi >= xn)
			{
				if (++yi >= yn)
				{
					xi = xn;
					yi = yn;
					return false;
				}
				xi = x0;
			}
			return true;
		}

		public void Reset()
		{
			xi = x0 - 1;
			yi = y0;
		}
	}

	class GridOffsetTraversal<T>(IGrid<T> grid, int x, int y, IEnumerable<(int dx, int dy)> offsets) : IEnumerable<GridPosition<T>>
	{
		public IEnumerator<GridPosition<T>> GetEnumerator() => new GridOffsetTraverser<T>(grid, x, y, offsets);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	class GridOffsetTraverser<T>(IGrid<T> grid, int x, int y, IEnumerable<(int dx, int dy)> offsets) : IEnumerator<GridPosition<T>>
	{
		readonly IEnumerator<(int dx, int dy)> offsetEnumerator = offsets.GetEnumerator();

		int dx => offsetEnumerator.Current.dx;
		int dy => offsetEnumerator.Current.dy;
		int i => x + dx;
		int j => y + dy;

		public GridPosition<T> Current => new(dx, dy, grid[i, j]);

		object IEnumerator.Current => Current;

		public void Dispose() { }

		public bool MoveNext()
		{
			do
			{
				if (!offsetEnumerator.MoveNext())
					return false;
			} 
			while (i < 0 || j < 0 || i >= grid.Width || j >= grid.Height);
			return true;
		}

		public void Reset()
		{
			offsetEnumerator.Reset();
		}
	}
}
