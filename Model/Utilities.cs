using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;    

namespace GenerativeWorldBuildingUtility.Model
{
    internal class Utilities
    {
        public static List<string> LoadFileIntoContainer(string fileName)
        {
            return File.ReadAllLines(fileName).ToList();
        }
    }
}
