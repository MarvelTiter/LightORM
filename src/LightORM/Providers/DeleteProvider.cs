using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightORM.Builder;
using LightORM.ExpressionSql;
using LightORM.Interfaces.ExpSql;

namespace LightORM.Providers
{
    internal sealed class DeleteProvider<T> : IExpDelete<T>
    {
        private readonly ISqlExecutor executor;
        private readonly DeleteBuilder SqlBuilder;
        public DeleteProvider(ISqlExecutor executor, T? entity)
        {
            this.executor = executor;
            SqlBuilder = new DeleteBuilder(this.executor.ConnectInfo.DbBaseType);
            SqlBuilder.SelectedTables.Add(TableContext.GetTableInfo<T>());
            SqlBuilder.TargetObject = entity;
        }

        public DeleteProvider(ISqlExecutor executor, IEnumerable<T> entities)
        {
            this.executor = executor;
            SqlBuilder = new DeleteBuilder(this.executor.ConnectInfo.DbBaseType);
            SqlBuilder.SelectedTables.Add(TableContext.GetTableInfo<T>());
            SqlBuilder.TargetObject = entities;
            SqlBuilder.IsDeleteList = true;
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

        public IExpDelete<T> Where(Expression<Func<T, bool>> exp)
        {
            SqlBuilder.Expressions.Add(new ExpressionInfo
            {
                ResolveOptions = SqlResolveOptions.DeleteWhere,
                Expression = exp,
            });
            return this;
        }

        public IExpDelete<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            if (condition)
            {
                return Where(exp);
            }
            return this;
        }
    }
}
