using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

static class AnimeList
{
	static Anime[] animeList;
	static int animeCount;
	const int AnimeArray_size = 100; // max number of Anime entries
	const string CommandList =
		@"list, exit, cls, delall, add (link), del (index), upd (index) (ep/link), open (index), play (index)";

	struct Anime
	{
		public string _link;
		public string _name;
		public string _watchLink;
		public int _episodeFinished;
		public int _episodeTotal;

		public Anime(string link, int epTotal, int epFinish = 0, string watchLink = "none")
		{
			_name             = GetAnimeName(link);
			_link             = link;
			_episodeTotal     = epTotal;
			_episodeFinished  = epFinish;
			_watchLink        = watchLink;
		}
	}

	static void SortAnimeList()
	{
		int index_w = 0, index_uw = 0, index_n = 0;
		Anime[] animeData_w  = new Anime[animeCount];
		Anime[] animeData_uw = new Anime[animeCount];
		Anime[] animeDataNew = new Anime[AnimeArray_size];

		for (int i = 0; i < animeCount; i++)
		{
			if (animeList[i]._episodeFinished == animeList[i]._episodeTotal)
			{
				animeData_w[index_w] = animeList[i];
				index_w++;
			}
			else
			{
				animeData_uw[index_uw] = animeList[i];
				index_uw++;
			}
		}

		Anime[] _animeData_w  = new Anime[index_w];
		Anime[] _animeData_uw = new Anime[index_uw];
		Array.Copy(animeData_w, _animeData_w, index_w);
		Array.Copy(animeData_uw, _animeData_uw, index_uw);
		Array.Sort(_animeData_w,  (x, y) => string.Compare(x._name, y._name));
		Array.Sort(_animeData_uw, (x, y) => string.Compare(x._name, y._name));

		for (int i = 0; i < index_w; i++)
		{
			animeDataNew[index_n] = _animeData_w[i];
			index_n++;
		}
		for (int i = 0; i < index_uw; i++)
		{
			animeDataNew[index_n] = _animeData_uw[i];
			index_n++;
		}

		animeList = animeDataNew;
	}

	static void ReadAnimeList()
	{
		Anime[] animeData = new Anime[AnimeArray_size];

		using (var file = new StreamReader("data/animelist.txt"))
		{
			string[] line;
			int index = 0;

			while (!file.EndOfStream)
			{
				line = file.ReadLine().Split('#');
				if (line.Length == 3)
				{
					animeData[index] = new Anime(line[0], Convert.ToInt32(line[1]), Convert.ToInt32(line[2]));
				}
				else
				{
					animeData[index] = new Anime(line[0], Convert.ToInt32(line[1]), Convert.ToInt32(line[2]), line[3]);
				}

				index++;
			}

			animeCount = index;
		}

		animeList = animeData;
	}

	static void WriteAnimeList()
	{
		if (File.Exists("data/animelist.txt"))
			File.Delete("data/animelist.txt");

		SortAnimeList();

		using (var file = new StreamWriter("data/animelist.txt"))
		{
			for (int i = 0; i < animeCount; i++)
			{
				file.WriteLine(animeList[i]._link + "#" + animeList[i]._episodeTotal.ToString() + "#" + 
					animeList[i]._episodeFinished.ToString() + "#" + animeList[i]._watchLink);
			}
		}
	}

	static string GetAnimeName(string link)
	{
		string pattern = @"https://myanimelist.net/anime/.*/(.*)";

		return Regex.Match(link, pattern).Groups[1].Value.Replace("_", " ");
	}

