using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MechanicsUI;

public partial class RenderView : UserControl
{
    public RenderView()
    {
        InitializeComponent();
    }

    private RenderVM? ViewModel => DataContext as RenderVM;

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var vm = ViewModel;
        if (vm == null)
            return;

        var mousePosition = GetCanvasMousePosition(vm, e);
        vm.SetMousePosition(mousePosition);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        ViewModel?.SetMousePosition(null);
    }

    private Point? GetCanvasMousePosition(RenderVM vm, MouseEventArgs e)
    {
        // This solution is very dependent on how we set up the alignments and transforms
        // in the canvas defined by the template for ItemsControl.ItemsPanel.
        var rawPosition = e.GetPosition(this);
        return new
        (
            (rawPosition.X - ActualWidth / 2 - vm.CanvasTranslateX) / vm.CanvasScale,
            (rawPosition.Y - ActualHeight / 2 - vm.CanvasTranslateY) / vm.CanvasScale
        );

        // We used to have a more future-proof solution,
        // where we would find the canvas and simply return e.GetPosition(canvas).
        // However it only worked for smaller systems like MoonFromRing.
        // For larger systems like SunEarthMoon,
        // the returned mouse position was always (0,0) for some reason.
    }
}
