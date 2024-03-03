using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.Model
{
    public class GeneratorPrompt
    {
        public GeneratorPrompt(string name, string prompt)
        {
            Name = name;
            Prompt = prompt;
        }
        public string Name { get; set; }
        public string Prompt { get; set; }
    }
    public  class PromptGenerator
    {
        public void LoadPrompts()
        {
           var RawPrompt =  Utilities.LoadFileIntoContainer(Constants.Prompts);

            foreach(var item in RawPrompt)
            {

                Prompts.Add(new GeneratorPrompt(item.Split(';')[0], item.Split(";")[1]));
            }
        }


        List<GeneratorPrompt> Prompts;
    }
}
