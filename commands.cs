using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using static AnimeList;

static class AnimeCommands
{
	public static void ParseRun(string[] args)
	{
		try
		{
			Console.ForegroundColor = ConsoleColor.Green;
			AnimeIO.ReadAnimeList();

			#region Command Switch
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
							AnimeUtil.PrintError("Anime already is in list");
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
					AnimeIO.WriteAnimeList();

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
					AnimeIO.WriteAnimeList();
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

					AnimeIO.WriteAnimeList();
					Console.WriteLine("Updated entry in AnimeList");
					break;

				case "open":
					Process.Start(animeList[Convert.ToInt32(args[1])]._link);
					Console.WriteLine("Opened MyAnimeList Webpage");
					break;

				case "play":
					if (animeList[Convert.ToInt32(args[1])]._watchLink == "none")
					{
						AnimeUtil.PrintError("Play link not defined");
						break;
					}
					if (animeList[Convert.ToInt32(args[1])]._episodeTotal == animeList[Convert.ToInt32(args[1])]._episodeFinished)
					{
						AnimeUtil.PrintError("No more episodes to watch");
						break;
					}

					Process.Start(animeList[Convert.ToInt32(args[1])]._watchLink + (animeList[Convert.ToInt32(args[1])]._episodeFinished + 1).ToString());
					Console.WriteLine("Opened Anime Webpage");
					break;

				default:
					AnimeUtil.PrintError("Unknown command");
					Console.WriteLine("Commands: " + CommandList);
					break;
			}
			#endregion Command Switch
		}
		catch (Exception ex)
		{
			AnimeUtil.PrintError("Bad Input (" + ex.Message + ")");
		}
	}
}