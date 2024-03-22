using SadConsole.Configuration;
using System.Runtime.CompilerServices;
using WaveLib;

var a = PrimitiveGrid.Create<string>(10, 10).Fill(pos => $"{pos.x}{pos.y}");




Settings.WindowTitle = "My SadConsole Game";

Builder gameStartup = new Builder()
	.SetScreenSize(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
	.SetStartingScreen<SadWave.Scenes.RootScreen>()
	.IsStartingScreenFocused(true)
	.ConfigureFonts(true)
	;

Game.Create(gameStartup);
Game.Instance.Run();
Game.Instance.Dispose();
