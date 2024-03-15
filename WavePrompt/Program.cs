using WaveLib;


var analyzer = new WaveAnalyzer();

var cells = new int[5, 5] // [height, width]
{
	{ 'a', 'a', 'a', '+', '+' },
	{ 'a', 'a', '+', '+', '.' },
	{ 'a', 'a', '+', '.', '.' },
	{ 'a', 'a', '+', '+', '.' },
	{ 'a', 'a', 'a', 'a', '+' }
};

var grid = Grid.FromArray(cells);

var rules = analyzer.Analyze(grid);

var tiles = rules.Select(it => it.Subject).Distinct().ToList();

foreach (var rule in rules)
{
	Console.WriteLine($"{rule}");
}

Console.ReadLine(); 

var grid2 = new Grid<Cell>(40, 20, (x, y) => new Cell());
var random = new Random(Environment.TickCount);
var wavePropagator = new WavePropagator(random, grid2, rules, tiles, it => 1);

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
			var glyph = cell.IsCollapsed ? (char)cell.State: (char)(cell.SuperState.Count + '0');
			var colorIdx = (cell.IsCollapsed ? cell.State: cell.SuperState.Count) % 7 + 31;
			Console.Write($"\x1b[{colorIdx}m{glyph}");
		}
		Console.WriteLine();
	}
}
