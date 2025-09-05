using LightORM;
using LightORM.DbStruct;
using LightORM.Implements;
using LightORM.Providers.SqlServer.TableStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.SqlServer;

public sealed class SqlServerTableHandler(TableGenerateOption option, SqlServerProvider provider)
    : BaseDatabaseHandler<SqlServerTableWriter, SqlServerTableReader>
{
    private SqlServerTableWriter? _writer;
    private SqlServerTableReader? _reader;

    protected override SqlServerTableWriter Writer => _writer ??= new SqlServerTableWriter(option);

    protected override SqlServerTableReader Reader => _reader ??= new SqlServerTableReader(CreateSqlExecutor(provider));
}
