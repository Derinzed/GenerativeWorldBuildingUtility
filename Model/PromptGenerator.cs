using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Security.Policy;
using JohnUtilities.Classes;
using JohnUtilities.Model.Classes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GenerativeWorldBuildingUtility.Model
{    
    public  class PromptGenerator
    {
        public PromptGenerator(ConfigurationManager configMan, TextGenerator gen) { 
            ConfigManager= configMan;
            Generator = gen;
        }

        public void OnRandomElementUpdated(object? sender, PropertyChangedEventArgs e)
        {

        }

        public void LoadPrompts()
        {
            //preload Randomzied Data to construct prompts from
            LoadRandomizedData();

            var file = Constants.Prompts;
            ConfigManager.LoadConfigFile(file);

            //get root element
            var RootElement = ConfigManager.GetItemFromName("Prompts");

            //get base prompts
            var prompts = ConfigManager.GetChildrenElements(RootElement);

            foreach (var prompt in prompts)
            {
                var NewPrompt = new Prompt();
                NewPrompt.Name = prompt.GetAttribute("name");
                var children = ConfigManager.GetChildrenElements(prompt);
                var ModifiersElement = children.First(x => x.Key == "ValidModifiers");
                var modifiers = ModifiersElement.GetAttribute("value").Split(',').ToList();
                NewPrompt.ValidModifiers = modifiers;

                var NewPromptLine = new PromptLine();
                var PromptLineElement = children.First(x => x.Key == "PromptLine");
                NewPromptLine.ID = Int32.TryParse(PromptLineElement.GetAttribute("id"), out var dump) ? Int32.Parse(PromptLineElement.GetAttribute("id")) : 0;
                NewPromptLine.Filter = PromptLineElement.GetAttribute("filter");
                NewPromptLine.Value = PromptLineElement.GetAttribute("value");

                var PromptLineChildren = ConfigManager.GetChildrenElements(PromptLineElement);
                //Get word blacklist
                var Blacklist = PromptLineChildren.First(x => x.Key == "WordsBlacklist");
                NewPromptLine.WordBlacklist = Blacklist.GetAttribute("value");

                //Get appended Prompts
                var AppendedPrompts = PromptLineChildren.First(x => x.Key == "AppendedPrompts");
                var AppendedPromptList = ConfigManager.GetChildrenElements(AppendedPrompts);
                foreach (var appPrompt in AppendedPromptList)
                {
                    var AppendedPrompt = new AppendedPrompt();
                    AppendedPrompt.prompt = appPrompt.GetAttribute("prompt");
                    AppendedPrompt.query = appPrompt.GetAttribute("promptQuery");
                    AppendedPrompt.queryReturn = appPrompt.GetAttribute("promptQueryReturnName");
                    NewPromptLine.AppendedPrompts.Add(AppendedPrompt);
                }
                    //Get Random Data
                var RandomDataElement = PromptLineChildren.First(x => x.Key == "RandomData");
                var RandomDataElementChildren = ConfigManager.GetChildrenElements(RandomDataElement);
                foreach(var randomElement in RandomDataElementChildren)
                {
                    var RandElm = new RandomizedDataElement();
                    if(NewPromptLine.RandomData.Where(x => x.DataList == randomElement.GetAttribute("DataList")).Count() != 0){
                        RandElm.DataList = randomElement.GetAttribute("DataList") + "2";
                    }
                    else
                    {
                        RandElm.DataList = randomElement.GetAttribute("DataList");
                    }
                    RandElm.Number = Int32.TryParse(randomElement.GetAttribute("number"), out dump) ? Int32.Parse(randomElement.GetAttribute("number")) : 0;
                    RandElm.repeatable = Boolean.TryParse(randomElement.GetAttribute("repeatable"), out var dump2) ? Convert.ToBoolean(randomElement.GetAttribute("repeatable")) : false;
                    RandElm.ReturnName = randomElement.GetAttribute("returnName");
                    if (randomElement.GetAttribute("displayName") == "")
                    {
                        RandElm.DisplayName = randomElement.GetAttribute("returnName");
                    }
                    else
                    {
                        RandElm.DisplayName = randomElement.GetAttribute("displayName");
                    }
                    var gatheredElements = GetRandomElements(randomElement.GetAttribute("DataList"));
                    foreach (var elm in gatheredElements)
                    {
                        RandElm.Elements.Add(new RandomizedElement() { File = elm.File, Name = elm.Name, Active = true, ContainingPrompt = NewPrompt.Name });
                    }
                    NewPromptLine.RandomData.Add(RandElm);
                }

                NewPrompt.PromptLine.Add(NewPromptLine);
                Prompts.Add(NewPrompt);

                Logging.WriteLogLine("Created Prompt");
            }
        }
        public List<RandomizedElement> GetRandomElements(string dataList)
        {
            return RandomizedElements.Where(x => x.File == dataList).ToList();
        }
        public void LoadRandomizedData()
        {
            foreach (var file in Directory.GetFiles(Constants.DataLists))
            {
                var RandomDataFile = file.Split('\\').Last().Split('.').First();
                StreamReader FileReader = new StreamReader(file);
                var line = "";
                while((line = FileReader.ReadLine()) != null ) {
                    var RandomElement = new RandomizedElement();
                    RandomElement.File = RandomDataFile;
                    RandomElement.Name = line;
                    RandomElement.Active= true;
                    RandomizedElements.Add(RandomElement);
                }
            }
        }

        public List<string> GetRandomData(string prompt, string dataList, int numReturns, bool repeatable)
        {
            var Results = new List<string>();

            var ActiveRandomData = Prompts.First(x => x.Name == prompt).PromptLine[0].RandomData.First(x => x.DataList == dataList).Elements.Where(x => x.Active == true).ToList();

            if(ActiveRandomData.Count == 0) {
                return new List<string> { "" };
            }
            
            for(var i = 0; i < numReturns; i++)
            {
                var index = Randomizer.GetRandomInt(ActiveRandomData.Count);
                Results.Add(ActiveRandomData.ElementAt(index).Name);

                if(repeatable == false)
                {
                    ActiveRandomData.RemoveAt(index);
                }
                if(ActiveRandomData.Count == 0)
                {
                    break;
                }
            }

            return Results;
        }

        public List<ResolvedValues> ResolveRandomData(string prompt)
        {
            var Resolved = new List<ResolvedValues>();

            //get prompt
            var Prompt = Prompts.First(x => x.Name == prompt);

            //resolve random data
            foreach (var data in Prompt.PromptLine[0].RandomData)
            {
                var result = new ResolvedValues();
                result.Name = data.ReturnName;
                foreach(var item in GetRandomData(prompt, data.DataList, data.Number, data.repeatable))
                {
                    result.Value += item + ", ";
                }
                Resolved.Add(result);
            }

            return Resolved;
        }

        public List<ResolvedValues> ResolvePromptRandomData(string prompt)
        {
            var RandomData = ResolveRandomData(prompt);
            return RandomData;
        }

        public string BuildPrompt(string line, List<ResolvedValues> resolved, string blacklist = null)
        {
            var Line = line;
            foreach(var element in resolved)
            {
                if (Line.Contains("@" + element.Name + "@"))
                {
                    Line = Line.Replace("@" + element.Name + "@", element.Value);
                }
            }

            if(!string.IsNullOrEmpty(blacklist))
            {
                Line += "Do not use the words: " + blacklist;
            }
            return Line;
        }

        public string ResolvePrompt(string promptLine, List<ResolvedValues> resolvedData)
        {

            var CompiledPrompt = BuildPrompt(promptLine, resolvedData);
            
            return CompiledPrompt;
        }

        public List<AppendedPrompt> GetAppendedPrompts(string prompt)
        {
            return Prompts.First(x => x.Name == prompt).PromptLine[0].AppendedPrompts;
        }

        private string RunPrompt(string input)
        {
            Logging.WriteLogLine("Executing prompt: " + input);
            return Task.Run(async () => await Generator.GenerateTextFromServer(input)).Result;
        }

        public Prompt GetPrompt(string prompt)
        {
            return Prompts.First(x => x.Name == prompt);
        }

        public string ExecutePrompt(string prompt)
        {
            var Prompt = GetPrompt(prompt);

            var RandData = ResolvePromptRandomData(prompt);
            string ResolvedPrompt = ResolvePrompt(Prompt.PromptLine[0].Value, RandData);
            var MainPromptReturn = RunPrompt(ResolvedPrompt);
            var FullReturn = MainPromptReturn;
            foreach(var appPrompt in GetAppendedPrompts(prompt))
            {
                var AppendedPrompt = appPrompt.prompt;
                if(appPrompt.queryReturn != "")
                {
                    var queryPrompt = RunPrompt(appPrompt.query);
                    AppendedPrompt = AppendedPrompt.Replace(appPrompt.queryReturn, queryPrompt);
                }
                var ResolvedAppPrompt = ResolvePrompt(appPrompt.prompt, RandData);
                FullReturn += "\n \n" + RunPrompt(FullReturn + "\n\n" + ResolvedAppPrompt);
            }

            return FullReturn;
        }


        public List<string> GetPromptNames()
        {
            return Prompts.Select(x => x.Name).ToList();
        }

        public List<RandomizedElement> GetPromptRandomizedElements(string prompt)
        {
            List<RandomizedElement> AllDataLists = new List<RandomizedElement>();

            var Prompt = Prompts.First(x => x.Name == prompt);

            foreach(var randData in Prompt.PromptLine[0].RandomData)
            {
                AllDataLists = AllDataLists.Concat(randData.Elements).ToList();
            }

            return AllDataLists;
        }
        public List<RandomizedDataElement> GetPromptRandomizedDataElements(string prompt)
        {

            var Prompt = Prompts.First(x => x.Name == prompt);

            return Prompt.PromptLine[0].RandomData;
        }

        public List<string> GetPromptDataLists(string prompt)
        {

            List<string> AllDataLists = new List<string>();

            //get prompt
            var Prompt = Prompts.FirstOrDefault(x => x.Name == prompt);
            if(Prompt == null)
            {
                return null;
            }

            AllDataLists = Prompt.PromptLine[0].RandomData.Select(x => x.DataList).ToList();
           

            return AllDataLists;
        }



        public List<Prompt> Prompts { get; private set; } = new List<Prompt>();
        //should only be used to quickly access RandomizedElements to build prompt objects.
        List<RandomizedElement> RandomizedElements { get; set; } = new List<RandomizedElement>();

        ConfigurationManager ConfigManager;
        TextGenerator Generator;
    }
}
