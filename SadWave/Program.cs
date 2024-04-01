using SadConsole.Configuration;
using SadWave.Scenes;
using WaveLib;
using WaveLib.Parser;

var (schema, tileSet) = await ReadSchema(args[0]);

Settings.WindowTitle = "My SadConsole Game";

Builder gameStartup = new Builder()
	.SetScreenSize(GameSettings.GAME_WIDTH, GameSettings.GAME_HEIGHT)
	.SetStartingScreen(game => new RootScreen(schema, tileSet))
	.IsStartingScreenFocused(true)
	.ConfigureFonts(true)
	;

Game.Create(gameStartup);
Game.Instance.DefaultFontSize = IFont.Sizes.Two;
Game.Instance.Run();
Game.Instance.Dispose();

async Task<(WaveSchema, char[])> ReadSchema(string path)
{
	using var file = File.OpenRead(path);
	switch (Path.GetExtension(path))
	{
		case ".txt":
			return WaveSchema.Analyze(await GridReader.Read(file), WaveSchema.Stencil.Plus);
		case ".schema":
			var (schema, tileSet) = WaveSchema.Parse(file);
			return (schema, tileSet.Select(s => Substitute(s.First())).ToArray());
		case ".rulz":
			return WaveRulz.Parse(file);
	}
	throw new Exception();
}

char Substitute(char c) => c switch
{
	'#' => ' ',
	'╗' => '\xbf', // '\xbb'
	'╝' => '\xd9', // '\xbc'
	'╚' => '\xc0', // '\xc8'
	'╔' => '\xda', // '\xc9'
	'║' => '\xb3', // '\xba'
	'═' => '\xc4', // '\xcd'
	_ => c
};
