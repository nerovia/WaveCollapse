using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WaveCollapse
{

	class BitSet32() : ISet<int>
	{
		public BitSet32(IEnumerable<int> values) : this()
		{
			foreach (var value in values)
				Add(value);
		}

		uint _bitmap;

		public int Count => BitOperations.PopCount(_bitmap);

		public bool IsReadOnly => throw new NotImplementedException();

		public bool Add(int item)
		{
			if (item < 0 || item >= 32)
				throw new ArgumentOutOfRangeException();
			var flag = 1u << item;
			if ((_bitmap & flag) != 0u)
				return false;
			_bitmap |= flag;
			return true;
		}

		public void Clear()
		{
			_bitmap = 0;
		}

		public bool Contains(int item)
		{
			if (item < 0 || item >= 32)
				throw new ArgumentOutOfRangeException();
			return (_bitmap & (1u << item)) != 0;
		}

		public void CopyTo(int[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public void ExceptWith(IEnumerable<int> other)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<int> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public void IntersectWith(IEnumerable<int> other)
		{
			_bitmap &= new BitSet32(other)._bitmap;
		}

		public bool IsProperSubsetOf(IEnumerable<int> other)
		{
			throw new NotImplementedException();
		}

		public bool IsProperSupersetOf(IEnumerable<int> other)
		{
			throw new NotImplementedException();
		}

		public bool IsSubsetOf(IEnumerable<int> other)
		{
			throw new NotImplementedException();
		}

		public bool IsSupersetOf(IEnumerable<int> other)
		{
			throw new NotImplementedException();
		}

		public bool Overlaps(IEnumerable<int> other)
		{
			throw new NotImplementedException();
		}

		public bool Remove(int item)
		{
			throw new NotImplementedException();
		}

		public bool SetEquals(IEnumerable<int> other)
		{
			throw new NotImplementedException();
		}

		public void SymmetricExceptWith(IEnumerable<int> other)
		{
			throw new NotImplementedException();
		}

		public void UnionWith(IEnumerable<int> other)
		{
			throw new NotImplementedException();
		}

		void ICollection<int>.Add(int item)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}

}
