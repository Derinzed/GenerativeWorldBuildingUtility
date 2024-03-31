using JohnUtilities.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GenerativeWorldBuildingUtility.Model
{
    public class FileManagement
    {
        public static void SavePrompt(string prompt, string Name)
        {
            if (!Directory.Exists("SavedPrompts"))
            {
                Directory.CreateDirectory("SavedPrompts");
            }
            File.WriteAllText(@"SavedPrompts\" + Name, prompt);
        }
    }
}
