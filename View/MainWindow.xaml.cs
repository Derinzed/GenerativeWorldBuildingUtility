using GenerativeWorldBuildingUtility.Model;
using GenerativeWorldBuildingUtility.ViewModel;
using JohnUtilities.Model.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GenerativeWorldBuildingUtility.Converters;

namespace GenerativeWorldBuildingUtility.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public void SetupAIModelSettingsView()
        {
            MenuItem optionsMenu = new MenuItem { Name = "AIModels", Header = "AI Models" };

            foreach (var model in VM.Generator.GetAIModels())
            {
                MenuItem option = new MenuItem
                {
                    Header = model,
                    IsCheckable = true
                };

                // Create a binding for IsChecked
                Binding binding = new Binding("BoundProperties.SelectedAIModel")
                {
                    Source = this.DataContext,
                    Converter = new ModelSelectionToCheckConverter(),
                    ConverterParameter = model, // Pass the model to the converter
                    Mode = BindingMode.OneWay
                };

                if (model == VM.BoundProperties.SelectedAIModel)
                {
                    option.IsChecked = true;
                }

                option.Command = VM.AIModelChange;
                option.CommandParameter = model;

                option.SetBinding(MenuItem.IsCheckedProperty, binding);

                optionsMenu.Items.Add(option);
            }

            MainMenuBar.Items.Add(optionsMenu);

        }

        public MainWindowViewModel VM { get; set; }
    }
}
