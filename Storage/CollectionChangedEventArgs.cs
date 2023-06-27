using System;

namespace CQCopyPasteAdapter.Storage;

public class CollectionChangedEventArgs : EventArgs
{
    public CollectionChangedEventArgs(object key)
    {
        Key = key;
    }

    public enum CollectionChangedEvent
    {
        Clear,
        Add,
        Update,
        Remove
    }

    public CollectionChangedEvent Event { get; set; }
    public Object Key { get; set; }
}