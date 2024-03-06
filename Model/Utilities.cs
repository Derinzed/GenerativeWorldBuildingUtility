using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

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
    }
}
