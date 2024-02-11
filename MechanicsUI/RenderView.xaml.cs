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

        var mousePosition = GetCanvasMousePosition(e);
        vm.RefreshByDistance(mousePosition);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        (DataContext as RenderVM)?.RefreshByDistance(null);
    }

    private Point? GetCanvasMousePosition(MouseEventArgs e)
    {
        var canvas = GetItemsPanel(BodiesItemsControl);
        if (canvas == null)
        {
            return null;
        }

        // BUG:
        // This only works for smaller systems like MoonFromRing.
        // But for larger systems like SunEarthMoon,
        // where the computed mouse position is always (0,0) for some reason.
        return e.GetPosition(canvas);
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
