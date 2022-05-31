using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDbContext.NewExpSql
{    
    internal static class SqlContextExtend
    {
        public static string UpdateSql(this SqlContext self)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < self.Names.Count; i++)
            {
                sb.AppendLine($"{self.Names[i]} = {self.Values[i]},");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static (string, string) InsertSql(this SqlContext self)
        {
            return (string.Join(",", self.Names), string.Join(",", self.Values));
        }

        public static string Store(this SqlContext self)
        {
            var sql = self.Sql();
            self.Clear();
            self.Names.Clear();
            self.Values.Clear();
            return sql;
        }
    }

    internal class SqlContext : ITableContext
    {
        StringBuilder @string = new StringBuilder();
        Dictionary<string, object> values = new Dictionary<string, object>();
        public List<string> Names { get; set; } = new List<string>();
        public List<string> Values { get; set; } = new List<string>();
        private readonly ITableContext tableContext;
        public SqlContext(ITableContext tableContext)
        {
            this.tableContext = tableContext;
        }
        public int Length => @string.Length;
        public bool EndWith(string end) => @string.ToString().EndsWith(end);
        public void Insert(int index, string content) => @string.Insert(index, content);

        public void AppendDbParameter(object value)
        {
            var name = $"{GetPrefix()}p{values.Count}";
            @string.Append(name);
            values[name] = value;
        }
        public void AddEntityField(string name, object value)
        {
            var valueName = $"{GetPrefix()}p{values.Count}";
            Names.Add(name);
            Values.Add(valueName);
            values[valueName] = value;
        }

        public void Append(string sql) => @string.Append(sql);
        public void Remove(int index, int count) => @string.Remove(index, count);
        public string Sql() => @string.ToString();
        public void Clear() => @string.Clear();

        #region ITableContext
        public bool SetTableAlias(Type tableName)
        {
            return tableContext.SetTableAlias(tableName);
        }

        public string GetTableAlias(Type tableName)
        {
            return tableContext.GetTableAlias(tableName);
        }

        public string GetTableName(bool withAlias, Type t = null)
        {
            return tableContext.GetTableName(withAlias, t);
        }

        public string GetPrefix()
        {
            return tableContext.GetPrefix();
        }
        #endregion
        public override string ToString()
        {
            return "\n" + paramString();

            string paramString()
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, object> item in values)
                {
                    sb.AppendLine($"[{item.Key},{item.Value}]");
                }
                return sb.ToString();
            }
        }

    }
}
