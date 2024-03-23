using MathNet.Numerics;
using SadConsole.UI;
using SadConsole.UI.Controls;
using WaveLib;

namespace SadWave.Scenes
{
	internal class RootScreen : ScreenObject
	{
		readonly ControlsConsole console;
		readonly ScreenSurface canvas;
		readonly WaveSynthesizer synthesizer;
		readonly Palette palette = new([Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Violet, Color.Magenta]);

		Task? generatorTask;
		CancellationTokenSource? generatorCancellationSource;

		public RootScreen()
		{
			console = new ControlsConsole(GameSettings.GAME_WIDTH, 1) { Position = new(1, 1) };
			canvas = new ScreenSurface(GameSettings.GAME_WIDTH - 2, GameSettings.GAME_HEIGHT - 4) { Position = new(1, 3) };
			Children.Add(canvas);
			Children.Add(console);

			var button = new Button("Generate");
			button.Click += Click;
			console.Controls.Add(button);


			var cells = new int[5, 5] // [height, width]
			{
				{ '#', '#', '#', '#', '#' },
				{ '#', '+', '+', '+', '#' },
				{ '#', '+', '.', '+', '#' },
				{ '#', '+', '+', '+', '#' },
				{ '#', '#', '#', '#', '#' }
			};

			var grid = Grid.From(cells);
			var patterns = WaveAnalyzer.Analyze(grid);
			var random = new Random(Environment.TickCount);
			var tiles = patterns.Keys.Select(it => it.Subject).Distinct();
			var canvasGrid = Grid.Create<Cell>(canvas.Width, canvas.Height, _ => new());
			synthesizer = new WaveSynthesizer(random, canvasGrid, patterns.Keys, tiles, it => 1);
		}

		void Click(object? sender, EventArgs e)
		{
			if (generatorCancellationSource != null)
			{
				generatorCancellationSource.Cancel();
				generatorCancellationSource = null;
				generatorTask = null;
			}

			generatorCancellationSource = new();
			generatorTask = Generate(generatorCancellationSource.Token);
		}

		async Task Generate(CancellationToken cancellationToken)
		{
			synthesizer.Reset();
			canvas.Clear();
			await Task.Yield();
			while (!cancellationToken.IsCancellationRequested && synthesizer.CollapseNext())
			{
				foreach (var (x, y, cell) in synthesizer.Grid.Traverse())
				{
					var coloredGlyph = canvas.Surface[x, y];
					coloredGlyph.Glyph = cell.IsCollapsed ? (char)cell.State : (char)(cell.Entropy + '0');
					coloredGlyph.Foreground = palette[(cell.IsCollapsed ? cell.State : cell.Entropy) % palette.Length];
				}

				canvas.IsDirty = true;
			}
		}
	}
}
