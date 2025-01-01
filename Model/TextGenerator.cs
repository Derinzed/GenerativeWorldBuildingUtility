using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JohnUtilities.Classes;
using Newtonsoft.Json.Linq;
using OpenAISharp;
using OpenAISharp.API;
using GenerativeWorldBuildingUtility.Model;
using System.Net;

namespace GenerativeWorldBuildingUtility.Model
{
    public class TextGenerator
    {
        public TextGenerator() { }

#if LOCALSERVERDEBUG
        string ServerAddress = "http://localhost:3000";
#else
        string ServerAddress = "http://3.137.208.22:3000";
#endif

        private static readonly CookieContainer cookieContainer = new CookieContainer();
        private static readonly HttpClientHandler handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            AllowAutoRedirect = true // Allow redirects if necessary
        };
        HttpClient client = new HttpClient(handler);

        public async Task<string> GenerateTextFromLocal(string prompt, string aiModel)
        {
            prompt += "f You are ChatGPT, a large language model trained by OpenAI, based on the GPT-3.5 architecture. Knowledge cutoff: 2021-09 Current date: 2024-03.  Do not use these symbols in your response: #, *, do not bold headers. do not use bolds or italics.";
            Logging.WriteLogLine("Prompt after modification: " + prompt);
            //hide this for release
            OpenAISettings.UrlPrefix = @"https://api.openai.com/v1";
            OpenAISettings.OrganizationID = @"";
            OpenAISettings.ApiKey = Environment.GetEnvironmentVariable("OpenAIAPIKey");
            var message = new chatformat[1]
            {
                new chatformat
                {
                    role = chatformat.roles.user,
                    content = prompt
                }
            };
            var selectedModel = Chat.AvailableModel.gpt_3_5_turbo;
            switch (aiModel)
            {
                case "gpt-3.5-turbo":
                    selectedModel = Chat.AvailableModel.gpt_3_5_turbo;
                    break;
                case "gpt-4":
                    selectedModel = Chat.AvailableModel.gpt_4;
                    break;
                case "gpt-4o":
                    selectedModel = Chat.AvailableModel.gpt_4;
                    break;
            }
            var call = await Chat.Request(new Chat() { SelectedModel = selectedModel, temperature = 0.8M, messages = message, max_tokens=4096 });
            //var call = await Chat.Request(prompt);
            return (call.error != null) ? call.error.message : call.choices.FirstOrDefault()!.message.content;
        }

        public async Task<string> GenerateTextFromServer(string prom, string aiModel)
        {
            var promptPrefix = "You are ChatGPT, and you are needed to generate a random element for a table top roleplaying game.  You have received the following request form a game master, please answer it in a format that closely resembles adventure modules, or in a format that best meets the specific request: ";

            // Define the API endpoint (the Node.js server URL)
            var apiUrl = ServerAddress + "/generate-response";  // Or your deployed server's URL

            // Try to load the token
            string token = Utilities.LoadToken();

#if ADMINOVERRIDE
            var AdminOverride = true;
#else
            var AdminOverride = false;
#endif


            var prompt = new
            {
                model = aiModel,
                prompt = promptPrefix + prom,
                adminOverride = AdminOverride
            };

            // Serialize the object to JSON
            var jsonContent = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(prompt),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
            {
                Content = jsonContent
            };

            // Attach the Authorization header with the token
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Send the POST request to the Node.js server
                HttpResponseMessage response = await client.SendAsync(request);

                // Read the response content
                string result = await response.Content.ReadAsStringAsync();

                // If the server responds with Unauthorized or Forbidden, token is likely expired
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // Parse the JSON response to get the redirect path
                    var parsedJson = JObject.Parse(result);

                    var server = new OAuthServer().StartServerAsync();

                    // Open the authentication page for the user to re-authenticate
                    Utilities.OpenBrowser(ServerAddress + parsedJson["redirectPath"]);

                    //startup the local server to listen for the return
                    var recievedToken = await server;

                    Utilities.SaveToken(recievedToken);

                    // Inform the user to verify subscription and try again

                    if (String.IsNullOrEmpty(recievedToken))
                    {
                        return "Your subscription could not be verified, please ensure you are subscribed to the RoguishCartographer with an appropriate tier to allow AI generation.";
                    }
                    return "Your subscription has been confirmed. Please run the generation again. You will not need to repeat this process for another 30 days.  Thank you for your support!";
                }

                // Check if the response was successful
                if (response.IsSuccessStatusCode)
                {
                    // Assuming the response contains the actual data you need
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
