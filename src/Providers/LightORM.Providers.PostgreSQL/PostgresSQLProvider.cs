using LightORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.PostgreSQL;

public sealed class PostgreSQLProvider : IDatabaseProvider
{
    public static PostgreSQLProvider Create(string master, params string[] slaves) => new PostgreSQLProvider(master, slaves);
    public DbBaseType DbBaseType => DbBaseType.PostgreSQL;

    public PostgreSQLProvider(string master, params string[] slaves)
    {
        MasterConnectionString = master;
        SlaveConnectionStrings = slaves;
    }

    public string MasterConnectionString { get; }

    public ICustomDatabase CustomDatabase { get; } = CustomPostgreSQL.Instance;

    public Func<TableGenerateOption, IDatabaseTableHandler>? TableHandler { get; } = option => new PostgreSQLTableHandler(option);

    public string[] SlaveConnectionStrings { get; }

    public DbProviderFactory DbProviderFactory { get; internal set; } = Npgsql.NpgsqlFactory.Instance;

    public int BulkCopy(DataTable dataTable)
    {
        throw new NotImplementedException();
    }
}
