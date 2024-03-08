using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Security.Policy;

namespace GenerativeWorldBuildingUtility.Model
{
    public class PromptReference
    {
        public string BasePrompt;
        public string ReturnName;
        public string Number;
    }
    public class PromptModifier
    {
        public PromptModifier(string name, string modifier)
        {
            Name = name;
            Modifier = modifier;
        }
        public string Name { get; set; }
        public string Modifier { get; set; }
    }
    public class RandomizedDataElement
    {
        public string DataList;
        public string repeatable;
        public string ReturnName;
    }
    public class PromptLine
    {
        public string Value { get; set; }
        public string Filter;
        public List<PromptReference> PrerequisitePrompts { get; set; }
        public List<RandomizedDataElement> RandomData;
    }
    public class Prompt
    {
        public Prompt(string name, List<PromptLine> prompt, List<string> validModifiers, List<RandomizedDataElement> dataElements)
        {
            Name = name;
            PromptLine = prompt;
            ValidModifiers = validModifiers;
            DataElements = dataElements;
        }
        public string Name { get; set; }
        public List<RandomizedDataElement> DataElements { get; set; }
        public List<PromptLine> PromptLine { get; set; }
        public List<string> ValidModifiers { get; set; }
    }
    public class PrerequisitePrompt
    {

    }
    public class GeneratorPrompt
    {

    }
    public class Element
    {
        public Element(string name, bool active) {
            Name = name;
            Active = active;
        }
        public string Name { get; set; }
        public bool Active { get; set; }
    }
    public class RandomizedElementList
    {
        public RandomizedElementList(string name, List<Element> elements) { 
        
            Name = name;
            Elements = elements;
        }

        public string Name;
        public List<Element> Elements;
    }
    public  class PromptGenerator
    {
        public void LoadPrompts()
        {

        }

        public void LoadRandomizedElements()
        {
            foreach(var file in Directory.GetFiles(Constants.DataLists))
            {
                List<string> data = Utilities.LoadFileIntoContainer(file);
                List<Element> loadedData = new List<Element>();

                foreach(var item in data)
                {
                    loadedData.Add(new Element(item, true));
                }

                RandomizedElements.Add(new RandomizedElementList(file.Split("\\").Last().Split(".")[0], loadedData));
            }
        }

        public string GetRandomElementFromList(string ElementList)
        {
            List<string> elmenets = new List<string>();

            RandomizedElementList selectedList = RandomizedElements.First(x => x.Name == ElementList);
            List<string> activeElements = selectedList.Elements.Where(x => x.Active == true).Select(x => x.Name).ToList();


            string result = Randomizer.GetRandomElementFromList(activeElements);
            return result;
        }

        List<GeneratorPrompt> Prompts = new List<GeneratorPrompt>();
        List<RandomizedElementList> RandomizedElements= new List<RandomizedElementList>();
    }
}
