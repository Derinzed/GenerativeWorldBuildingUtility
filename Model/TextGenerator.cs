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
            prompt += "f You are ChatGPT, a large language model trained by OpenAI, based on the GPT-3.5 architecture. Knowledge cutoff: 2021-09 Current date: 2024-03.  ";

            //hide this for release
            OpenAISettings.UrlPrefix = @"https://api.openai.com/v1";
            OpenAISettings.OrganizationID = @"org-Vp7SmzC16IvQdmu1em2kyGNA";
            OpenAISettings.ApiKey = @"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";
            var message = new chatformat[1]
            {
                new chatformat
                {
                    role = chatformat.roles.user,
                    content = prompt
                }
            };
            var call = await Chat.Request(new Chat() { SelectedModel = Chat.AvailableModel.gpt_3_5_turbo, temperature = 0.8M, messages = message, max_tokens=4096 });
            //var call = await Chat.Request(prompt);
            return (call.error != null) ? call.error.message : call.choices.FirstOrDefault()!.message.content;
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
