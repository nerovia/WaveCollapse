﻿namespace WaveLib
{
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
				if (rand < acc)
					return item;
			}
			throw new Exception();
		}
	}

	public static class Util
	{
		public static int Mod(int n, int m)
		{
			return (n %= m) < 0 ? n + m : n;
		}
	}
}
