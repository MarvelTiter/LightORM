using System;
using System.Collections.Generic;
using System.Text;

namespace MDbContext.NewExpSql
{
    internal struct EntityField
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    internal static class SqlContextExtend
    {
        public static string UpdateSql(this ISqlContext self)
        {
            StringBuilder sb = new StringBuilder();
            foreach (EntityField item in self.Fields)
            {
                sb.AppendLine($"[{item.Name} = {item.Value}]");
            }
            return sb.ToString();
        }

        public static string InsertSql(this ISqlContext self)
        {
            StringBuilder sb = new StringBuilder();
            foreach (EntityField item in self.Fields)
            {
                sb.AppendLine($"{item.Name}, {item.Value}");
            }
            return sb.ToString();
        }

        public static string Store(this ISqlContext self)
        {
            var sql = self.Sql();
            self.Clear();
            return sql;
        }
    }
    internal class SqlContext : ISqlContext
    {
        StringBuilder @string = new StringBuilder();
        Dictionary<string, object> values = new Dictionary<string, object>();
        public List<EntityField> Fields { get; set; } = new List<EntityField>();
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
            Fields.Add(new EntityField { Name = name, Value = valueName });
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
            var sql = @string.ToString();
            return sql + "\n" + paramString();

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
