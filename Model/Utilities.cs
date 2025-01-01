using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Net.Http;
using System.Diagnostics;

namespace GenerativeWorldBuildingUtility.Model
{
    internal class Utilities
    {
        public static List<string> LoadFileIntoContainer(string fileName)
        {
            return File.ReadAllLines(fileName).ToList();
        }

        public static List<string> GetTextBetweenCharacters(string str, string char1, string char2)
        {
            List<string> result = new List<string>();
            var pattern = char1 + "[^"+char2+"]+"+ char2;
            foreach (Match match in Regex.Matches(str, pattern))
            {
                result.Add(match.Value.Replace("$", ""));
            }
            return result;
        }

        public static void OpenBrowser(string url)
        {
            // Open the default web browser to the given URL
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true // Ensures the URL opens in the user's default browser
            });
        }

        public static void SaveToken(string token)
        {
            File.WriteAllText("token.txt", token); // Save token to a file
        }

        public static string LoadToken()
        {
            return File.Exists("token.txt") ? File.ReadAllText("token.txt") : string.Empty;
        }
    }
}
