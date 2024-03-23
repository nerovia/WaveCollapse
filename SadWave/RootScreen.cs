using MathNet.Numerics;
using SadConsole.Effects;
using SadConsole.StringParser;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives.GridViews;
using SadRogue.Primitives.SerializedTypes;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading;
using WaveLib;

namespace SadWave.Scenes
{
	internal class RootScreen : ControlsConsole
	{
		readonly DrawingArea canvas;
		readonly WaveSynthesizer synthesizer;
		readonly char[] tileSet;
		readonly WaveSchema schema;
		readonly Palette palette = new([
			new Color(0xFF532b1d),
			new Color(0xFF53257e),
			new Color(0xFF518700),
			new Color(0xFF3652ab),
			new Color(0xFF4f575f),
			new Color(0xFFc7c3c2),
			new Color(0xFFe8f1ff),
			new Color(0xFF4d00ff),
			new Color(0xFF00a3ff),
			new Color(0xFF27ecff),
			new Color(0xFF36e400),
			new Color(0xFFffad29),
			new Color(0xFF9c7683),
			new Color(0xFFa877ff),
			new Color(0xFFaaccff),
			new Color(0xFF141829),
			new Color(0xFF351d11),
			new Color(0xFF362142),
			new Color(0xFF595312),
			new Color(0xFF292f74),
			new Color(0xFF3b3349),
			new Color(0xFF7988a2),
			new Color(0xFF7deff3),
			new Color(0xFF5012be),
			new Color(0xFF246cff),
			new Color(0xFF2ee7a8),
			new Color(0xFF43b500),
			new Color(0xFFb55a06),
			new Color(0xFF654675),
			new Color(0xFF596eff),
			new Color(0xFF819dff)
		]);

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
			list.IsEnabled = false;
			Controls.Add(list);

			Surface.DrawBox(canvas.Bounds.Expand(space1.X, space1.Y), ShapeParameters.CreateStyledBoxThick(Color.White));

			var cells = new char[5, 5] // [height, width]
			{
				{ '#', '#', '#', '#', '#' },
				{ '#', '+', '+', '+', '#' },
				{ '#', '+', '.', '+', '#' },
				{ '#', '+', '+', '+', '#' },
				{ '#', '#', '#', '#', '#' }
			};

			(schema, tileSet) = WaveSchema.Analyze(Grid.From(cells), WaveSchema.Stencil.Plus);
			synthesizer = new WaveSynthesizer(canvas.Width, canvas.Height, schema, new Random(Environment.TickCount));

			foreach (var pattern in schema.Patterns.Select(PatternString))
				list.Items.Add(pattern);
		}

		async void Click(object? sender, EventArgs e)
		{
			if (generatorCancellationSource != null)
			{
				generatorCancellationSource.Cancel();
				generatorCancellationSource = null;
				generatorTask = null;
			}

			generatorCancellationSource = new();
			generatorTask = Generate(generatorCancellationSource.Token);
			//await generatorTask;
		}

		async Task Generate(CancellationToken cancellationToken)
		{
			synthesizer.Reset();
			canvas.Surface.Clear();
			await Task.Yield();
			while (!cancellationToken.IsCancellationRequested && synthesizer.CollapseNext())
			{
				RefreshCanvas();
			}
		}

		async Task OneShot(CancellationToken cancellationToken)
		{
			synthesizer.Reset();
			canvas.Surface.Clear();
			await Task.Yield();
			while (!cancellationToken.IsCancellationRequested && synthesizer.CollapseNext()) ;
			RefreshCanvas();
		}

		void RefreshCanvas()
		{
			foreach (var (x, y, cell) in synthesizer.Changes)
			{
				var coloredGlyph = canvas.Surface[x, y];
				coloredGlyph.Glyph = cell.IsCollapsed ? tileSet[cell.StateId] : (char)(cell.Entropy + '0');
				//coloredGlyph.Foreground = cell.IsCollapsed ? palette[cell.StateId] : Color.Transparent;
				coloredGlyph.Foreground = palette[cell.IsCollapsed ? cell.StateId : ^(cell.Entropy % palette.Length)];
			}
			canvas.IsDirty = true;
		}

		ColoredString PatternString(Pattern pattern)
		{
			var str = Regex.Replace(pattern.ToString(), @"#(-?\d+)", match =>
			{
				var idx = int.Parse(match.Value.Substring(1));
				var col = palette[idx];
				return $"[c:r f:{col.R},{col.G},{col.B}:{match.Value.Length}]{match.Value}";
			});

			return ColoredString.Parser.Parse(str);
		}

	}
}
