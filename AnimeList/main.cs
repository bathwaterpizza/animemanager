using System;
using System.IO;

static class AnimeList
{
	public static Anime[] animeList;
	public static int animeCount;
	public const int AnimeArray_size = 256; // max number of Anime entries
	public const string CommandList = 
		@"list, exit, cls, purge, add (link), del (index), upd (index) (ep/link), open (index), play (index)";

	public class Anime
	{
		public string _link;
		public string _name;
		public string _watchLink;
		public int    _episodeFinished;
		public int    _episodeTotal;

		public Anime(string link, int epTotal, int epFinish = 0, string watchLink = "none")
		{
			_name             = AnimeUtil.GetAnimeName(link);
			_link             = link;
			_episodeTotal     = epTotal;
			_episodeFinished  = epFinish;
			_watchLink        = watchLink;
		}
	}

	private static void InitializeFiles()
	{
		Console.ForegroundColor = ConsoleColor.Yellow;

		if (!Directory.Exists("data"))
		{
			Directory.CreateDirectory("data");
			Console.WriteLine("[SYSTEM] Created directory \"data\"");
		}
		if (!File.Exists("data/animelist.txt"))
		{
			FileStream fs = File.Create("data/animelist.txt");
			fs.Close(); fs.Dispose();
			Console.WriteLine("[SYSTEM] Created file \"animelist.txt\"");
		}
	}

	private static void Main(string[] args)
	{
		string input;

		Console.Title = "Anime Manager";
		Console.OutputEncoding = System.Text.Encoding.UTF8;
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("Commands: " + CommandList);

		InitializeFiles(); // creates data files if necessary

		Console.WriteLine(); AnimeCommands.ParseRun(new string[] { "list" });

		while (true)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("\n> ");

			input = Console.ReadLine();
			if (input == "exit") break;

			AnimeCommands.ParseRun(input.Split(' '));
		}

		Environment.Exit(0);
	}
}