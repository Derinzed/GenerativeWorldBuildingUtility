using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.Model
{
    internal class Randomizer
    {
        static public int GetRandomInt(int min, int max)
        {
            return Rand.Next(min, max);
        }
        static public int GetRandomInt(int index)
        {
            return Rand.Next(index);
        }
        static public string GetRandomElementFromList(List<string> strings)
        {
            if (strings.Count == 0) {
                return "";
            }
            
            return strings.ElementAt(Rand.Next(strings.Count));
        }

        static Random Rand = new Random();
    }
}
