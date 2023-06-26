using System.Windows;

namespace MechanicsUI;

/// <summary>
/// Adapted from https://stackoverflow.com/a/1083733
/// </summary>
public static class SizeObserver
{
    public static readonly DependencyProperty ObserveProperty = DependencyProperty.RegisterAttached(
        "Observe",
        typeof(bool),
        typeof(SizeObserver),
        new FrameworkPropertyMetadata(OnObserveChanged));

    public static readonly DependencyProperty ObservedSizeProperty = DependencyProperty.RegisterAttached(
        "ObservedSize",
        typeof(Size),
        typeof(SizeObserver));

    public static bool GetObserve(FrameworkElement frameworkElement)
    {
        return (bool)frameworkElement.GetValue(ObserveProperty);
    }

    public static void SetObserve(FrameworkElement frameworkElement, bool observe)
    {
        frameworkElement.SetValue(ObserveProperty, observe);
    }

    public static Size GetObservedSize(FrameworkElement frameworkElement)
    {
        return (Size)frameworkElement.GetValue(ObservedSizeProperty);
    }

    public static void SetObservedSize(FrameworkElement frameworkElement, Size observedSize)
    {
        frameworkElement.SetValue(ObservedSizeProperty, observedSize);
    }

    private static void OnObserveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var frameworkElement = (FrameworkElement)dependencyObject;

        if ((bool)e.NewValue)
        {
            frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
            UpdateObservedSizesForFrameworkElement(frameworkElement, new Size(frameworkElement.ActualWidth, frameworkElement.ActualHeight));
        }
        else
        {
            frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
        }
    }

    private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateObservedSizesForFrameworkElement((FrameworkElement)sender, e.NewSize);
    }

    private static void UpdateObservedSizesForFrameworkElement(FrameworkElement frameworkElement, Size newSize)
    {
        frameworkElement.SetCurrentValue(ObservedSizeProperty, newSize);
    }
}
