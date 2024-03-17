using LightORM.ExpressionSql.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers
{
    internal class UpdateProvider<T> : IExpUpdate<T>
    {
        private readonly ISqlExecutor executor;

        public UpdateProvider(ISqlExecutor executor, T? entity)
        {
            this.executor = executor;
            // SqlBuilder.DbType = this.executor.ConnectInfo.DbBaseType;
            // SqlBuilder.TableInfo = Cache.TableContext.GetTableInfo<T>();
            // SqlBuilder.TargetObject = entity;
        }
        public IExpUpdate<T> AppendData(T item)
        {
            throw new NotImplementedException();
        }

        public int Execute()
        {
            throw new NotImplementedException();
        }

        public Task<int> ExecuteAsync()
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> IgnoreColumns(Expression<Func<T, object>> columns)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp, object value)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> exp, object value)
        {
            throw new NotImplementedException();
        }

        public string ToSql()
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> UpdateColumns(Expression<Func<object>> columns)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> Where(T item)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> Where(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> Where(Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }

        public IExpUpdate<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }
    }
}
