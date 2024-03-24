using System.Collections.Immutable;
using WaveLib;

Console.ForegroundColor = ConsoleColor.White;
Console.BackgroundColor = ConsoleColor.Black;

var random = new Random(Environment.TickCount);
var palette = Enum.GetValues<ConsoleColor>().Except([ConsoleColor.Black, ConsoleColor.White, ConsoleColor.Gray]).ToArray();

var (schema, tileSet) = await ReadSchema(args[0]);
var synthesizer = new WaveSynthesizer(Console.BufferWidth, Console.BufferHeight, schema, random);

Console.WriteLine("RULES");
foreach (var pattern in schema.Patterns.Order())
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
		Draw(synthesizer.Changes);
}

void Draw(IEnumerable<GridPosition<Cell>> changes)
{
	var foreground = Console.ForegroundColor;
	var background = Console.BackgroundColor;
	foreach (var (x, y, cell) in changes)
	{
		Console.SetCursorPosition(x, y);
		Console.ForegroundColor = cell.IsCollapsed ? ConsoleColor.White : (cell.IsExhausted ? ConsoleColor.Black : palette[^cell.Entropy]);
		Console.BackgroundColor = cell.IsCollapsed ? palette![cell.TileId] : ConsoleColor.Black;
		Console.Write(cell.IsCollapsed ? tileSet![cell.TileId] : (char)('0' + cell.Entropy));
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
	}
	throw new Exception();
}