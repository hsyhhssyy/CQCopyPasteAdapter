using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQCopyPasteAdapter.Helpers
{
    public static class SqliteHelper
    {
        public static List<String> GetTableNames(this SQLiteConnection connection)
        {
            string sql = "SELECT * FROM sqlite_master WHERE type='table'";
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            var reader = cmd.ExecuteReader();
            var tables = new List<String>();
            while (reader.Read())
            {
                var tableName = reader["name"].ToString();
                if (tableName != null) tables.Add(tableName);
            }

            return tables;
        }
    }
}
