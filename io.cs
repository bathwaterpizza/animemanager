using System;
using System.IO;
using static AnimeList;

static class AnimeIO
{
	private static void SortAnimeList()
	{
		int index_w = 0, index_uw = 0, index_n = 0;
		Anime[] animeData_w = new Anime[animeCount];
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

		Anime[] _animeData_w = new Anime[index_w];
		Anime[] _animeData_uw = new Anime[index_uw];
		Array.Copy(animeData_w, _animeData_w, index_w);
		Array.Copy(animeData_uw, _animeData_uw, index_uw);
		Array.Sort(_animeData_w, (x, y) => string.Compare(x._name, y._name));
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

	public static void ReadAnimeList()
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

	public static void WriteAnimeList()
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
}