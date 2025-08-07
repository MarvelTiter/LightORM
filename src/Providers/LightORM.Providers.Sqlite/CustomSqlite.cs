using LightORM.Implements;
using LightORM.Interfaces;
using System.Text;

namespace LightORM.Providers.Sqlite;

public sealed class CustomSqlite() : CustomDatabase(new SqliteMethodResolver())
{
    internal readonly static CustomSqlite Instance = new CustomSqlite();
    public override string Prefix => "@";
    public override string Emphasis => "``";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.AppendLine($"LIMIT {builder.Skip}, {builder.Take}");
    }
    public override string ReturnIdentitySql() => "SELECT LAST_INSERT_ROWID()";
}
