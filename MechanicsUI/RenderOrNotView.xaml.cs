using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MechanicsUI
{
    public partial class RenderOrNotView : UserControl
    {
        public RenderOrNotView()
        {
            InitializeComponent();
        }
    }

    public class RenderOrNot_ContentConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is RenderVM vm ? new RenderView { DataContext = vm } : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
