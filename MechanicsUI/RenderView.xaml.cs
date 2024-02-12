using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MechanicsUI;

public partial class RenderView : UserControl
{
    public RenderView()
    {
        InitializeComponent();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (DataContext is not RenderVM vm)
            return;

        var mousePosition = GetCanvasMousePosition(vm, e);
        vm.RefreshByDistance(mousePosition);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        (DataContext as RenderVM)?.RefreshByDistance(null);
    }

    private Point? GetCanvasMousePosition(RenderVM vm, MouseEventArgs e)
    {
        var canvas = GetItemsPanel(BodiesItemsControl);
        if (canvas == null)
        {
            return null;
        }

        // This solution is simpler, but didn't always work:
        // This only works for smaller systems like MoonFromRing.
        // But for larger systems like SunEarthMoon,
        // where the computed mouse position is always (0,0) for some reason.
        //return e.GetPosition(canvas);

        // This solution is very dependent on how we set up the canvas alignments and transforms.
        var rawPosition = e.GetPosition(this);
        return new
        (
            (rawPosition.X - ActualWidth / 2 - vm.CanvasTranslateX) / vm.CanvasScale,
            (rawPosition.Y - ActualHeight / 2 - vm.CanvasTranslateY) / vm.CanvasScale
        );
    }

    private static Panel? GetItemsPanel(ItemsControl itemsControl)
    {
        var itemsPresenter = GetVisualChild<ItemsPresenter>(itemsControl);
        if (itemsPresenter == null)
            return null;
        var itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel;
        return itemsPanel;
    }

    private static T? GetVisualChild<T>(DependencyObject parent) where T : Visual
    {
        if (parent == null)
            return null;

        T? child = default;

        int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < numVisuals; i++)
        {
            var v = (Visual)VisualTreeHelper.GetChild(parent, i);
            child = v as T;
            if (child == null)
            {
                child = GetVisualChild<T>(v);
            }
            if (child != null)
            {
                break;
            }
        }
        return child;
    }
}
