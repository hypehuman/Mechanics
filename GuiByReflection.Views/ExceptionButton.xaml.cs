using GuiByReflection.ViewModels;
using System.Windows;

namespace GuiByReflection.Views;

partial class ExceptionButton
{
    public ExceptionButton()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as IExceptionButtonVM;
        if (vm == null)
            return;

        Clipboard.SetText(vm.Details);
    }
}
