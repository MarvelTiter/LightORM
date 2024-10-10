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
        public SelectBuilder SqlBuilder { get; }
        public ISqlExecutor Executor { get; }

        public GroupSelectProvider(ISqlExecutor executor, SelectBuilder builder)
        {
            this.Executor = executor;
            this.SqlBuilder = builder;
        }
        public IExpSelectGroup<TGroup, TTables> Having(Expression<Func<IExpSelectGrouping<TGroup, TTables>, bool>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            SqlBuilder.Expressions.Add(new()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Having
            });
            return this;
        }

        public IExpSelectGroup<TGroup, TTables> OrderBy(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            SqlBuilder.Expressions.Add(new()
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
            SqlBuilder.Expressions.Add(new()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select,
                AdditionalParameter = "asc"
            });
            return new SelectProvider1<TTemp>(Executor, SqlBuilder);
        }

        public IExpSelect<TTemp> AsTempQuery<TTemp>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            SqlBuilder.Expressions.Add(new()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select,
                AdditionalParameter = "asc"
            });
            SqlBuilder.Level += 1;
            SqlBuilder.IsSubQuery = true;
            var newBuilder = new SelectBuilder(SqlBuilder.DbType)
            {
                SubQuery = SqlBuilder,
            };
            newBuilder.SelectedTables.Add(TableContext.GetTableInfo(typeof(TTemp)));
            return new SelectProvider1<TTemp>(Executor, newBuilder);
        }

        public IExpSelectGroup<TGroup, TTables> OrderByDesc(Expression<Func<IExpSelectGrouping<TGroup, TTables>, bool>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            SqlBuilder.Expressions.Add(new()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Order,
                AdditionalParameter = "desc"
            });
            return this;
        }

        public IExpSelectGroup<TGroup, TTables> Paging(int pageIndex, int pageSize)
        {
            SqlBuilder.PageIndex = pageIndex;
            SqlBuilder.PageSize = pageSize;
            return this;
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select
            });
            var sql = SqlBuilder.ToSqlString();
            var dbParams = SqlBuilder.DbParameters;
            return Executor.Query<TReturn>(sql, dbParams);
        }

        public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select
            });
            var sql = SqlBuilder.ToSqlString();
            var dbParams = SqlBuilder.DbParameters;
            return Executor.QueryAsync<TReturn>(sql, dbParams);
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select
            });
            var sql = SqlBuilder.ToSqlString();
            var dbParams = SqlBuilder.DbParameters;
            return Executor.Query<TReturn>(sql, dbParams);
        }

        public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select
            });
            var sql = SqlBuilder.ToSqlString();
            var dbParams = SqlBuilder.DbParameters;
            return Executor.QueryAsync<TReturn>(sql, dbParams);
        }

        public string ToSql(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Select
            });
            return SqlBuilder.ToSqlString();
        }

        public string ToSql() => SqlBuilder.ToSqlString();


    }
}
