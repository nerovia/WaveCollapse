using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;

namespace WaveLib
{
	public record Pattern(int Subject, int Object, int DeltaX, int DeltaY) : IComparable<Pattern>
	{
		public static (int, int)[] Offsets = [(1, 0), (-1, 0), (0, 1), (0, -1)];

		public int CompareTo(Pattern? other)
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
				_ =>       " ??? ",
			};

			return $"'{(char)Subject}' [ {relation} ] '{(char)Object}'";
		}
	}
}
