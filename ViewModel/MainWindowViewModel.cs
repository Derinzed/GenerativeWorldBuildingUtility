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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;
using GenerativeWorldBuildingUtility.View;

namespace GenerativeWorldBuildingUtility.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ICommand GeneratePrompt {get; set;}
        public ICommand SavePrompt { get; set; }
        public ICommand AIModelChange { get; set; }
        public ICommand SetAPIKey { get; set; }
        public ICommand RemoveAPIKey { get; set; }
        public ICommand SavedGenerations { get; set; }
        public ICommand GeneratorEditor { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindowViewModel(Generator Generator)
        {
            this.Generator = Generator;
            Prompts = new ObservableCollection<string>(Generator.GetPromptNames());
            

            RandomElements = new ObservableCollection<RandomizedElementsViewModel>();

            GeneratePrompt = new RelayCommand(o => OnGenerate());
            SavePrompt = new RelayCommand(o => OnSave());
            AIModelChange = new MultiRelayCommand<string>(OnAIModelChange);
            SetAPIKey = new RelayCommand(o => OnSetAPIKey());
            RemoveAPIKey = new RelayCommand(o => OnRemoveAPIKey());
            SavedGenerations = new RelayCommand(o => OnSavedGenerations());
            GeneratorEditor = new RelayCommand(o => OnOpenEditor());

            BoundProperties = new BoundProperties();
            BoundProperties.LoadedPrompts = Generator.GetPrompts().Select(x => x.Name).ToList();
            BoundProperties.SelectedAIModel = Generator.SelectedAIModel;
            BoundProperties.AIModels = Generator.GetAIModels();



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
        ObservableCollection<string> _aiModels;
        public ObservableCollection<string> AIModels
        {

            get
            {
                return _aiModels;
            }
            set
            {
                _aiModels = value;
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

        public void OnAIModelChange(string model)
        {
            if (String.IsNullOrEmpty(model))
            {
                return;
            }
            BoundProperties.SelectedAIModel = model;
            EventReporting.GetEventReporter().InvokeEvent(GeneratorBaseEvents.AIModelChanged, BoundProperties.SelectedAIModel);
        }
        public void OnGenerate()
        {
            BoundProperties.PromptResult = "Generating result. Please wait.";
            EventReporting.GetEventReporter().InvokeEventAsync(GeneratorBaseEvents.PromptExecuted, BoundProperties.SelectedPrompt);
        }
        public void OnSave()
        {
            string File = Microsoft.VisualBasic.Interaction.InputBox("Please select a unique name to save the generation. This will be saved to a .txt file.", "Save Generation", "Default", 0, 0);
            if(String.IsNullOrEmpty(File) || String.IsNullOrEmpty(BoundProperties.PromptResult))
            {
                return;
            }

            FileManagement.SavePrompt(BoundProperties.PromptResult, File);
        }

        public void OnSetAPIKey()
        {
            string key = Microsoft.VisualBasic.Interaction.InputBox("Please provide your OPEN AI API key. Do not share this with anyone! The key is stored locally, and not saved on our servers.", "Set Your API Key", "Default", 0, 0);
            if (String.IsNullOrEmpty(key))
            {
                return;
            }
            Generator.SetAPIKey(key);
        }

        public void OnRemoveAPIKey()
        {
            Generator.SetAPIKey(null);
        }

        public void OnSavedGenerations()
        {
            Process.Start("explorer.exe", "SavedPrompts");
        }
        public void OnPromptCompleted(object? o, NotificationEventArgs args)
        {
            BoundProperties.PromptResult = args.Message;
        }

        public void OnOpenEditor()
        {
            GeneratorEditor editor = new GeneratorEditor();
            editor.Show();
        }

        public BoundProperties BoundProperties { get; set; }
        public Generator Generator { get; set; }
    }
}
