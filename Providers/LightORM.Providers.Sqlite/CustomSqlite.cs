using LightORM.Implements;
using LightORM.Interfaces;
using System.Data.Common;
using System.Text;

namespace LightORM.Providers.Sqlite;

public sealed class CustomSqlite() : CustomDatabase(new SqliteMethodResolver())
{
    public override string Prefix => "@";
    public override string Emphasis => "``";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.AppendLine($"LIMIT {(builder.PageIndex - 1) * builder.PageSize}, {builder.PageSize}");
    }
    public override string ReturnIdentitySql() => "SELECT LAST_INSERT_ROWID()";
}

public sealed class SqliteProvider : IDatabaseProvider
{
    public DbProviderFactory DbProviderFactory { get; }

    public string MasterConnectionString { get; }

    public string[] SlaveConnectionStrings { get; }
    public SqliteProvider(string master, DbProviderFactory factory, params string[] slaves)
    {
        MasterConnectionString = master;
        DbProviderFactory = factory;
        SlaveConnectionStrings = slaves;
    }
}
