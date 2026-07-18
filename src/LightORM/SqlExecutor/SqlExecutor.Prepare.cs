using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightORM.SqlExecutor;

internal partial class SqlExecutor
{
    private void DisposeCommand(CommandResult result)
    {
        result.Command.Parameters.Clear();
        result.Command.Dispose();
        if (result.NeedToReturn)
        {
            Pool.Return(result.Connection);
        }
    }

    private CommandResult PrepareCommand(CommandType commandType, SqlExecuteContext et)
    {
        var context = CurrentTransactionContext.Value;
        if (context?.IsOccurException == true)
        {
            return new(null!, null!, false, true);
        }
        //DbLog?.Invoke(commandText, dbParameters);
        Interceptor.NotifyPrepareCommand(et);
        DbConnection conn;
        bool needToReturn;
        if (context?.Transaction is not null)
        {
            // 事务操作使用事务连接
            conn = context.Transaction.Connection!;
            needToReturn = false;
        }
        else
        {
            // 非事务操作从池中获取连接
            conn = Pool.Get();
            needToReturn = true;
        }

        if (conn.State != ConnectionState.Open)
        {
            conn.Open();
        }
        var command = conn.CreateCommand();
        Database.DatabaseAdapter.DbCommandInit(command);
        if (context != null)
        {
            command.Transaction = context.Transaction;
        }
        et.HandleDbParameter(Database.DatabaseAdapter.Prefix, command);
        return new(command, conn, needToReturn, false);
    }

    private async Task<CommandResult> PrepareCommandAsync(CommandType commandType, SqlExecuteContext et, CancellationToken cancellationToken = default)
    {
        var context = CurrentTransactionContext.Value;
        if (context?.IsOccurException == true)
        {
            return new(null!, null!, false, true);
        }
        //DbLog?.Invoke(commandText, dbParameters);
        Interceptor.NotifyPrepareCommand(et);
        DbConnection conn;
        bool needToReturn;
        if (context?.Transaction is not null)
        {
            // 事务操作使用事务连接
            conn = context.Transaction.Connection!;
            needToReturn = false;
        }
        else
        {
            // 非事务操作从池中获取连接
            conn = Pool.Get();
            needToReturn = true;
        }

        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        var command = conn.CreateCommand();
        Database.DatabaseAdapter.DbCommandInit(command);

        if (context != null)
        {
            command.Transaction = context.Transaction;
        }
        //command.CommandText = commandText;
        //command.CommandType = commandType;
        //if (dbParameters is not DBNull)
        //{
        //    var action = DbParameterReader.GetDbParameterReader(Database.DatabaseAdapter.Prefix, commandText, dbParameters.GetType());
        //    action?.Invoke(command, dbParameters);
        //}
        et.HandleDbParameter(Database.DatabaseAdapter.Prefix, command);
        return new(command, conn, needToReturn, false);
    }

}
