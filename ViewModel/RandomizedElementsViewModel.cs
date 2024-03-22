using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.ViewModel
{
    public class RandomizedElementsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public List<RandomizedElementsViewModel> Children { get; } = new List<RandomizedElementsViewModel>();

        public RandomizedElementsViewModel Parent { get; set; }
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
                this.SetIsChecked(value, true, true);
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _active)
                return;

            _active = value;

            if (updateChildren && _active.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_active, true, false));

            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            this.OnPropertyChanged("Active");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].Active;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }
    }
}
