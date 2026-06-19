using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace LightORM;

public static partial class SqlExecutorExtensions
{
    extension(ISqlExecutor executor)
    {
        public ExecuteResult<TParameter> Execute<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TParameter>(string sql, TParameter? param, DbTransaction? trans = null, CommandType commandType = CommandType.Text)
        {
            return new(executor, sql, param, trans, commandType);
        }

        public ExecuteResult Execute(string sql, DbTransaction? trans = null, CommandType commandType = CommandType.Text)
        {
            return new ExecuteResult(executor, sql, trans, commandType);
        }


        public int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text)
            => executor.ExecuteNonQuery(commandText, NullDbParameter.Instance, commandType);

        public ScalarValue ExecuteScalar(string commandText, CommandType commandType = CommandType.Text)
            => executor.ExecuteScalar(commandText, NullDbParameter.Instance, commandType);

        public DbDataReader ExecuteReader(string commandText, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null)
             => executor.ExecuteReader(commandText, NullDbParameter.Instance, commandType, behavior);

        public MultipleResult QueryMultiple(string sql, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null)
            => executor.QueryMultiple(sql, NullDbParameter.Instance, commandType, behavior);

        public DataSet ExecuteDataSet(string commandText, CommandType commandType = CommandType.Text)
            => executor.ExecuteDataSet(commandText, NullDbParameter.Instance, commandType);

        public DataTable ExecuteDataTable(string commandText, CommandType commandType = CommandType.Text)
            => executor.ExecuteDataTable(commandText, NullDbParameter.Instance, commandType);

        public Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
             => executor.ExecuteNonQueryAsync(commandText, NullDbParameter.Instance, commandType, cancellationToken);

        public Task<ScalarValue> ExecuteScalarAsync(string commandText, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
            => executor.ExecuteScalarAsync(commandText, NullDbParameter.Instance, commandType, cancellationToken);

        public Task<DbDataReader> ExecuteReaderAsync(string commandText, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null, CancellationToken cancellationToken = default)
            => executor.ExecuteReaderAsync(commandText, NullDbParameter.Instance, commandType, behavior, cancellationToken);

        public Task<MultipleResult> QueryMultipleAsync(string commandText, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null, CancellationToken cancellationToken = default)
            => executor.QueryMultipleAsync(commandText, NullDbParameter.Instance, commandType, behavior, cancellationToken);

        public Task<DataSet> ExecuteDataSetAsync(string commandText, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
            => executor.ExecuteDataSetAsync(commandText, NullDbParameter.Instance, commandType, cancellationToken);

        public Task<DataTable> ExecuteDataTableAsync(string commandText, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
            => executor.ExecuteDataTableAsync(commandText, NullDbParameter.Instance, commandType, cancellationToken);
    }
}
