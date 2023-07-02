using CQCopyPasteAdapter.Helpers;
using CQCopyPasteAdapter.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQCopyPasteAdapter.Storage
{
    public class SqliteKvStore<T> : IDictionary<String, T>
    {
        private readonly Dictionary<String, T> _cache = new Dictionary<String, T>();
        private readonly String _storeName;
        private readonly SQLiteConnection _connection;

        public SqliteKvStore(SQLiteConnection conn, String storeName)
        {
            if (!conn.GetTableNames().Contains(storeName))
            {
                new SQLiteCommand($"Create table \"{storeName}\" (Key text primary key, Value text);", conn).ExecuteNonQuery();
            }

            string sql = $"SELECT * FROM \"{storeName}\"";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var key = reader["Key"].ToString();
                if (key == null)
                {
                    Logger.Current.Alert($"Null Key read in table {storeName}.");
                    continue;
                }

                var strValue = reader["Value"].ToString();

                if (!JsonConvertHelper.TryDeserializeObject<T>(strValue, out var value) || value == null)
                {
                    Logger.Current.Alert($"Value format for {key} is wrong in table {storeName}.");
                    continue;
                }

                _cache.Add(key, value);
                if (value is INotifiedCollection ntc)
                {
                    var localKey = key;
                    ntc.CollectionChanged += (sender, e) => { SqliteUpdate(key, _cache[key]); };
                }
            }

            _storeName = storeName;
            _connection = conn;
        }

        private void SqliteSet(String key, T value)
        {
            var strValue = JsonConvert.SerializeObject(value);
            string sql = $"Insert into \"{_storeName}\" (Key,Value) Values (@key,@value)";
            SQLiteCommand cmd = new SQLiteCommand(sql, _connection);
            cmd.Parameters.AddWithValue("key", key);
            cmd.Parameters.AddWithValue("value", strValue);
            cmd.ExecuteNonQuery();
        }

        private void SqliteUpdate(String key, T value)
        {
            var strValue = JsonConvert.SerializeObject(value);
            string sql = $"Update \"{_storeName}\" Set Value = @value Where Key = @key";
            SQLiteCommand cmd = new SQLiteCommand(sql, _connection);
            cmd.Parameters.AddWithValue("key", key);
            cmd.Parameters.AddWithValue("value", strValue);
            cmd.ExecuteNonQuery();
        }

        private void SqliteClear()
        {
            string sql = $"Delete from \"{_storeName}\"";
            SQLiteCommand cmd = new SQLiteCommand(sql, _connection);
            cmd.ExecuteNonQuery();
        }

        private void SqliteRemove(string key)
        {
            string sql = $"Delete from \"{_storeName}\" Where Key = @key";
            SQLiteCommand cmd = new SQLiteCommand(sql, _connection);
            cmd.Parameters.AddWithValue("key", key);
            cmd.ExecuteNonQuery();
        }

        #region IDictionary<String,T>


        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return _cache.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_cache).GetEnumerator();
        }

        public void Add(KeyValuePair<string, T> item)
        {
            lock (this)
            {
                if (_cache.ContainsKey(item.Key))
                {
                    SqliteUpdate(item.Key, item.Value);
                }
                else
                {
                    SqliteSet(item.Key, item.Value);
                }
                _cache[item.Key] = item.Value;

                if (item.Value is INotifiedCollection ntc)
                {
                    var key = item.Key;
                    ntc.CollectionChanged += (sender, e) =>
                    {
                        SqliteUpdate(key, _cache[key]);
                    };
                }
            }
        }

        public void Clear()
        {
            lock (this)
            {
                SqliteClear();
                _cache.Clear();
            }
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return _cache.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            (_cache as ICollection<KeyValuePair<string, T>>).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            lock (this)
            {
                if (_cache.ContainsKey(item.Key))
                {
                    SqliteRemove(item.Key);
                }

                return (_cache as ICollection<KeyValuePair<string, T>>).Remove(item);
            }
        }

        public int Count => _cache.Count;

        public bool IsReadOnly => false;

        public void Add(string key, T value)
        {
            lock (this)
            {
                if (_cache.ContainsKey(key))
                {
                    SqliteUpdate(key, value);
                }
                else
                {
                    SqliteSet(key, value);
                }
                _cache[key] = value;
                if (value is INotifiedCollection ntc)
                {
                    var localKey = key;
                    ntc.CollectionChanged += (sender, e) =>
                    {
                        SqliteUpdate(localKey, _cache[localKey]);
                    };
                }
            }
        }

        public bool ContainsKey(string key)
        {
            return _cache.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            lock (this)
            {
                if (_cache.ContainsKey(key))
                {
                    SqliteRemove(key);
                }

                return _cache.Remove(key);
            }
        }

        public bool TryGetValue(string key, out T value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public T this[string key]
        {
            get => _cache[key];
            set => Add(key, value);
        }

        public ICollection<string> Keys => _cache.Keys;

        public ICollection<T> Values => _cache.Values;

        #endregion
        
        public T GetValueOrSetDefault(String key, T defaultValue)
        {
            if (!this.ContainsKey(key))
            {
                this[key] = defaultValue;
                return defaultValue;
            }

            return this[key];
        }
    }
}
