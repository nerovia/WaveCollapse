using WaveLib;
using WaveLib.Parser;

Console.ForegroundColor = ConsoleColor.White;
Console.BackgroundColor = ConsoleColor.Black;

var random = new Random(Environment.TickCount);
var palette = Enum.GetValues<ConsoleColor>().Except([ConsoleColor.Black, ConsoleColor.White, ConsoleColor.Gray]).ToArray();
var changes = new List<GridPosition<Cell>>();

var (schema, tileSet) = await ReadSchema(args[0]);
var synthesizer = new WaveSynthesizer(Console.WindowWidth, Console.WindowHeight, schema, random);

Console.WriteLine("RULES");
foreach (var constraint in schema.Order())
	Console.WriteLine(constraint);

while (true)
{
	Console.CursorVisible = true;
	Console.ReadLine();
	Console.Clear();
	Console.CursorVisible = false;

	random.Shuffle(palette);
	synthesizer.Reset();
	while (synthesizer.CollapseNext(changes))
	{
		Draw(changes);
		changes.Clear();
	}
}

void Draw(IEnumerable<GridPosition<Cell>> changes)
{
	var foreground = Console.ForegroundColor;
	var background = Console.BackgroundColor;
	foreach (var (x, y, cell) in changes)
	{
		Console.SetCursorPosition(x, y);
		Console.ForegroundColor = cell.State switch
		{
			CellState.SuperPosition => palette[^cell.SuperPosition.Count],
			CellState.Collapsed => ConsoleColor.White,
			_ => ConsoleColor.Black
		};
		Console.BackgroundColor = cell.State switch
		{
			CellState.Collapsed => palette![cell.Position],
			_ => ConsoleColor.Black
		};
		Console.Write(cell.State switch 
		{ 
			CellState.Collapsed => tileSet![cell.Position],
			_ => (char)('0' + cell.SuperPosition.Count)
		});
	}
	Console.ForegroundColor = foreground;
	Console.BackgroundColor = background;
}

async Task<(WaveSchema, char[])> ReadSchema(string path)
{
	using var file = File.OpenRead(path);
	switch (Path.GetExtension(path))
	{
		case ".txt":
			return WaveSchema.Analyze(await GridReader.Read(file), WaveSchema.Stencil.Plus);
		case ".schema":
			var (schema, tileSet) = WaveSchema.Parse(file);
			return (schema, tileSet.Select(s => s.First()).ToArray());
		case ".rulz":
			return WaveRulz.Parse(file);
	}
	throw new Exception();
}