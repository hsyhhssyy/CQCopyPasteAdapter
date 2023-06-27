using System;

namespace CQCopyPasteAdapter.Storage;

public interface INotifiedCollection
{
    event EventHandler<CollectionChangedEventArgs>? CollectionChanged;
}