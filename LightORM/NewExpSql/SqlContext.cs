﻿using System;
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
        public TableInfo Table { get; set; }
        public Stack<SqlFieldInfo> Fields { get; set; }
    }

    internal class SqlContext : ITableContext
    {
        StringBuilder? @string;
        Dictionary<string, object> values = new Dictionary<string, object>();
        internal Dictionary<string, TableInfo> Tables { get; set; } = new Dictionary<string, TableInfo>();
        /// <summary>
        /// 本次构建Sql中的字段
        /// </summary>            
        Dictionary<string, SqlFieldInfo> allFields = new Dictionary<string, SqlFieldInfo>();
        public List<string> Names { get; set; } = new List<string>();
        public List<string> Values { get; set; } = new List<string>();
        private readonly ITableContext tableContext;
        public SqlContext(ITableContext tableContext)
        {
            this.tableContext = tableContext;
        }
        public void SetStringBuilder(StringBuilder builder)
        {
            @string = builder;
        }
        public int Length => @string?.Length ?? 0;

        public bool EndWith(string end) => @string?.ToString().EndsWith(end) ?? false;
        public void Insert(int index, string content) => @string?.Insert(index, content);

        public void AppendDbParameter(object value)
        {
            var name = $"{GetPrefix()}p{values.Count}";
            @string?.Append(name);
            values[name] = value;
        }

        public void AddEntityField(string name, object value)
        {
            var valueName = $"{GetPrefix()}p{values.Count}";
            Names.Add(name);
            Values.Add(valueName);
            values[valueName] = value;
        }

        public void Append(string sql) => @string?.Append(sql);
        public void Remove(int index, int count) => @string?.Remove(index, count);
        //public string? Sql() => @string?.ToString();
        public void Clear() => @string?.Clear();

        public object GetParameters() => values;

        public SqlFieldInfo GetColumn(string csName)
        {
            if (allFields.TryGetValue(csName, out var field))
            {
                return field;
            }
            return null;
        }

        #region ITableContext
        public TableInfo AddTable(Type table, TableLinkType tableLinkType = TableLinkType.None)
        {
            var ti = tableContext.AddTable(table, tableLinkType);
            Tables[table.Name] = ti;
            foreach (var item in ti.Fields)
            {
                allFields[item.Key] = item.Value;
            }
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
