using LightORM.Builder;
using System.Linq;
using System.Threading.Tasks;

namespace LightORM.Providers
{
    internal class UpdateProvider<T> : IExpUpdate<T>
    {
        private readonly ISqlExecutor executor;
        UpdateBuilder SqlBuilder = new UpdateBuilder();
        public UpdateProvider(ISqlExecutor executor, T? entity)
        {
            this.executor = executor;
            SqlBuilder.DbType = this.executor.ConnectInfo.DbBaseType;
            SqlBuilder.TableInfo = Cache.TableContext.GetTableInfo<T>();
            SqlBuilder.TargetObject = entity;
        }

        public UpdateProvider(ISqlExecutor executor, IEnumerable<T> entities)
        {
            this.executor = executor;
            SqlBuilder.DbType = this.executor.ConnectInfo.DbBaseType;
            SqlBuilder.TableInfo = Cache.TableContext.GetTableInfo<T>();
            SqlBuilder.TargetObject = entities;
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



        public IExpUpdate<T> Set<TField>(Expression<Func<T, TField>> exp, TField value)
        {
            //TODO null处理
            var result = exp.Resolve(SqlResolveOptions.Update);
            var member = result.Members!.First();
            SqlBuilder.Members.Add(member);
            SqlBuilder.DbParameters.Add(member, value!);
            return this;
        }

        public IExpUpdate<T> SetIf<TField>(bool condition, Expression<Func<T, TField>> exp, TField value)
        {
            if (condition)
            {
                return Set(exp, value);
            }
            return this;
        }

        public IExpUpdate<T> UpdateColumns(Expression<Func<object>> columns)
        {
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = columns,
                ResolveOptions = SqlResolveOptions.Update,
            });
            return this;
        }
        public IExpUpdate<T> UpdateColumns(Expression<Func<T, object>> columns)
        {
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = columns,
                ResolveOptions = SqlResolveOptions.Update,
            });
            return this;
        }

        public IExpUpdate<T> IgnoreColumns(Expression<Func<T, object>> columns)
        {
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = columns,
                ResolveOptions = SqlResolveOptions.UpdateIgnore
            });
            return this;
        }

        public IExpUpdate<T> Where(Expression<Func<T, bool>> exp)
        {
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = exp,
                ResolveOptions = SqlResolveOptions.UpdateWhere
            });
            return this;
        }

        public IExpUpdate<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            if (condition)
            {
                return Where(exp);
            }
            return this;
        }
        public string ToSql() => SqlBuilder.ToSqlString();

       
    }
}
