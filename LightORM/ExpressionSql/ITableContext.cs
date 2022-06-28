using MDbContext.ExpressionSql.DbHandle;
using System;

namespace MDbContext.ExpressionSql;

internal interface ITableContext
{
    TableInfo AddTable(Type table, TableLinkType tableLinkType);
    string? GetTableAlias(string csName);
    string GetTableName(string csName);
    string? GetTableAlias<T>();
    string GetTableName<T>();
    //string GetPrefix();
    IDbHelper DbHandler { get; }
}
