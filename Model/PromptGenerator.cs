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
using Markdig.Extensions.Yaml;
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
                var PrerequisitePromptsElement = PromptLineChildren.First(x => x.Key == "PrerequisitePrompts");
                var PrerequisitePromptCildren = ConfigManager.GetChildrenElements(PrerequisitePromptsElement);

                foreach (var prereqPrompt in PrerequisitePromptCildren)
                {
                    var prePrompt = new PrerequisitePrompt();
                    prePrompt.Prompt = prereqPrompt.GetAttribute("prompt");
                    prePrompt.ReturnName = prereqPrompt.GetAttribute("returnName");
                    prePrompt.Number = Int32.TryParse(prereqPrompt.GetAttribute("number"), out dump) ? Int32.Parse(prereqPrompt.GetAttribute("number")) : 0;
                    NewPromptLine.PrerequisitePrompts.Add(prePrompt);
                }

                var RandomDataElement = PromptLineChildren.First(x => x.Key == "RandomData");
                var RandomDataElementChildren = ConfigManager.GetChildrenElements(RandomDataElement);
                foreach(var randomElement in RandomDataElementChildren)
                {
                    var RandElm = new RandomizedDataElement();
                    RandElm.DataList = randomElement.GetAttribute("DataList");
                    RandElm.Number = Int32.TryParse(randomElement.GetAttribute("number"), out dump) ? Int32.Parse(randomElement.GetAttribute("number")) : 0;
                    RandElm.repeatable = Boolean.TryParse(randomElement.GetAttribute("repeatable"), out var dump2) ? Convert.ToBoolean(randomElement.GetAttribute("repeatable")) : false;
                    RandElm.ReturnName = randomElement.GetAttribute("returnName");
                    NewPromptLine.RandomData.Add(RandElm);
                }

                NewPrompt.PromptLine.Add(NewPromptLine);
                Prompts.Add(NewPrompt);

                Logging.WriteLogLine("Created Prompt");
            }
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

        public List<string> GetRandomData(string dataFile, int numReturns, bool repeatable)
        {
            var Results = new List<string>();   

            var ActiveRandomData = RandomizedElements.Where(x => x.Active == true && x.File == dataFile).ToList();
            
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
                foreach(var item in GetRandomData(data.DataList, data.Number, data.repeatable))
                {
                    result.Value += item + ", ";
                }
                Resolved.Add(result);
            }

            return Resolved;
        }

        public List<ResolvedValues> ResolvePrerequisitePrompts(string prompt)
        {
            var Resolved = new List<ResolvedValues>();

            //get prompt
            var Prompt = Prompts.First(x => x.Name == prompt);

            foreach(var prePrompt in Prompt.PromptLine[0].PrerequisitePrompts)
            {
                var resolvedValue = new ResolvedValues();
                resolvedValue.Name = prePrompt.ReturnName;
                var val = "";
                for(var i = 0; i < prePrompt.Number; i++)
                {
                    val += ExecutePrompt(prePrompt.Prompt) + ", ";
                }
                resolvedValue.Value = val;
                Resolved.Add(resolvedValue);

                Resolved = Resolved.UnionBy(ResolveRandomData(prePrompt.Prompt), x => x.Name).ToList();
            }

            return Resolved;
        }

        public List<ResolvedValues> ResolvePromptValues(string prompt)
        {

            var RandomData = ResolveRandomData(prompt);
            var PromptValues = ResolvePrerequisitePrompts(prompt);

            var ResolvedValues = RandomData.UnionBy(PromptValues, x => x.Name).ToList();
            return ResolvedValues;
        }

        public string BuildPrompt(string line, List<ResolvedValues> resolved)
        {
            var Line = line;
            foreach(var element in resolved)
            {
                if (Line.Contains("@" + element.Name + "@"))
                {
                    Line = Line.Replace("@" + element.Name + "@", element.Value);
                }
            }
            return Line;
        }

        public string ResolvePrompt(string prompt)
        {
            //get prompt
            var Prompt = Prompts.First(x => x.Name == prompt);

            var PromptInput = ResolvePromptValues(prompt);

            var CompiledPrompt = BuildPrompt(Prompt.PromptLine[0].Value, PromptInput);
            
            return CompiledPrompt;
        }

        public string ExecutePrompt(string prompt)
        {
            string ResolvedPrompt = ResolvePrompt(prompt);
            Logging.WriteLogLine("Executing prompt: " + ResolvedPrompt);
            return Task.Run(async () => await Generator.GenerateText(ResolvedPrompt)).Result;
        }

        public List<string> GetPromptNames()
        {
            return Prompts.Select(x => x.Name).ToList();
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

            foreach(var prereqPrompt in Prompt.PromptLine[0].PrerequisitePrompts) {
                var prereqDataList = GetPromptDataLists(prereqPrompt.Prompt);
                if(prereqDataList == null)
                {
                    continue;
                }
                AllDataLists = AllDataLists.Union(prereqDataList).ToList();
            }

            return AllDataLists;
        }



        public List<Prompt> Prompts { get; private set; } = new List<Prompt>();
        public List<RandomizedElement> RandomizedElements { get; private set; } = new List<RandomizedElement>();

        ConfigurationManager ConfigManager;
        TextGenerator Generator;
    }
}
