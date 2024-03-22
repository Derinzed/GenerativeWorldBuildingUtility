﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAISharp;
using OpenAISharp.API;

namespace GenerativeWorldBuildingUtility.Model
{
    public class TextGenerator
    {
        public TextGenerator() { }


        public async Task<string> GenerateText(string prompt)
        {
            OpenAIConfiguration.Load();

            var call = await Chat.Request(prompt);
            return call;
        }

        public void InitialSetup()
        {
            Console.WriteLine("Input your Organization ID:");
            string orgid = Console.ReadLine();
            Console.WriteLine("Input your API Key:");
            string apikey = Console.ReadLine();
            OpenAIConfiguration.CreateConfigFile(orgid, apikey);
            Console.WriteLine("appsettings.json file is created.");
            Console.ReadLine();
        }
    }
}
