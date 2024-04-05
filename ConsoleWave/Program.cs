using WaveLib;
using WaveLib.Parser;

var random = new Random(Environment.TickCount);
var palette = Enum.GetValues<ConsoleColor>().Except([ConsoleColor.Black, ConsoleColor.White, ConsoleColor.Gray]).ToArray();
var changes = new List<GridPosition<Cell>>();

var (schema, tileSet) = await ReadSchema(args[0]);
var synthesizer = new WaveSynthesizer(Console.WindowWidth, Console.WindowHeight, schema, random);

// Console.WriteLine("RULES");
// foreach (var constraint in schema.Order())
// 	Console.WriteLine(constraint);

Console.CancelKeyPress += (s, e) =>
{
	Console.ResetColor();
	Console.SetCursorPosition(0, synthesizer.Grid.Height - 1);
	Console.WriteLine();
};

while (true)
{
	Console.CursorVisible = true;
	Console.ResetColor();
	Console.Clear();
	Console.CursorVisible = false;

	random.Shuffle(palette);
	synthesizer.Reset();
	while (synthesizer.CollapseNext(changes))
	{
		Draw(changes);
		changes.Clear();
	}

	while (Console.KeyAvailable) Console.ReadKey(true);
	Console.ReadKey(true);
}


void Draw(IEnumerable<GridPosition<Cell>> changes)
{
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