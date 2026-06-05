using System.Data.Common;
using System.Threading;

namespace LightORM;

public static partial class SqlExecutorExtensions
{
    extension(ISqlExecutor executor)
    {
        public int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text)
        {
            throw new NotImplementedException();
        }

        public T? ExecuteScalar<T>(string commandText, CommandType commandType = CommandType.Text)
        {
            throw new NotImplementedException();
        }

        public DbDataReader ExecuteReader(string commandText, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null)
        {
            throw new NotImplementedException();
        }

        public MultipleResult QueryMultiple(string sql, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null)
        {
            throw new NotImplementedException();
        }


        public DataSet ExecuteDataSet(string commandText, CommandType commandType = CommandType.Text)
        {
            throw new NotImplementedException();
        }


        public DataTable ExecuteDataTable(string commandText, CommandType commandType = CommandType.Text)
        {
            throw new NotImplementedException();
        }


        public Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<T?> ExecuteScalarAsync<T>(string commandText, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<DbDataReader> ExecuteReaderAsync(string commandText, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<MultipleResult> QueryMultipleAsync(string commandText, CommandType commandType = CommandType.Text, CommandBehavior? behavior = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<DataSet> ExecuteDataSetAsync(string commandText, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<DataTable> ExecuteDataTableAsync(string commandText, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
