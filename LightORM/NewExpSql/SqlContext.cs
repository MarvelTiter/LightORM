using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDbContext.NewExpSql
{
    internal interface ISqlContext
    {
        int Length { get; }
        bool EndWith(string end);
        void Insert(int index, string content);
        void AppendDbParameter(object value);
        void AddEntityField(string name, object value);
        void Append(string sql);
        void AddField(string fieldName, string parameterName, string tableAlias = "", string FieldAlias = "");
        string ToSql();
    }
    internal class SqlFieldInfo
    {
        public string FieldName { get; set; }
        public string? TableAlias => Table?.Alias;
        public string FieldAlias { get; set; }
        public string ParameterName { get; set; }
        public string Compare { get; set; }
        public TableInfo Table { get; set; }
    }

    internal class SqlFragment
    {
        public StringBuilder Sql { get; set; } = new StringBuilder();
        public List<string> Names { get; set; } = new List<string>();
        public List<string> Values { get; set; } = new List<string>();

        public int Length => Sql.Length;
        public StringBuilder Append(string content) => Sql.Append(content);
        public StringBuilder Clear() => Sql.Clear();
        public StringBuilder Remove(int startIndex, int length) => Sql.Remove(startIndex, length);
        public StringBuilder Insert(int index, string content) => Sql.Insert(index, content);
        public override string ToString()
        {
            return Sql.ToString();
        }
    }

    internal class SqlContext : ITableContext
    {
        SqlFragment? fragment;
        Dictionary<string, object> values = new Dictionary<string, object>();
        internal Dictionary<string, TableInfo> Tables { get; set; } = new Dictionary<string, TableInfo>();
        /// <summary>
        /// 本次构建Sql中的字段
        /// </summary>            
        List<Dictionary<string, SqlFieldInfo>> allFields = new List<Dictionary<string, SqlFieldInfo>>();
        private readonly ITableContext tableContext;
        public SqlContext(ITableContext tableContext)
        {
            this.tableContext = tableContext;
        }

        public void SetFragment(SqlFragment fragment)
        {
            this.fragment = fragment;
        }
        public int Length => fragment?.Length ?? 0;

        public bool EndWith(string end) => fragment?.Sql.ToString().EndsWith(end) ?? false;
        public void Insert(int index, string content) => fragment?.Insert(index, content);

        public void AppendDbParameter(object value)
        {
            var name = $"{GetPrefix()}p{values.Count}";
            fragment?.Append(name);
            values[name] = value;
            if (fragment?.Names.Count - fragment?.Values.Count == 1)
            {
                fragment?.Values.Add(name);
            }
        }

        public void AddEntityField(string name, object value)
        {
            var valueName = $"{GetPrefix()}p{values.Count}";
            fragment?.Names.Add(name);
            fragment?.Values.Add(valueName);
            values[valueName] = value;
        }

        public void AddFieldName(string name)
        {
            fragment?.Names.Add(name);
        }

        public void Append(string sql) => fragment?.Append(sql);
        public void Remove(int index, int count) => fragment?.Remove(index, count);
        //public string? Sql() => @string?.ToString();
        public void Clear() => fragment?.Clear();

        public object GetParameters() => values;

        public SqlFieldInfo GetColumn(string csName)
        {
            SqlFieldInfo field = null;
            allFields.FirstOrDefault(dic => dic.TryGetValue(csName, out field));
            return field;
        }

        #region ITableContext
        public TableInfo AddTable(Type table, TableLinkType tableLinkType = TableLinkType.None)
        {
            var ti = tableContext.AddTable(table, tableLinkType);
            Tables[table.Name] = ti;
            allFields.Add(ti.Fields);
            return ti;
        }

        public string? GetTableAlias(string csName)
        {
            return tableContext.GetTableAlias(csName);
        }

        public string GetTableName(string csName)
        {
            return tableContext.GetTableName(csName);
        }

        public string? GetTableAlias<T>()
        {
            return tableContext.GetTableAlias<T>();
        }

        public string GetTableName<T>()
        {
            return tableContext.GetTableName<T>();
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


        public static SqlContext operator +(SqlContext self, string sql)
        {
            self.Append(sql);
            return self;
        }
        public static SqlContext operator -(SqlContext self, string sql)
        {
            var start = self.Length - sql.Length;
            self.Remove(start, sql.Length);
            return self;
        }

    }
}
