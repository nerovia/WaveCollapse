using WaveLib;

var random = new Random(Environment.TickCount);
var palette = Enum.GetValues<ConsoleColor>().Except([ConsoleColor.Black]).ToArray();

var grid = await GridReader.Read(File.OpenRead(args[0]));
var (schema, tileSet) = WaveSchema.Analyze(grid, WaveSchema.Stencil.Plus);
var synthesizer = new WaveSynthesizer(Console.BufferWidth, Console.BufferHeight, schema, random);

Console.WriteLine("RULES");
foreach (var pattern in schema.Patterns)
	Console.WriteLine(pattern);

while (true)
{
	Console.CursorVisible = true;
	Console.ReadLine();
	Console.Clear();
	Console.CursorVisible = false;

	random.Shuffle(palette);
	synthesizer.Reset();
	while (synthesizer.CollapseNext())
	{
		var foreground = Console.ForegroundColor;
		var background = Console.BackgroundColor;
		foreach (var (x, y, cell) in synthesizer.Changes)
		{
			Console.SetCursorPosition(x, y);
			Console.ForegroundColor = cell.IsCollapsed ? palette[cell.TileId] : (cell.IsExhausted ? ConsoleColor.Black : palette[^cell.Entropy]);
			Console.BackgroundColor = cell.IsCollapsed ? Console.ForegroundColor : ConsoleColor.Black;
			Console.Write(cell.IsCollapsed ? tileSet[cell.TileId] : (char)('0' + cell.Entropy));
		}
		Console.ForegroundColor = foreground;
		Console.BackgroundColor = background;
	}
}
