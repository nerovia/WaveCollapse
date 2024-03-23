using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveLib.Primitive
{
	public static class PrimitiveGrid
	{
		public static T[,] Create<T>(int width, int height) => new T[height, width];

		public static ref T At<T>(this T[,] self, int x, int y) => ref self[y, x];

		public static GridPosition<T> GetPosition<T>(this T[,] self, int x, int y) => new(x, y, self.At(x, y));

		public static int GetWidth<T>(this T[,] self) => self.GetLength(1);

		public static int GetHeight<T>(this T[,] self) => self.GetLength(0);

		public static T[,] Fill<T>(this T[,] self, T value) => self.Fill(_ => value);

		public static T[,] Fill<T>(this T[,] self, Func<GridPosition<T>, T> selector)
		{
			for (int y = 0; y < self.GetHeight(); y++)
				for (int x = 0; x < self.GetWidth(); x++)
					self.At(x, y) = selector(self.GetPosition(x, y));
			return self;
		}

		public static IEnumerable<GridPosition<T>> Enumerate<T>(this T[,] self) => new PrimitiveGridEnumerator<T>(self);

		public static IEnumerable<GridPosition<T>> Enumerate<T>(this T[,] self, Range x, Range y) => new PrimitiveGridEnumerator<T>(self, x, y);

		public static IEnumerable<GridPosition<T>> Enumerate<T>(this T[,] self, int x, int y, int r = 1) => new PrimitiveGridEnumerator<T>(self, x - r, x + r + 1, y - r, y + r + 1);
	}

	class PrimitiveGridEnumerator<T> : IEnumerator<GridPosition<T>>, IEnumerable<GridPosition<T>>
	{
		readonly T[,] self;
		readonly int x0;
		readonly int xn;
		readonly int y0;
		readonly int yn;
		int xi;
		int yi;

		public PrimitiveGridEnumerator(T[,] self, int x0, int xn, int y0, int yn)
		{
			this.self = self;
			this.x0 = Math.Max(x0, 0);
			this.y0 = Math.Max(y0, 0);
			this.xn = Math.Min(xn, self.GetWidth());
			this.yn = Math.Min(yn, self.GetHeight());
			this.xi = this.x0 - 1;
			this.yi = 0;
		}

		public PrimitiveGridEnumerator(T[,] self)
		{
			this.self = self;
			this.x0 = 0;
			this.y0 = 0;
			this.xn = self.GetWidth();
			this.yn = self.GetHeight();
			this.xi = -1;
			this.yi = 0;
		}

		public PrimitiveGridEnumerator(T[,] self, Range x, Range y)
		{
			var width = self.GetWidth();
			var height = self.GetHeight();

			this.self = self;
			this.x0 = Math.Max(x.Start.GetOffset(width), 0);
			this.y0 = Math.Max(x.End.GetOffset(width), 0);
			this.xn = Math.Min(y.Start.GetOffset(height), self.GetWidth());
			this.yn = Math.Min(y.End.GetOffset(height), self.GetHeight());
			this.xi = this.x0 - 1;
			this.yi = 0;
		}

		public GridPosition<T> Current => self.GetPosition(xi, yi);

		object? IEnumerator.Current => Current;

		public void Dispose() { }

		public IEnumerator<GridPosition<T>> GetEnumerator() => this;

		public bool MoveNext()
		{
			if (++xi >= xn)
			{
				if (++yi >= yn)
					return false;
				xi = -1;
			}
			return true;
		}

		public void Reset()
		{
			xi = x0 - 1;
			yi = y0;
		}

		IEnumerator IEnumerable.GetEnumerator() => this;
	}
}
