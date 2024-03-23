using MathNet.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace WaveLib
{
	public class BitSet32 : ISet<int>
	{
		const int MinValue = 0;
		const int MaxValue = 32;

		class Enumerator(uint map) : IEnumerator<int>
		{
			int idx = MinValue - 1;

			public int Current => idx;

			object IEnumerator.Current => Current;

			public void Dispose() { }

			public bool MoveNext()
			{
				do 
				{
					if (++idx >= MaxValue)
						return false;
				} 
				while (0u == (map & (1u << idx)));
				return true;
			}

			public void Reset()
			{
				idx = MinValue - 1;
			}
		}

		uint map;

		static BitSet32 From(IEnumerable<int> items)
		{
			if (items is BitSet32 set)
				return set;
			return new BitSet32(items);
		}

		public BitSet32()
		{
			map = 0;
		}

		public BitSet32(IEnumerable<int> items)
		{
			if (items is BitSet32 set)
			{
				map = set.map;
			}

			foreach (var item in items)
			{
				if (item < MinValue || item >= MaxValue)
					throw new ArgumentOutOfRangeException();
				map |= (1u << item);
			}
		}

		public int Count => BitOperations.PopCount(map);

		public bool IsReadOnly => false;

		public bool Add(int item)
		{
			if (Contains(item))
				return false;
			map |= (1u << (int)item);
			return true;
		}

		public void Clear()
		{
			map = 0u;
		}

		public bool Contains(int item)
		{
			if (item < MinValue || item >= MaxValue)
				throw new ArgumentOutOfRangeException();
			return 0u != (map & (1u << item));
		}

		public void CopyTo(int[] array, int arrayIndex)
		{
			var seq = GetEnumerator();
			while (seq.MoveNext() && arrayIndex < array.Length)
				array[arrayIndex++] = seq.Current;
		}

		public void ExceptWith(IEnumerable<int> other)
		{
			map &= ~From(other).map;
		}

		public IEnumerator<int> GetEnumerator() => new Enumerator(map);

		public void IntersectWith(IEnumerable<int> other)
		{
			map &= From(other).map;
		}

		public bool IsProperSubsetOf(IEnumerable<int> other)
		{
			var set = From(other);
			return map != set.map & 0u == (map & ~set.map);
		}

		public bool IsProperSupersetOf(IEnumerable<int> other)
		{
			var set = From(other);
			return map != set.map & 0u == (~map & set.map);
		}

		public bool IsSubsetOf(IEnumerable<int> other)
		{
			return 0u == (map & ~From(other).map);
		}

		public bool IsSupersetOf(IEnumerable<int> other)
		{
			return 0u == (~map & From(other).map);
		}

		public bool Overlaps(IEnumerable<int> other)
		{
			return 0u != (map | From(other).map);
		}

		public bool Remove(int item)
		{
			if (Contains(item))
			{
				map &= ~(1u << (int)item);
				return true;
			}
			return false;
		}

		public bool SetEquals(IEnumerable<int> other)
		{
			return map == From(other).map;
		}

		public void SymmetricExceptWith(IEnumerable<int> other)
		{
			throw new NotImplementedException();
		}

		public void UnionWith(IEnumerable<int> other)
		{
			map |= From(other).map;
		}

		void ICollection<int>.Add(int item) => Add(item);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
