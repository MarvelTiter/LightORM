using LightORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightORM.Implements;

namespace LightORM.Providers.PostgreSQL;

public sealed class PostgreSQLProvider : BaseDatabaseProvider
{
    public static PostgreSQLProvider Create(string master, params string[] slaves) => new PostgreSQLProvider(master, slaves);
    private static readonly Lazy<IDatabaseTableHandler> lazyHandler = new(() => new PostgreSQLTableHandler());
    public override DbBaseType DbBaseType => DbBaseType.PostgreSQL;

    private PostgreSQLProvider(string master, params string[] slaves) : base(master, slaves)
    {
    }


    public override ICustomDatabase CustomDatabase { get; } = CustomPostgreSQL.Instance;

    public override Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => throw new NotSupportedException();

    public override IDatabaseTableHandler DbHandler => lazyHandler.Value;
    public override DbProviderFactory DbProviderFactory { get; set; } = Npgsql.NpgsqlFactory.Instance;

    public override int BulkCopy(DataTable dataTable)
    {
        throw new NotImplementedException();
    }
}