	static void PrintError(string err)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine("[ERROR] " + err);
	}
	
	static void ParseCommand(string[] args)
	{
		try
		{
			Console.ForegroundColor = ConsoleColor.Green;
			ReadAnimeList();

			#region Commands

			switch (args[0].ToLower())
			{
				case "cls":
					Console.Clear();
					break;

				case "delall":
					FileStream fs = File.Create("data/animelist.txt");
					fs.Close(); fs.Dispose();
					Console.WriteLine("Deleted all entries in AnimeList");
					break;

				case "list":
					string bt;

					if (animeCount < 1)
					{
						Console.WriteLine("[Empty AnimeList]");
						break;
					}

					for (int i = 0; i < animeCount; i++)
					{
						Console.ForegroundColor = ConsoleColor.Green;

						if (i > 9) { bt = "]"; } else { bt = " ]"; }
						if (animeList[i]._episodeFinished == animeList[i]._episodeTotal) { Console.ForegroundColor = ConsoleColor.DarkGreen; }
						else { Console.ForegroundColor = ConsoleColor.Green; }
						if (animeList[i]._episodeTotal < 10)
						{
							Console.Write("[" + i.ToString() + bt + " (" + animeList[i]._episodeFinished + "  | " + animeList[i]._episodeTotal + " ) ");
						}
						else if (animeList[i]._episodeFinished < 10)
						{
							Console.Write("[" + i.ToString() + bt + " (" + animeList[i]._episodeFinished + "  | " + animeList[i]._episodeTotal + ") ");
						}
						else
						{
							Console.Write("[" + i.ToString() + bt + " (" + animeList[i]._episodeFinished + " | " + animeList[i]._episodeTotal + ") ");
						}

						if (animeList[i]._watchLink == "none" && animeList[i]._episodeFinished != animeList[i]._episodeTotal)
						{ Console.ForegroundColor = ConsoleColor.Red; Console.Write("● "); }
						else if (animeList[i]._episodeFinished != animeList[i]._episodeTotal) { Console.Write("● "); }

						if (animeList[i]._episodeFinished == animeList[i]._episodeTotal) { Console.ForegroundColor = ConsoleColor.Gray; }
						else if (animeList[i]._episodeFinished == 0) { Console.ForegroundColor = ConsoleColor.White; }
						else { Console.ForegroundColor = ConsoleColor.Cyan; }
						Console.WriteLine(animeList[i]._name);
					}
					break;

				case "add":
					if (!args[1].StartsWith("https://myanimelist.net/anime/")) { throw new ArgumentException("Invalid MyAnimeList.net URL"); }

					int epCount;

					for (int i = 0; i < animeCount; i++)
					{
						if (animeList[i]._link == args[1])
						{
							PrintError("Anime already is in list");
							return;
						}
					}

					using (var web = new WebClient())
					{
						bool loading = true;
						Console.Write("Working.. |");
						Task task = Task.Run(async () => // fancy loading effect
						{
							while (loading)
							{
								if (!loading) break;
								Console.Write("\b/");
								if (!loading) break;
								await Task.Delay(70);
								if (!loading) break;
								Console.Write("\b-");
								if (!loading) break;
								await Task.Delay(70);
								if (!loading) break;
								Console.Write("\b\\");
								if (!loading) break;
								await Task.Delay(70);
								if (!loading) break;
								Console.Write("\b|");
								if (!loading) break;
								await Task.Delay(70);
							}
						});
						epCount = Convert.ToInt32(Regex.Match(web.DownloadString(args[1]), "<span id=\"curEps\">(.*)</span>").Groups[1].Value);
						loading = false;
					}

					animeList[animeCount] = new Anime(args[1], epCount);
					animeCount++;
					WriteAnimeList();

					Console.WriteLine("\b\b\b\b\b\b\b\b\b\b\bAdded entry to AnimeList");
					break;

				case "del":
					if (Convert.ToInt32(args[1]) >= animeCount) { throw new ArgumentException("Index out of range"); }

					Anime[] animeData = new Anime[AnimeArray_size];
					int index = 0;
					animeCount--;

					for (int i = 0; i < animeCount; i++)
					{
						if (i == Convert.ToInt32(args[1])) { args[1] = "256"; i--; index++; continue; }

						animeData[i] = new Anime(animeList[index]._link, animeList[index]._episodeTotal,
							animeList[index]._episodeFinished, animeList[index]._watchLink);
						index++;
					}

					animeList = animeData;
					WriteAnimeList();
					Console.WriteLine("Deleted entry from AnimeList");
					break;

				case "upd":
					if (Convert.ToInt32(args[1]) >= animeCount) { throw new ArgumentException("Index out of range"); }

					if (args[2].StartsWith("http"))
					{
						animeList[Convert.ToInt32(args[1])]._watchLink = args[2];
					}
					else
					{
						if (Convert.ToInt32(args[2]) > animeList[Convert.ToInt32(args[1])]._episodeTotal || Convert.ToInt32(args[2]) < 0)
						{ throw new ArgumentException("Episode out of range"); }

						animeList[Convert.ToInt32(args[1])]._episodeFinished = Convert.ToInt32(args[2]);
					}

					WriteAnimeList();
					Console.WriteLine("Updated entry in AnimeList");
					break;

				case "open":
					Process.Start(animeList[Convert.ToInt32(args[1])]._link);
					Console.WriteLine("Opened MyAnimeList Webpage");
					break;

				case "play":
					if (animeList[Convert.ToInt32(args[1])]._watchLink == "none")
					{
						PrintError("Play link not defined");
						break;
					}
					if (animeList[Convert.ToInt32(args[1])]._episodeTotal == animeList[Convert.ToInt32(args[1])]._episodeFinished)
					{
						PrintError("No more episodes to watch");
						break;
					}

					Process.Start(animeList[Convert.ToInt32(args[1])]._watchLink + (animeList[Convert.ToInt32(args[1])]._episodeFinished + 1).ToString());
					Console.WriteLine("Opened Anime Webpage");
					break;

				default:
					PrintError("Unknown command");
					Console.WriteLine("Commands: " + CommandList);
					break;
			}

			#endregion Commands
		}
		catch (Exception ex)
		{
			PrintError("Bad Input (" + ex.Message + ")");
		}
	}

	static void InitializeFiles()
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

	static void Main()
	{
		string input;

		Console.Title = "Anime Manager";
		Console.OutputEncoding = System.Text.Encoding.UTF8;
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("Commands: " + CommandList);

		InitializeFiles(); // creates data files if necessary

		Console.WriteLine(); ParseCommand(new string[] { "list" });

		while (true)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("\n> ");

			input = Console.ReadLine();
			if (input == "exit") break;

			ParseCommand(input.Split(' '));
		}

		Environment.Exit(0);
	}
}