using System.Windows;

namespace GuiByReflection.Views;

partial class HelpTooltipWrapper
{
    static HelpTooltipWrapper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(HelpTooltipWrapper),
            new FrameworkPropertyMetadata(typeof(HelpTooltipWrapper))
        );
    }

    public HelpTooltipWrapper()
    {
        InitializeComponent();
    }
}
