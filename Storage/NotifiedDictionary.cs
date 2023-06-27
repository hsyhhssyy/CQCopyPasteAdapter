using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQCopyPasteAdapter.Storage
{
    public class NotifiedDictionary<K, V> : IDictionary<K, V>, IReadOnlyDictionary<K, V>, INotifiedCollection where K : notnull
    {
        public event EventHandler<CollectionChangedEventArgs>? CollectionChanged;

        private readonly IDictionary<K, V> _dictionaryImplementation = new Dictionary<K, V>();

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return _dictionaryImplementation.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionaryImplementation).GetEnumerator();
        }

        public void Add(KeyValuePair<K, V> item)
        {
            _dictionaryImplementation.Add(item);
            CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(item.Key));
        }

        public void Clear()
        {
            _dictionaryImplementation.Clear();
            CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(default(K)) { Event = CollectionChangedEventArgs.CollectionChangedEvent.Clear });
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return _dictionaryImplementation.Contains(item);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            _dictionaryImplementation.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            var result = _dictionaryImplementation.Remove(item);
            CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(item.Key) { Event = CollectionChangedEventArgs.CollectionChangedEvent.Remove });
            return result;
        }

        public int Count => _dictionaryImplementation.Count;

        public bool IsReadOnly => _dictionaryImplementation.IsReadOnly;

        public void Add(K key, V value)
        {
            _dictionaryImplementation.Add(key, value);
            CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(key));
        }

        public bool ContainsKey(K key)
        {
            return _dictionaryImplementation.ContainsKey(key);
        }

        public bool Remove(K key)
        {
            var result = _dictionaryImplementation.Remove(key);
            CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(key) { Event = CollectionChangedEventArgs.CollectionChangedEvent.Remove });
            return result;
        }

        public bool TryGetValue(K key, out V value)
        {
            return _dictionaryImplementation.TryGetValue(key, out value);
        }

        public V this[K key]
        {
            get => _dictionaryImplementation[key];
            set
            {
                _dictionaryImplementation[key] = value;
                CollectionChanged?.Invoke(this, new CollectionChangedEventArgs(key));
            }
        }

        IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => _dictionaryImplementation.Keys;

        IEnumerable<V> IReadOnlyDictionary<K, V>.Values => _dictionaryImplementation.Values;

        public ICollection<K> Keys => _dictionaryImplementation.Keys;

        public ICollection<V> Values => _dictionaryImplementation.Values;
    }
}
