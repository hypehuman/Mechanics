using System.Windows;

namespace GuiByReflection.Views;

partial class GuiHelpTooltipWrapper
{
    static GuiHelpTooltipWrapper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GuiHelpTooltipWrapper),
            new FrameworkPropertyMetadata(typeof(GuiHelpTooltipWrapper))
        );
    }

    public GuiHelpTooltipWrapper()
    {
        InitializeComponent();
    }
}
