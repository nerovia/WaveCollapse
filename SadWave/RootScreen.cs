using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives.GridViews;
using System.Text.RegularExpressions;
using WaveLib;

namespace SadWave.Scenes
{
	internal partial class RootScreen : ControlsConsole
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

		public RootScreen(WaveSchema schema, char[] tileSet) : base(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
		{
			this.schema = schema;
			this.tileSet = tileSet;

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
			synthesizer = new WaveSynthesizer(canvas.Width, canvas.Height, schema, new Random(Environment.TickCount));

			foreach (var constraint in schema.Order().Select(ConstraintString))
				list.Items.Add(constraint);
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
			var changes = new List<GridPosition<Cell>>();

			await Task.Yield();
			while (!cancellationToken.IsCancellationRequested && synthesizer.CollapseNext(DrawCallback)) ;
		}

		async Task OneShot(CancellationToken cancellationToken)
		{
			synthesizer.Reset();
			canvas.Surface.Clear();
			await Task.Yield();
			while (!cancellationToken.IsCancellationRequested && synthesizer.CollapseNext());
		}

		void DrawCallback(GridPosition<Cell> pos)
		{
			var (x, y, cell) = pos;
			var coloredGlyph = canvas.Surface[x, y];
			coloredGlyph.Glyph = cell.Status switch
			{
				CellStatus.Collapsed => tileSet[cell.State],
				_ => (char)(cell.SuperState.Count + '0')
			};
			coloredGlyph.Foreground = cell.Status switch
			{
				CellStatus.Collapsed => palette[cell.State],
				CellStatus.Undetermined => palette[^cell.SuperState.Count],
				_ => Color.Black
			};
			canvas.IsDirty = true;
		}

		ColoredString ConstraintString(Constraint constraint)
		{
			var str = ConstraintRegex().Replace(constraint.ToString(), match =>
			{
				var idx = int.Parse(match.Value[1..]);
				var col = palette[idx];
				return $"[c:r f:{col.R},{col.G},{col.B}:{match.Value.Length}]{match.Value}";
			});

			return ColoredString.Parser.Parse(str);
		}

		[GeneratedRegex(@"#(-?\d+)")]
		private static partial Regex ConstraintRegex();
	}
}
