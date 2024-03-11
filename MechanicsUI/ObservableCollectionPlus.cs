using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace MechanicsUI;

public class ObservableCollectionPlus<T> : ObservableCollection<T>
{
    internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");
    internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new PropertyChangedEventArgs("Item[]");
    internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

    private readonly List<T> _itemsList;

    public ObservableCollectionPlus()
        : base()
    {
        _itemsList = (List<T>)Items;
    }

    public void Reset(IEnumerable<T> collection)
    {
        CheckReentrancy();
        _itemsList.Clear();
        _itemsList.AddRange(collection);
        OnPropertyChanged(CountPropertyChanged);
        OnPropertyChanged(IndexerPropertyChanged);
        OnCollectionChanged(ResetCollectionChanged);
    }
}
