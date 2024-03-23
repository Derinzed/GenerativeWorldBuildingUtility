using GenerativeWorldBuildingUtility.Model;
using JohnUtilities.Classes;
using JohnUtilities.Model.Classes;
using Markdig.Helpers;
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

            RandomElements = new ObservableCollection<RandomizedElementsViewModel>();

            GeneratePrompt = new RelayCommand(o => OnGenerate());

            BoundProperties = new BoundProperties();
            BoundProperties.LoadedPrompts = Generator.GetPrompts().Select(x => x.Name).ToList();

            EventReporting.GetEventReporter().SubscribeToEvent(GeneratorBaseEvents.PromptCompleted, OnPromptCompleted);

            BoundProperties.PropertyChanged += NotifyPromptPropertyChanged;
        }

        public void GetApplicableRandomElements(string prompt)
        {
            //Dont display filters if no prompts are selected
            if (string.IsNullOrEmpty(prompt))
            {
                return;
            }
            else
            {

                RandomElements.Clear();
                RandomElements.CollectionChanged += NotifyCollectionChange;

                var RandomData = Generator.GetPromptRandomizedDataElements(prompt);

                foreach(var data in RandomData)
                {
                    RandomizedElementsViewModel NewParent = new RandomizedElementsViewModel() { Name = data.DisplayName, File = data.DataList };
                    
                    foreach(var element in data.Elements)
                    {
                        RandomizedElementsViewModel NewChild = new RandomizedElementsViewModel();
                        NewChild.Name = element.Name;
                        NewChild.Parent = NewParent;
                        NewChild.Active = element.Active;
                        NewChild.PropertyChanged += NotifyFilterPropertyChanged;
                        NewParent.Children.Add(NewChild);
                    }


                    var activeCount = NewParent.Children.Where(x => x.Active == true).Count();
                    if (activeCount == 0)
                    {
                        NewParent.Active = false;
                    }
                    if (activeCount == NewParent.Children.Count())
                    {
                        NewParent.Active = true;
                    }

                    RandomElements.Add(NewParent);
                }
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

            Generator.SetRandomizedElementActivity(element.Parent.File, BoundProperties.SelectedPrompt, element.Name, element.Active);
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
