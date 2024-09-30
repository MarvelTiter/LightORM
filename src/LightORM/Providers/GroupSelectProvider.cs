using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightORM.Interfaces.ExpSql;

namespace LightORM.Providers
{
    internal class GroupSelectProvider<TGroup, TTables> : IExpSelectGroup<TGroup, TTables>
    {
        private readonly ISqlExecutor executor;
        private readonly SelectBuilder builder;

        public GroupSelectProvider(ISqlExecutor executor, SelectBuilder builder)
        {
            this.executor = executor;
            this.builder = builder;
        }
        public IExpSelectGroup<TGroup, TTables> Having(Expression<Func<IExpSelectGrouping<TGroup, TTables>, bool>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            builder.Expressions.Add(new()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Having
            });
            return this;
        }

        public IExpSelectGroup<TGroup, TTables> OrderBy(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
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

        public IExpSelect<TTemp> ToSelect<TTemp>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            builder.Expressions.Add(new()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select,
                AdditionalParameter = "asc"
            });
            return new SelectProvider1<TTemp>(executor, builder);
        }

        public IExpSelect<TTemp> AsTempQuery<TTemp>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            builder.Expressions.Add(new()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select,
                AdditionalParameter = "asc"
            });
            builder.Level += 1;
            builder.IsSubQuery = true;
            var newBuilder = new SelectBuilder(builder.DbType)
            {
                SubQuery = builder,
            };
            newBuilder.SelectedTables.Add(TableContext.GetTableInfo(typeof(TTemp)));
            return new SelectProvider1<TTemp>(executor, newBuilder);
        }

        public IExpSelectGroup<TGroup, TTables> OrderByDesc(Expression<Func<IExpSelectGrouping<TGroup, TTables>, bool>> exp)
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

        public IExpSelectGroup<TGroup, TTables> Paging(int pageIndex, int pageSize)
        {
            builder.PageIndex = pageIndex;
            builder.PageSize = pageSize;
            return this;
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp)
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

        public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp)
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

        public string ToSql(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
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
