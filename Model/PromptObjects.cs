using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.Model
{
    public class PromptModifier
    {
        public string Name { get; set; }
        public string Modifier { get; set; }
    }
    public class RandomizedDataElement
    {
        public string DataList;
        public int Number;
        public bool repeatable;
        public string ReturnName;
    }
    public class PromptLine
    {
        public string Value { get; set; }
        public int ID { get; set; }
        public string Filter { get; set; }
        public List<PrerequisitePrompt> PrerequisitePrompts { get; set; } = new List<PrerequisitePrompt>();
        public List<RandomizedDataElement> RandomData { get; set; } = new List<RandomizedDataElement>();
    }
    public class Prompt
    {
        public string Name { get; set; }
        //public List<RandomizedDataElement> DataElements { get; set; } = new List<RandomizedDataElement>();
        public List<PromptLine> PromptLine { get; set; } = new List<PromptLine>();
        public List<string> ValidModifiers { get; set; } = new List<string>();
    }
    public class PrerequisitePrompt
    {
        public string Prompt { get; set; }
        public string ReturnName { get; set; }
        public int Number { get; set; }
    }
    public class GeneratorPrompt
    {

    }
    public class RandomizedElement : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string File { get; set; }
        public string Name { get; set; }
        bool? _active;
        public bool? Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                OnPropertyChanged();
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    /*public class RandomizedElementList { 
    
        public string Name { get; set; }
        public List<Element> Elements { get; set;} = new List<Element>();
    }*/
    public class ResolvedValues
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
