using GenerativeWorldBuildingUtility.Model;
using JohnUtilities.Classes;
using JohnUtilities.Model.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GenerativeWorldBuildingUtility.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ICommand GeneratePrompt {get; set;}

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel(Generator Generator)
        {
            this.Generator = Generator;
            Prompts = new ObservableCollection<string>(Generator.GetPromptNames());

            GetApplicableRandomElements("");

            GeneratePrompt = new RelayCommand(o => OnGenerate());

            BoundProperties = new BoundProperties();
            BoundProperties.LoadedPrompts = Generator.GetPrompts().Select(x => x.Name).ToList();

            EventReporting.GetEventReporter().SubscribeToEvent(GeneratorBaseEvents.PromptCompleted, OnPromptCompleted);

            BoundProperties.PropertyChanged += NotifyPromptPropertyChanged;
        }

        public void GetApplicableRandomElements(string prompt)
        {

            List<RandomizedElement> randElements = null;

            if (string.IsNullOrEmpty(prompt))
            {
                RandomElements = new ObservableCollection<RandomizedElementsViewModel>();
                randElements = Generator.GetRandomizedElements();
            }
            else
            {
                RandomElements.Clear();
                var randLists = Generator.GetPromptDataLists(prompt);
                if(randLists == null)
                {
                    return;
                }
                randElements = Generator.GetRandomizedElements().Where(x => randLists.Contains(x.File)).ToList();
            }
            RandomElements.CollectionChanged += NotifyCollectionChange;

            var uniqueElementFiles = randElements.Select(x => x.File).Distinct().ToList();

            foreach (var uniqeFile in uniqueElementFiles)
            {
                var NewFile = new RandomizedElementsViewModel { Name = uniqeFile };
                var FilesChildren = randElements.Where(x => x.File == uniqeFile);
                foreach (var file in FilesChildren)
                {
                    var newChild = new RandomizedElementsViewModel { Name = file.Name, Active = file.Active };
                    newChild.PropertyChanged += NotifyFilterPropertyChanged;
                    newChild.Parent = NewFile;
                    NewFile.Children.Add(newChild);
                }
                
                var activeCount = FilesChildren.Where(x => x.Active == true).Count();
                if(activeCount == 0)
                {
                    NewFile.Active = false;
                }
                if(activeCount == FilesChildren.Count())
                {
                    NewFile.Active = true;
                }

                RandomElements.Add(NewFile);
            }
        }
    
        public void NotifyPromptPropertyChanged(object? o, PropertyChangedEventArgs e)
        {
            if(e.PropertyName != "SelectedPrompt")
            {
                return;
            }

            GetApplicableRandomElements(BoundProperties.SelectedPrompt);
        }
        public void NotifyFilterPropertyChanged(object o , PropertyChangedEventArgs e)
        {
            RandomizedElementsViewModel element = (RandomizedElementsViewModel)o;

            if(element.Parent == null)
            {
                return;
            }

            Generator.SetRandomizedElementActivity(element.Parent.Name, element.Name, element.Active);
        }

        public void NotifyCollectionChange(object? o, NotifyCollectionChangedEventArgs arg)
        {
            if (arg.NewItems != null)
                foreach (RandomizedElementsViewModel item in arg.NewItems)
                    item.PropertyChanged += NotifyFilterPropertyChanged;

            if (arg.OldItems != null)
                foreach (RandomizedElementsViewModel item in arg.OldItems)
                    item.PropertyChanged -= NotifyFilterPropertyChanged;
        }

        ObservableCollection<string> _prompts;
        public ObservableCollection<string> Prompts {

            get {
                return _prompts;
            }
            set
            {
                _prompts = value;
            }
        }
        ObservableCollection<RandomizedElementsViewModel> _randomElements;
        public ObservableCollection<RandomizedElementsViewModel> RandomElements
        {
            get
            {
                return _randomElements;
            }
            set
            {
                _randomElements = value;
            }
        }

        public void OnGenerate()
        {
            EventReporting.GetEventReporter().InvokeEvent(GeneratorBaseEvents.PromptExecuted, BoundProperties.SelectedPrompt);
        }

        public void OnPromptCompleted(object? o, NotificationEventArgs args)
        {
            BoundProperties.PromptResult = args.Message;
        }

        public BoundProperties BoundProperties { get; set; }
        public Generator Generator { get; set; }
    }
}
