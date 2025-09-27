using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.Model
{
    public class Constants
    {
#if DEBUG || LOCALSERVERDEBUG
        public const string DataLists = "..\\..\\..\\..\\Config\\DataLists";
        public const string Prompts = "..\\..\\..\\..\\Config\\Prompts.xml";
        public const string SaveLocation = "..\\..\\..\\..\\SavedGenerations";

#else
        public const string DataLists = "..\\Config\\DataLists";
        public const string Prompts = "..\\Config\\Prompts.xml";
        public const string SaveLocation = "..\\..\\..\\SavedGenerations";
#endif
    }
}
