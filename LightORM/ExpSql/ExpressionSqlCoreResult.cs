using MDbContext.SqlExecutor;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DExpSql
{
    public partial class ExpressionSqlCore<T>
    {
        string ToSql() => _sqlCaluse.Sql.ToString();
        public IEnumerable<T> Query()
        {
            return _dbContext.DbConnection.Query<T>(ToSql(), _sqlCaluse.SqlParam);
        }
        public IEnumerable<TReturn> Query<TReturn>()
        {
            return _dbContext.DbConnection.Query<TReturn>(ToSql(), _sqlCaluse.SqlParam);
        }
        public T? FirstOne()
        {
            return _dbContext.DbConnection.QuerySingle<T>(ToSql(), _sqlCaluse.SqlParam);
        }

        public DataTable ExecuteTable()
        {
            return _dbContext.DbConnection.ExecuteTable(ToSql(), _sqlCaluse.SqlParam);
        }

        //===============================Async
#if !NET40
        public Task<IList<T>> QueryAsync()
        {
            return _dbContext.DbConnection.QueryAsync<T>(ToSql(), _sqlCaluse.SqlParam);
        }
        public Task<IList<TReturn>> QueryAsync<TReturn>()
        {
            return _dbContext.DbConnection.QueryAsync<TReturn>(ToSql(), _sqlCaluse.SqlParam);
        }
        public Task<T?> FirstOneAsync()
        {
            return _dbContext.DbConnection.QuerySingleAsync<T>(ToSql(), _sqlCaluse.SqlParam);
        }

        public Task<DataTable> ExecuteTableAsync()
        {
            return _dbContext.DbConnection.ExecuteTableAsync(ToSql(), _sqlCaluse.SqlParam);
        }
#endif

        internal long GetTotal()
        {
            long ret = 0;
            _sqlCaluse.Sql.Remove(selectContentIndexStart, selectContentIndexEnd - selectContentIndexStart);
            _sqlCaluse.Sql.Insert(selectContentIndexStart, "COUNT(*)");
            var result = _dbContext.DbConnection.ExecuteScale(ToSql(), _sqlCaluse.SqlParam);
            _sqlCaluse.Sql.Remove(selectContentIndexStart, 8);
            _sqlCaluse.Sql.Insert(selectContentIndexStart, _sqlCaluse.SelectedFieldString);
            long.TryParse(result?.ToString(), out ret);
            return ret;
        }
    }
}
