using JohnUtilities.Classes;
using JohnUtilities.Model.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.Model
{
    public class CompletedPrompt
    {
        public string Name { get; set; }
        public string Timestamp { get; set; }
        public string Result { get; set; }
    }

    public class Generator
    {
        public Generator(PromptGenerator promptGen) {
            PromptGen = promptGen;
            EnvironmentalManager = new EnvironmentalManager();

            EventReporting.GetEventReporter().SubscribeToEvent(GeneratorBaseEvents.PromptExecuted, OnPromptExecuted);
            EventReporting.GetEventReporter().SubscribeToEvent(GeneratorBaseEvents.AIModelChanged, OnAIModelChange);
            EventReporting.GetEventReporter().SubscribeToEvent(GeneratorBaseEvents.ContextualInformationUpdated, OnContextualInformationChange);
            EventReporting.GetEventReporter().SubscribeToEvent(GeneratorBaseEvents.PromptModifiersUpdated, OnPromptModifiersChange);

            SelectedAIModel = GetAIModels().First();
        }

        public void OnPromptExecuted(object? o, NotificationEventArgs args)
        {
            Generate(args.Message);
        }
        public void OnAIModelChange(object? o, NotificationEventArgs args)
        {
            SelectedAIModel = args.Message;
        }
        public void OnContextualInformationChange(object? o, NotificationEventArgs args)
        {
            ContextualInformation = args.Message;
        }
        public void OnPromptModifiersChange(object? o, NotificationEventArgs args)
        {
            PromptModifiers = args.Message;
        }

        public string GetAPIKey()
        {
            return EnvironmentalManager.GetEnvironmentalVariable("OpenAIAPIKey");
        }
        public void SetAPIKey(string? APIKey)
        {
            EnvironmentalManager.SetEnvironmentalVariable("OpenAIAPIKey", APIKey);
        }
        public string Generate(string prompt)
        {
            var Result = PromptGen.ExecutePrompt(prompt, SelectedAIModel, ContextualInformation, PromptModifiers);

            var Completed = new CompletedPrompt();
            Completed.Name = prompt;
            Completed.Result = Result; ;
            Completed.Timestamp = DateTime.Now.ToString("MMddyyy - hh:mm:ss");

            CompletedPrompts.Add(Completed);
            EventReporting.GetEventReporter().InvokeEvent(GeneratorBaseEvents.PromptCompleted, Result);

            return Result;
        }

        public string SelectedAIModel { get; set; }

        public List<string> GetAIModels()
        {
            return PromptGen.AIModels;
        }

        public List<string> GetPromptNames()
        {
            return PromptGen.GetPromptNames();
        }

        public List<Prompt> GetPrompts()
        {
            return PromptGen.Prompts;
        }

        public List<RandomizedElement> GetPromptRandomizedElements(string prompt)
        {
            return PromptGen.GetPromptRandomizedElements(prompt);
        }
        public List<RandomizedDataElement> GetPromptRandomizedDataElements(string prompt)
        {
            return PromptGen.GetPromptRandomizedDataElements(prompt);
        }

        public List<string> GetPromptDataLists(string prompt)
        {
            return PromptGen.GetPromptDataLists(prompt);
        }

        public void SetRandomizedElementActivity(string file, string prompt,  string elementName, bool? active)
        {
            PromptGen.Prompts.First(x => x.Name == prompt).PromptLine[0].RandomData.First(x => x.DataList == file).Elements.First(x => x.Name == elementName).Active = active;
        }

        public List<CompletedPrompt> CompletedPrompts { get; set; } = new List<CompletedPrompt>();
        PromptGenerator PromptGen;
        string ContextualInformation = "";
        string PromptModifiers = "";
        EnvironmentalManager EnvironmentalManager;
    }
}
