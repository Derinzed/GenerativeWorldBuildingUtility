using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.ViewModel
{
    public class BoundProperties : INotifyPropertyChanged
    {
        public BoundProperties() { 
            LoadedPrompts= new List<string>();
        }    
        public string _selectedPrompt;
        public string SelectedPrompt
        {
            get
            {
                return _selectedPrompt;
            }
            set
            {
                _selectedPrompt = value;
                OnPropertyChanged();
            }
        }

        public List<string> _loadedPrompts;
        public List<string> LoadedPrompts
        {
            get
            {
                return _loadedPrompts;
            }
            set
            {
                _loadedPrompts = value;
            }
        }

        public string _promptResult;
        public string PromptResult
        {
            get
            {
                return _promptResult;
            }
            set
            {
                _promptResult = value;
                OnPropertyChanged();
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
