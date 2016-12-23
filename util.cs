using System;
using System.Text.RegularExpressions;

static class AnimeUtil
{
	public static string GetAnimeName(string link)
	{
		string pattern = @"https://myanimelist.net/anime/.*/(.*)";

		return Regex.Match(link, pattern).Groups[1].Value.Replace("_", " ");
	}

	public static void PrintError(string err)
	{
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine("[ERROR] " + err);
	}
}