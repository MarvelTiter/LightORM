using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers
{
    internal class GroupSelectProvider<TGroup, TTables> : IExpGroupSelect<TGroup, TTables>
    {
        private readonly ISqlExecutor executor;
        private readonly SelectBuilder builder;

        public GroupSelectProvider(ISqlExecutor executor, SelectBuilder builder)
        {
            this.executor = executor;
            this.builder = builder;
        }
        public IExpGroupSelect<TGroup, TTables> Having(Expression<Func<IExpGroupSelectResult<TGroup, TTables>, bool>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            builder.Expressions.Add(new()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Having
            });
            return this;
        }

        public IExpGroupSelect<TGroup, TTables> OrderBy(Expression<Func<IExpGroupSelectResult<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            builder.Expressions.Add(new()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Order,
                AdditionalParameter = "asc"
            });
            return this;
        }

        public IExpGroupSelect<TGroup, TTables> OrderByDesc(Expression<Func<IExpGroupSelectResult<TGroup, TTables>, bool>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            builder.Expressions.Add(new()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Order,
                AdditionalParameter = "desc"
            });
            return this;
        }

        public IExpGroupSelect<TGroup, TTables> Paging(int pageIndex, int pageSize)
        {
            builder.PageIndex = pageIndex;
            builder.PageSize = pageSize;
            return this;
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<IExpGroupSelectResult<TGroup, TTables>, TReturn>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            builder.Expressions.Add(new ExpressionInfo()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select
            });
            var sql = builder.ToSqlString();
            var dbParams = builder.DbParameters;
            return executor.Query<TReturn>(sql, dbParams);
        }

        public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<IExpGroupSelectResult<TGroup, TTables>, TReturn>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            builder.Expressions.Add(new ExpressionInfo()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select
            });
            var sql = builder.ToSqlString();
            var dbParams = builder.DbParameters;
            return executor.QueryAsync<TReturn>(sql, dbParams);
        }

        public string ToSql(Expression<Func<IExpGroupSelectResult<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            builder.Expressions.Add(new ExpressionInfo()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select
            });
            return builder.ToSqlString();
        }

        public string ToSql() => builder.ToSqlString();
    }
}
