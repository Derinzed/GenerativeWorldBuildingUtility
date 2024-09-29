using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JohnUtilities.Classes;
using Newtonsoft.Json.Linq;
using OpenAISharp;
using OpenAISharp.API;

namespace GenerativeWorldBuildingUtility.Model
{
    public class TextGenerator
    {
        public TextGenerator() { }

        public async Task<string> GenerateTextFromLocal(string prompt)
        {
            prompt += "f You are ChatGPT, a large language model trained by OpenAI, based on the GPT-3.5 architecture. Knowledge cutoff: 2021-09 Current date: 2024-03.  Do not use these symbols in your response: #, *, do not bold headers. do not use bolds or italics.";
            Logging.WriteLogLine("Prompt after modification: " + prompt);
            //hide this for release
            OpenAISettings.UrlPrefix = @"https://api.openai.com/v1";
            OpenAISettings.OrganizationID = @"";
            OpenAISettings.ApiKey = @"";
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

        public async Task<string> GenerateTextFromServer(string prom, string aiModel)
        {
            // Define the API endpoint (the Node.js server URL)
            var apiUrl = "http://3.137.208.22:3000/generate-response";  // Or your deployed server's URL

            // Create an instance of HttpClient
            using (HttpClient client = new HttpClient())
            {

                var prompt = new
                {
                    model = aiModel,
                    prompt = prom
                };

                // Serialize the object to JSON
                var jsonContent = new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(prompt),
                    Encoding.UTF8,
                    "application/json");

                try
                {
                    // Send the POST request to the Node.js server
                    HttpResponseMessage response = await client.PostAsync(apiUrl, jsonContent);

                    // Check if the response was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content
                        string result = await response.Content.ReadAsStringAsync();

                        // Parse the JSON to extract the "content" field
                        var parsedJson = JObject.Parse(result);
                        string responseText = parsedJson["choices"][0]["message"]["content"].ToString();

                        return responseText;
                    }
                    else
                    {
                        return "Error: " + response.StatusCode;
                    }
                }
                catch (HttpRequestException e)
                {
                    return "Request error: " + e.Message;
                }
            }
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
