using WaveLib;

var cells = new int[5, 5] // [height, width]
{
	{ '#', '#', '#', '#', '#' },
	{ '#', '+', '+', '+', '#' },
	{ '#', '+', '.', '+', '#' },
	{ '#', '+', '+', '+', '#' },
	{ '#', '#', '#', '#', '#' }
};

var (schema, tileSet) = WaveSchema.Analyze(Grid.From(cells), WaveSchema.Stencil.Plus);
var synthesizer = new WaveSynthesizer(40, 20, schema, new Random(Environment.TickCount));

Console.WriteLine("RULES");
foreach (var pattern in schema.Patterns)
	Console.WriteLine(pattern);

Console.ReadLine();
Console.Clear();

while (true)
{
	synthesizer.Reset();
	Console.CursorVisible = false;
	while (synthesizer.CollapseNext())
		DrawGrid();
	Console.ReadLine();
}

void DrawGrid()
{
	var grid = synthesizer.Grid;


	Console.SetCursorPosition(0, 0);
	for (int y = 0; y < grid.Height; y++)
	{
		for (int x = 0; x < grid.Width; x++)
		{
			var cell = grid[x, y];
			var glyph = cell.IsCollapsed ? tileSet[cell.StateId]: (char)(cell.Entropy + '0');
			var colorIdx = (cell.IsCollapsed ? cell.StateId: cell.Entropy) % 7 + 31;
			Console.Write($"\x1b[{colorIdx}m{glyph}");
		}
		Console.WriteLine();
	}
}
