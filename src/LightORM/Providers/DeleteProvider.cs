using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LightORM.Builder;
using LightORM.ExpressionSql;
using LightORM.Interfaces.ExpSql;

namespace LightORM.Providers
{
    internal sealed class DeleteProvider<T> : IExpDelete<T>
    {
        private readonly ISqlExecutor executor;
        private readonly DeleteBuilder<T> sqlBuilder;
        public bool ForceDelete { get => sqlBuilder.ForceDelete; set => sqlBuilder.ForceDelete = value; }
        public bool Truncate { get => sqlBuilder.Truncate; set => sqlBuilder.Truncate = value; }
        public DeleteProvider(ISqlExecutor executor, T? entity)
        {
            this.executor = executor;
            sqlBuilder = new DeleteBuilder<T>(this.executor.Database.DbBaseType);
            sqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
            sqlBuilder.TargetObject = entity;
        }

        public DeleteProvider(ISqlExecutor executor, IEnumerable<T> entities)
        {
            this.executor = executor;
            sqlBuilder = new DeleteBuilder<T>(this.executor.Database.DbBaseType);
            sqlBuilder.SelectedTables.Add(TableInfo.Create<T>());
            sqlBuilder.TargetObjects = entities;
            sqlBuilder.IsBatchDelete = true;
        }


        public int Execute()
        {
            var sql = sqlBuilder.ToSqlString();
            var dbParameters = sqlBuilder.DbParameters;
            return executor.ExecuteNonQuery(sql, dbParameters);
        }

        public Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var sql = sqlBuilder.ToSqlString();
            var dbParameters = sqlBuilder.DbParameters;
            return executor.ExecuteNonQueryAsync(sql, dbParameters, cancellationToken: cancellationToken);
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
