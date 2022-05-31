using System;
using System.Collections.Generic;
using System.Text;

namespace MDbContext.NewExpSql
{
    internal interface ISqlContext : ITableContext
    {
        void Append(string sql);
        int Length { get; }
        void AppendDbParameter(object value);
        void AddEntityField(string name, object value);
        bool EndWith(string end);
        void Insert(int index, string content);
        void Remove(int index, int count);
        void Clear();
        string Sql();
    }
}
