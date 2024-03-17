using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightORM.Abstracts.Builder;
using LightORM.ExpressionSql;

namespace LightORM.Providers
{
    internal sealed class DeleteProvider<T> : IExpDelete<T>
    {
        private readonly ISqlExecutor executor;
        private readonly DeleteBuilder SqlBuilder = new DeleteBuilder();
        
        public DeleteProvider(ISqlExecutor executor, T? entity)
        {
            this.executor = executor;
            SqlBuilder.DbType = this.executor.ConnectInfo.DbBaseType;
            SqlBuilder.TableInfo = Cache.TableContext.GetTableInfo<T>();
            SqlBuilder.TargetObject = entity;
        }
        public IExpDelete<T> AppendData(T item)
        {
            SqlBuilder.TargetObject = item;
            return this;
        }

        public int Execute()
        {
            var sql = SqlBuilder.ToSqlString();
            var dbParameters = SqlBuilder.DbParameters;
            return executor.ExecuteNonQuery(sql, dbParameters);
        }

        public Task<int> ExecuteAsync()
        {
            var sql = SqlBuilder.ToSqlString();
            var dbParameters = SqlBuilder.DbParameters;
            return executor.ExecuteNonQueryAsync(sql, dbParameters);
        }

        public string ToSql()
        {
            return SqlBuilder.ToSqlString();
        }

        public IExpDelete<T> Where(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public IExpDelete<T> Where(Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }

        public IExpDelete<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }
    }
}
