using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQCopyPasteAdapter.Storage
{
    public class SqliteList<T> : IList<T>
    {
        private SqliteKvStore<T> innerStore;
        private List<T> cache;

        public SqliteList(SQLiteConnection conn, String storeName)
        {
            innerStore = new SqliteKvStore<T>(conn, storeName);
            cache = innerStore.OrderBy(t => double.TryParse(t.Key, out double parsedValue) ? parsedValue : 0)
                .Select(t => t.Value).ToList();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return cache.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return cache.GetEnumerator();
        }

        public double GetMaxKey()
        {
            if (innerStore.Count == 0)
            {
                return 1;
            }

            return innerStore.Keys.Select(t => double.TryParse(t, out double parsedValue) ? parsedValue : 0)
                .Max() + 1;
        }

        public void Add(T item)
        {
            var key = GetMaxKey();

            innerStore.Add($"{key}", item);

            cache = innerStore.OrderBy(t => double.TryParse(t.Key, out double parsedValue) ? parsedValue : 0)
                .Select(t => t.Value).ToList();
        }

        public void Clear()
        {
            innerStore.Clear();
            cache.Clear();
        }

        public bool Contains(T item)
        {
            return innerStore.Values.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            cache.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            //TODO Cache Invalid
            return innerStore.Remove(innerStore.First(t => t.Value?.Equals(item) ?? false));
        }

        public int Count
        {
            get => innerStore.Count;
        }
        public bool IsReadOnly
        {
            get => false;
        }
        public int IndexOf(T item)
        {
            return innerStore.OrderBy(t => double.TryParse(t.Key, out double parsedValue) ? parsedValue : 0)
                .Select((t, i) => new { T = t, Index = i }).Where(t => t.T.Equals(item)).Select(t => t.Index).FirstOrDefault();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                var key = innerStore
                    .OrderBy(t => double.TryParse(t.Key, out double parsedValue) ? parsedValue : 0)
                    .Select((t, i) => new { T = t, Index = i }).FirstOrDefault(t => t.Index == index)?.T.Key;
                if (key != null)
                {
                    return innerStore[key];
                }

                throw new IndexOutOfRangeException();
            }
            set
            {
                var key = innerStore
                    .OrderBy(t => double.TryParse(t.Key, out double parsedValue) ? parsedValue : 0)
                    .Select((t, i) => new { T = t, Index = i }).FirstOrDefault(t => t.Index == index)?.T.Key;
                if (key != null)
                {
                    innerStore[key] = value;
                }
            }
        }
    }

}
