using SadConsole.UI;
using WaveLib;

namespace SadWave.Scenes
{
	internal class RootScreen : ControlsConsole
	{
		

		public RootScreen() : base(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
		{
			//var cells = new int[5, 5] // [height, width]
			//{
			//	{ '#', '#', '#', '#', '#' },
			//	{ '#', '+', '+', '+', '#' },
			//	{ '#', '+', '.', '+', '#' },
			//	{ '#', '+', '+', '+', '#' },
			//	{ '#', '#', '#', '#', '#' }
			//};

			//var grid = Grid.FromArray(cells);
			//var analyzer = new WaveAnalyzer();

			//var rules = analyzer.Analyze(grid);

			//var tiles = grid
			//	.GroupBy(it => it)
			//	.ToDictionary(it => it.Key, it => it.Count());


			//Console.WriteLine("RULES");
			//foreach (var rule in rules)
			//{
			//	Console.WriteLine($"{rule.Key}: {rule.Value}");
			//}
			//Console.WriteLine("TILES");
			//foreach (var (tileId, weight) in tiles)
			//{
			//	Console.WriteLine($"'{(char)tileId}': {weight}");
			//}

			//Console.ReadLine();

			//var grid2 = new Grid<Cell>(40, 20, (x, y) => new Cell());
			//var random = new Random(Environment.TickCount);
			//var wavePropagator = new WaveSynthesizer(random, grid2, rules.Keys, tiles.Keys, it => tiles[it]);

		}
	}
}
