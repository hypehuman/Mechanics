using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TestIsMouseOver;

partial class BottomPart
{
    public BottomPart()
    {
        InitializeComponent();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        (DataContext as BottomPartViewModel)?.AddItem("OnMouseMove: " + e.GetPosition(this));
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        (DataContext as BottomPartViewModel)?.AddItem("OnMouseLeave");
    }
}

public class BottomPartViewModel
{
    public ObservableCollection<object> Items { get; } = new ObservableCollection<object>
    {
        "Initial Item",
    };

    internal void AddItem(object item)
    {
        Items.RemoveAt(Items.Count - 1);
        Items.Insert(0, item);
    }
}
