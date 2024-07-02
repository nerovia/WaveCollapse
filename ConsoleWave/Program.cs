using WaveLib;
using WaveLib.Parser;

var random = new Random(Environment.TickCount);
var palette = Enum.GetValues<ConsoleColor>().Except([ConsoleColor.Black, ConsoleColor.White, ConsoleColor.Gray]).ToArray();

var (schema, tileSet) = await ReadSchema(args[0]);
var synthesizer = new WaveSynthesizer(Console.WindowWidth, Console.WindowHeight, schema, random);

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
	while (synthesizer.CollapseNext(DrawCallback)) ;

	while (Console.KeyAvailable) Console.ReadKey(true);
	Console.ReadKey(true);
}


void DrawCallback(GridPosition<Cell> pos)
{
	var (x, y, cell) = pos;
	Console.SetCursorPosition(x, y);
	Console.ForegroundColor = cell.Status switch
	{
		CellStatus.Undetermined => palette[^cell.SuperState.Count],
		CellStatus.Collapsed => ConsoleColor.White,
		_ => ConsoleColor.Black
	};
	Console.BackgroundColor = cell.Status switch
	{
		CellStatus.Collapsed => palette![cell.State],
		_ => ConsoleColor.Black
	};
	Console.Write(cell.Status switch 
	{ 
		CellStatus.Collapsed => tileSet![cell.State],
		_ => (char)('0' + cell.SuperState.Count)
	});
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