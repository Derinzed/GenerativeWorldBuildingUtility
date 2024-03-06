using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace GenerativeWorldBuildingUtility.Model
{
    public class Prompt
    {
        public Prompt(string name, string prompt, List<string> validModifiers)
        {
            Name = name;
            PromptLine = prompt;
            ValidModifiers = validModifiers;
        }
        public string Name { get; set; }
        public string PromptLine { get; set; }
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
