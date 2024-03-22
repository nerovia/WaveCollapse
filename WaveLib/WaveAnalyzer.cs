namespace WaveLib
{
	public class WaveAnalyzer()
	{
		public Dictionary<Pattern, int> Analyze(IGrid<int> grid)
		{
			//var rules = new Dictionary<Rule, int>();
			var rules = new Dictionary<Pattern, int>();

			foreach (var (sub, x, y) in grid.Indexed())
			{
				foreach (var (obj, dx, dy) in grid.Neighbouring(x, y))
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
