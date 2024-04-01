using Antlr4.Runtime;
using MathNet.Numerics.RootFinding;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using WaveLib;
using WaveParser;

namespace WaveLib.Parser
{
	public static class WaveRulz
	{
		public static (WaveSchema, char[]) Parse(Stream stream)
		{
			using var reader = new StreamReader(stream);
			var s = reader.ReadToEnd();
			var str = new AntlrInputStream(s);
			var lexer = new WaveRulzLexer(str);
			var tokens = new CommonTokenStream(lexer);
			var parser = new WaveRulzParser(tokens);
			var listener_lexer = new ErrorListener<int>();
			var listener_parser = new ErrorListener<IToken>();
			lexer.AddErrorListener(listener_lexer);
			parser.AddErrorListener(listener_parser);
			var tree = parser.rulz();
			var schema = new WaveSchema();
			var tileSet = new TileSetBuilder<char>();

			foreach (var stat in tree.children.Cast<WaveRulzParser.StatContext>())
			{
				var sub = stat.GetChild(0).GetText()[0];
				var subId = tileSet.Add(sub);
				foreach (var prep in stat.children.Skip(1).SkipLast(1).Cast<WaveRulzParser.PrepContext>())
				{
					GridOffset[] deltas = prep.GetChild(0).GetText() switch
					{
						@"<l>" => [(-1, 0)],
						@"<r>" => [(1, 0)],
						@"<b>" => [(0, 1)],
						@"<t>" => [(0, -1)],
						@"<tl>" => [(-1, -1)],
						@"<tr>" => [(1, -1)],
						@"<bl>" => [(-1, 1)],
						@"<br>" => [(1, 1)],
						@"<->" => [(1, 0), (-1, 0)],
						@"<|>" => [(0, 1), (0, -1)],
						@"<\>" => [(-1, -1), (1, 1)],
						@"</>" => [(-1, 1), (1, -1)],
						@"<x>" => [(-1, -1), (1, 1), (-1, 1), (1, -1)],
						@"<+>" => [(1, 0), (-1, 0), (0, 1), (0, -1)],
						@"<*>" => [(1, 0), (-1, 0), (0, 1), (0, -1), (-1, -1), (1, 1), (-1, 1), (1, -1)],
						_ => throw new ArgumentException()
					};

					foreach (var tile in prep.children.Skip(1))
					{
						var obj = tile.GetText()[0];
						var objId = tileSet.Add(obj);
						foreach (var delta in deltas)
						{
							schema.Add(subId, objId, delta);
						}
					}
				}
			}

			return (schema, tileSet.Build());
		}
	}

	class ErrorListener<TSymbol> : ConsoleErrorListener<TSymbol>
	{
		public bool had_error;

		public override void SyntaxError(TextWriter output, IRecognizer recognizer, TSymbol offendingSymbol, int line,
			int col, string msg, RecognitionException e)
		{
			had_error = true;
			base.SyntaxError(output, recognizer, offendingSymbol, line, col, msg, e);
		}
	}
}
