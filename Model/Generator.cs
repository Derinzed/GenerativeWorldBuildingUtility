﻿using JohnUtilities.Classes;
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

            EventReporting.GetEventReporter().SubscribeToEvent(GeneratorBaseEvents.PromptExecuted, OnPromptExecuted);
        }

        public void OnPromptExecuted(object? o, NotificationEventArgs args)
        {
            Generate(args.Message);
        }

        public string Generate(string prompt)
        {
            var Result = PromptGen.ExecutePrompt(prompt);

            var Completed = new CompletedPrompt();
            Completed.Name = prompt;
            Completed.Result = Result; ;
            Completed.Timestamp = DateTime.Now.ToString("MMddyyy - hh:mm:ss");

            CompletedPrompts.Add(Completed);
            EventReporting.GetEventReporter().InvokeEvent(GeneratorBaseEvents.PromptCompleted, Result);

            return Result;
        }


        public List<string> GetPromptNames()
        {
            return PromptGen.GetPromptNames();
        }

        public List<Prompt> GetPrompts()
        {
            return PromptGen.Prompts;
        }

        public List<RandomizedElement> GetRandomizedElements()
        {
            return PromptGen.RandomizedElements;
        }

        public List<string> GetPromptDataLists(string prompt)
        {
            return PromptGen.GetPromptDataLists(prompt);
        }

        public void SetRandomizedElementActivity(string file, string elementName, bool? active)
        {
            PromptGen.RandomizedElements.First(x => x.File == file && x.Name == elementName).Active = active;
        }

        public List<CompletedPrompt> CompletedPrompts { get; set; } = new List<CompletedPrompt>();
        PromptGenerator PromptGen;
    }
}
