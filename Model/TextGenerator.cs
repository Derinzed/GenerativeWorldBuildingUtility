using System;
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
            prompt += "f You are ChatGPT, a large language model trained by OpenAI, based on the GPT-3.5 architecture. Knowledge cutoff: 2021-09 Current date: 2024-03.  ";
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
