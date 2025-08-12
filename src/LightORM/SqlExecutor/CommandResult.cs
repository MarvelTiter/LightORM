using System.Data.Common;
namespace LightORM.SqlExecutor;

internal readonly struct CommandResult(DbCommand command, DbConnection connection, bool needToReturn, bool @break)
{
    public DbCommand Command { get; } = command;
    public DbConnection Connection { get; } = connection;
    public bool NeedToReturn { get; } = needToReturn;
    public bool Break { get; } = @break;
}
