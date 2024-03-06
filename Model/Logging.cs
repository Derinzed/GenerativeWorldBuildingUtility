using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.Model
{
    internal class Logging
    {
        private static Logging instance = null;
        private Logging() { }

        public void Init(StreamWriter writer)
        {
            File = null;
        }

        public static Logging Logger
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logging();
                }
                return instance;
            }
        }

        StreamWriter File = null;
    }
}
