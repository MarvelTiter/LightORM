using System;

namespace MDbContext.NewExpSql
{
    internal interface ITableContext
    {
        bool SetTableAlias(Type tableName);
        string GetTableAlias(Type tableName);
        string GetTableName(bool withAlias, Type t = null);
        string GetPrefix();
    }
}
