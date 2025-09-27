using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GenerativeWorldBuildingUtility.Converters
{
    public class ModelSelectionToCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string selectedModel = value as string;
            string model = parameter as string;

            return model == selectedModel; // Return true if the model is selected, false otherwise
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null; // Not needed for this case
        }
    }
}
