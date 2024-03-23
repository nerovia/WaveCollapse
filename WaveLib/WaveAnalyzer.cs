namespace WaveLib
{
	public class WaveAnalyzer()
	{
		public static Dictionary<Pattern, int> Analyze(IGrid<int> grid)
		{
			//var rules = new Dictionary<Rule, int>();
			var rules = new Dictionary<Pattern, int>();

			foreach (var (x, y, sub) in grid.Traverse())
			{
				foreach (var (dx, dy, obj) in grid.TraverseOffsets(x, y, Pattern.Offsets))
				{
					// rules.Add(new Rule(subject, @object, dx, dy));
					var rule = new Pattern(sub, obj, dx, dy);
					if (!rules.TryAdd(rule, 1))
						rules[rule]++;
				}
			}

			return rules;
		}
	}
}
