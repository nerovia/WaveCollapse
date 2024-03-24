using SadConsole.Configuration;
using SadWave.Scenes;
using WaveLib;

var grid = await GridReader.Read(File.OpenRead(args[0]));

Settings.WindowTitle = "My SadConsole Game";

Builder gameStartup = new Builder()
	.SetScreenSize(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
	.SetStartingScreen(game => new RootScreen(grid))
	.IsStartingScreenFocused(true)
	.ConfigureFonts(true)
	;

Game.Create(gameStartup);
Game.Instance.DefaultFontSize = IFont.Sizes.Two;
Game.Instance.Run();
Game.Instance.Dispose();
