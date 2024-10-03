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
        private readonly DeleteBuilder sqlBuilder;
        public DeleteProvider(ISqlExecutor executor, T? entity)
        {
            this.executor = executor;
            sqlBuilder = new DeleteBuilder(this.executor.Database.DbBaseType);
            sqlBuilder.SelectedTables.Add(TableContext.GetTableInfo<T>());
            sqlBuilder.TargetObject = entity;
        }

        public DeleteProvider(ISqlExecutor executor, IEnumerable<T> entities)
        {
            this.executor = executor;
            sqlBuilder = new DeleteBuilder(this.executor.Database.DbBaseType);
            sqlBuilder.SelectedTables.Add(TableContext.GetTableInfo<T>());
            sqlBuilder.TargetObject = entities;
            sqlBuilder.IsDeleteList = true;
        }

        public int Execute()
        {
            var sql = sqlBuilder.ToSqlString();
            var dbParameters = sqlBuilder.DbParameters;
            return executor.ExecuteNonQuery(sql, dbParameters);
        }

        public Task<int> ExecuteAsync()
        {
            var sql = sqlBuilder.ToSqlString();
            var dbParameters = sqlBuilder.DbParameters;
            return executor.ExecuteNonQueryAsync(sql, dbParameters);
        }

        public string ToSql()
        {
            return sqlBuilder.ToSqlString();
        }

        public IExpDelete<T> Where(Expression<Func<T, bool>> exp)
        {
            sqlBuilder.Expressions.Add(new ExpressionInfo
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
