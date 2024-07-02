namespace WaveLib
{
	public record Constraint(int SubjectId, int ObjectId, GridOffset Delta) : IComparable<Constraint>
	{
		public int Weight { get; set; }

		public int CompareTo(Constraint? other)
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

			return Delta.CompareTo(other.Delta);
		}

		public override string ToString()
		{
			return $"#{SubjectId} -> #{ObjectId} : [{Delta.X,2},{Delta.Y,2}]";
		}
	}
}
