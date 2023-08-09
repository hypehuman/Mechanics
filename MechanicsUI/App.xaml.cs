using AdonisUI;
using System.Windows;

namespace MechanicsUI;

partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        ResourceLocator.SetColorScheme(Resources, ResourceLocator.DarkColorScheme);

        base.OnStartup(e);
    }
}
