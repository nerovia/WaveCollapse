using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveLib;

namespace WaveTest
{
	[TestClass]
	public class BitSetTest
	{
		[TestMethod]
		public void TestSetEquals()
		{
			int[] items = [1, 2, 3, 4];
			var set = new BitSet32(items);
			Assert.IsTrue(set.SetEquals(items));
		}

		[TestMethod]
		public void TestRange()
		{
			var a = new BitSet32();
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new BitSet32() { -1 });
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => new BitSet32() { 32 });
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => a.Add(-1));
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => a.Add(32));
		}

		[TestMethod]
		public void TestBasics()
		{
			var set = new BitSet32() { 1 };
			Assert.IsTrue(set.SetEquals([1]));
			Assert.AreEqual(1, set.Count);
			set.Remove(1);
			Assert.IsTrue(set.SetEquals([]));
			Assert.AreEqual(0, set.Count);
		}

		[TestMethod]
		public void TestEnumerator()
		{
			int[] a = [1, 2, 3, 4, 5];
			var set = new BitSet32(a);
			Assert.IsTrue(set.AsEnumerable().SequenceEqual(a));
			var arr = set.ToArray();
			Assert.IsTrue(arr.SequenceEqual(a));
		}

		[TestMethod]
		public void TestUnion()
		{
			var a = new BitSet32([1, 2, 3, 4]);
			a.UnionWith([3, 4, 5, 6]);
			Assert.IsTrue(a.SequenceEqual([1, 2, 3, 4, 5, 6]));
		}

		[TestMethod]
		public void TestIntersect()
		{
			var a = new BitSet32([1, 2, 3, 4]);
			a.IntersectWith([3, 4, 5, 6]);
			Assert.IsTrue(a.SequenceEqual([3, 4]));
		}

		[TestMethod]
		public void TestExcept()
		{
			var a = new BitSet32([1, 2, 3, 4]);
			a.ExceptWith([3, 4, 5, 6]);
			Assert.IsTrue(a.SequenceEqual([1, 2]));
		}

		[TestMethod]
		public void TestSubset()
		{
			var a = new BitSet32([1, 2]);
			Assert.IsTrue(a.IsSubsetOf([1, 2, 3, 4]));
			Assert.IsTrue(a.IsSubsetOf([1, 2]));
			Assert.IsTrue(a.IsProperSubsetOf([1, 2, 3, 4]));
			Assert.IsFalse(a.IsProperSubsetOf([1, 2]));
		}

		[TestMethod]
		public void TestSuperset()
		{
			var a = new BitSet32([1, 2, 3, 4]);
			Assert.IsTrue(a.IsSupersetOf([1, 2, 3, 4]));
			Assert.IsTrue(a.IsSupersetOf([1, 2]));
			Assert.IsFalse(a.IsProperSupersetOf([1, 2, 3, 4]));
			Assert.IsTrue(a.IsProperSupersetOf([1, 2]));
		}
	}
}
