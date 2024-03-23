using WaveLib;

var cells = new char[5, 5] // [height, width]
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

while (true)
{
	Console.CursorVisible = true;
	Console.ReadLine();
	Console.Clear();
	Console.CursorVisible = false;
	
	synthesizer.Reset();
	while (synthesizer.CollapseNext())
	{
		foreach (var (x, y, cell) in synthesizer.Changes)
		{
			Console.SetCursorPosition(x, y);
			var glyph = cell.IsCollapsed ? tileSet[cell.StateId] : (char)(cell.Entropy + '0');
			var colorIdx = (cell.IsCollapsed ? cell.StateId : cell.Entropy) % 7 + 31;
			Console.Write($"\x1b[{colorIdx}m{glyph}");
		}
	}
}
