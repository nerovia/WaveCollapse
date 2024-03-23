using MathNet.Numerics;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives.GridViews;
using WaveLib;

namespace SadWave.Scenes
{
	internal class RootScreen : ControlsConsole
	{
		readonly DrawingArea canvas;
		readonly WaveSynthesizer synthesizer;
		readonly Palette palette = new([Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Violet, Color.Magenta]);

		Task? generatorTask;
		CancellationTokenSource? generatorCancellationSource;

		public RootScreen() : base(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
		{
			var space1 = new Point(2, 1);
			var space2 = space1 * 2;
			var bounds = Surface.Bounds().Expand(-space2.X, -space2.Y);
			
			canvas = new(bounds.Width * 2 / 3, bounds.Height) { Position = space2 };
			Controls.Add(canvas);

			var button = new Button("Generate");
			button.PlaceRelativeTo(canvas, Direction.Types.Right, space2.X);
			button.Click += Click;
			Controls.Add(button);

			var list = new ListBox(bounds.Width - canvas.Width - space1.X, bounds.Height - 2);
			list.PlaceRelativeTo(button, Direction.Types.Down, space1.Y);
			Controls.Add(list);

			Surface.DrawBox(canvas.Bounds.Expand(space1.X, space1.Y), ShapeParameters.CreateStyledBoxThick(Color.White));

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

			foreach (var pattern in patterns)
				list.Items.Add(pattern);
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
			canvas.Surface.Clear();
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
