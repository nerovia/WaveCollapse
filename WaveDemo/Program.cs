using WaveLib;

var a = PrimitiveGrid.Create<string>(5, 5).Fill(pos => $"{pos.x}{pos.y}");


Console.WriteLine(string.Join("\n", a.Enumerate(0..1, 0..1)));





return;

var cells = new int[5, 5] // [height, width]
{
	{ '#', '#', '#', '#', '#' },
	{ '#', '+', '+', '+', '#' },
	{ '#', '+', '.', '+', '#' },
	{ '#', '+', '+', '+', '#' },
	{ '#', '#', '#', '#', '#' }
};

var grid = Grid.FromArray(cells);
var analyzer = new WaveAnalyzer();

var rules = analyzer.Analyze(grid);

var tiles = grid
	.GroupBy(it => it)
	.ToDictionary(it => it.Key, it => it.Count());


Console.WriteLine("RULES");
foreach (var rule in rules)
{
	Console.WriteLine($"{rule.Key}: {rule.Value}");
}
Console.WriteLine("TILES");
foreach (var (tileId, weight) in tiles)
{
	Console.WriteLine($"'{(char)tileId}': {weight}");
}

Console.ReadLine(); 

var grid2 = new Grid<Cell>(40, 20, (x, y) => new Cell());
var random = new Random(Environment.TickCount);
var wavePropagator = new WaveSynthesizer(random, grid2, rules.Keys, tiles.Keys, it => tiles[it]);

Console.Clear();

while (true)
{
	wavePropagator.Reset();
	Console.CursorVisible = false;
	while (wavePropagator.CollapseNext())
	{
		PrintGrid(grid2);
		//Thread.Sleep(10);
	}
	Console.ReadLine();
}

void PrintGrid(IGrid<Cell> grid)
{
	Console.SetCursorPosition(0, 0);
	for (int y = 0; y < grid.Height; y++)
	{
		for (int x = 0; x < grid.Width; x++)
		{
			var cell = grid[x, y];
			var glyph = cell.IsCollapsed ? (char)cell.State: (char)(cell.Entropy + '0');
			var colorIdx = (cell.IsCollapsed ? cell.State: cell.Entropy) % 7 + 31;
			Console.Write($"\x1b[{colorIdx}m{glyph}");
		}
		Console.WriteLine();
	}
}
