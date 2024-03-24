using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;

namespace WaveLib
{
	public record Pattern(int SubjectId, int ObjectId, int DeltaX, int DeltaY) : IComparable<Pattern>
	{
		public int CompareTo(Pattern? other)
		{
			if (other == null)
				return 1;

			if (other == this) 
				return 0;

			int result;

			result = SubjectId.CompareTo(other.SubjectId);
			if (result != 0)
				return result;

			result = ObjectId.CompareTo(other.ObjectId);
			if (result != 0)
				return result;

			result = DeltaX.CompareTo(other.DeltaX);
			if (result != 0)
				return result;

			return DeltaY.CompareTo(other.DeltaY);
		}

		public bool Satisfies(int subId, int dx, int dy)
		{
			return (subId == SubjectId && dx == DeltaX && dy == DeltaY);
		}

		public override string ToString()
		{
			var tag = (DeltaX, DeltaY) switch
			{
				(1, 0) => "(r)",
				(-1, 0) => "(l)",
				(0, -1) => "(t)",
				(0, 1) => "(b)",
				_ => null,
			};

			return $"#{SubjectId} -> #{ObjectId} : [{DeltaX,2},{DeltaY,2}] {tag}";
		}
	}
}
