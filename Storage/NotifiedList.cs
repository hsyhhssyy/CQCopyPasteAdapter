using System;
using System.Collections;
using System.Collections.Generic;

namespace CQCopyPasteAdapter.Storage;

public class NotifiedList<T> : IList<T>, INotifiedCollection
{
    private readonly IList<T> _listImplementation = new List<T>();
    public IEnumerator<T> GetEnumerator()
    {
        return _listImplementation.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_listImplementation).GetEnumerator();
    }

    public void Add(T item)
    {
        _listImplementation.Add(item);
        CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(-1));
    }

    public void Clear()
    {
        _listImplementation.Clear();
        CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(-1));
    }

    public bool Contains(T item)
    {
        return _listImplementation.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _listImplementation.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        var result = _listImplementation.Remove(item);
        CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(-1));
        return result;
    }

    public int Count => _listImplementation.Count;

    public bool IsReadOnly => _listImplementation.IsReadOnly;

    public int IndexOf(T item)
    {
        return _listImplementation.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        _listImplementation.Insert(index, item);
        CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(-1));
    }

    public void RemoveAt(int index)
    {
        _listImplementation.RemoveAt(index);
        CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(-1));
    }

    public T this[int index]
    {
        get => _listImplementation[index];
        set
        {
            _listImplementation[index] = value;
            CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(-1));
        }
    }

    public event EventHandler<CollectionChangedEventArgs>? CollectionChanged;
}