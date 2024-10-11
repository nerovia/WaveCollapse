using WaveLib;
using WaveLib.Parser;
using System.CommandLine;

class Program : RootCommand
{
	public static int Main(params string[] args)
	{
		var program = new Program();
		return program.Invoke(args);
	}

	readonly Argument<string> SchemaArgument;
	readonly Option<int> WidthOption;
	readonly Option<int> HeightOption;
	readonly Option<int> TessellateOption;

	public Program()
	{
		SchemaArgument = new Argument<string>("schema");
		SchemaArgument.LegalFilePathsOnly();

		WidthOption = new Option<int>(["--width", "-w"], () => Console.WindowWidth);
		HeightOption = new Option<int>(["--height", "-h"], () => Console.WindowHeight);
		TessellateOption = new Option<int>(["--tess", "-t"], () => 1);

		AddArgument(SchemaArgument);
		AddOption(WidthOption);
		AddOption(HeightOption);
		AddOption(TessellateOption);

		this.SetHandler(RootHandler, SchemaArgument, WidthOption, HeightOption, TessellateOption);
	}

	static async void RootHandler(string schemaFile, int width, int height, int tessellate)
	{
		width = Math.Clamp(width, 1, Console.WindowWidth) / tessellate;
		height = Math.Clamp(height, 1, Console.WindowHeight) / tessellate;

		Random random = new(Environment.TickCount);
		ConsoleColor[] palette = [
			ConsoleColor.Blue,
			ConsoleColor.Cyan,
			ConsoleColor.DarkBlue,
			ConsoleColor.DarkCyan,
			ConsoleColor.DarkGray,
			ConsoleColor.DarkGreen,
			ConsoleColor.DarkMagenta,
			ConsoleColor.DarkRed,
			ConsoleColor.DarkYellow,
			ConsoleColor.Green,
			ConsoleColor.Magenta,
			ConsoleColor.Red,
			ConsoleColor.Yellow
		];

		(WaveSchema schema, char[] tileSet) = await ReadSchema(schemaFile);
		WaveSynthesizer synthesizer = new(width, height, schema, random);

		Console.CancelKeyPress += (s, e) =>
		{
			Console.ResetColor();
			Console.SetCursorPosition(0, height - 1);
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
			while (synthesizer.CollapseNext(pos =>
			{
				for (int ty = 0; ty < tessellate; ty++)
				{
					for (int tx = 0; tx < tessellate; tx++)
					{
						var (x, y, cell) = pos;
						Console.SetCursorPosition(x + tx * width, y + ty * height);
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
				}
			}));
			
			while (Console.KeyAvailable) Console.ReadKey(true);
			Console.ReadKey(true);
		}
	}

	static async Task<(WaveSchema, char[])> ReadSchema(string path)
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
}
