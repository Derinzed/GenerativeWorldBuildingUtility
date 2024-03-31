using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.Model
{
    public class Constants
    {
#if DEBUG
        public const string DataLists = "..\\..\\..\\..\\Config\\DataLists";
        public const string Prompts = "..\\..\\..\\..\\Config\\Prompts.xml";

#elif RELEASE
        public const string DataLists = "..\\Config\\DataLists";
        public const string Prompts = "..\\Config\\Prompts.xml";
#endif
    }
}
