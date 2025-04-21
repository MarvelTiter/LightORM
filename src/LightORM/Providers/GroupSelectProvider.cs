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
        public bool IsSubQuery { get; set; }

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
            this.OrderByHandle(flatExp, true);
            return this;
        }
        public IExpSelectGroup<TGroup, TTables> OrderByDesc(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            this.OrderByHandle(flatExp, false);
            return this;
        }
        public IExpSelectGroup<TGroup, TTables> Paging(int pageIndex, int pageSize)
        {
            SqlBuilder.PageIndex = pageIndex;
            SqlBuilder.PageSize = pageSize;
            return this;
        }
        public IExpSelectGroup<TGroup, TTables> Rollup()
        {
            SqlBuilder.IsRollup = true;
            return this;
        }
        public IExpSelectGroup<TGroup, TTables> Rollup(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            // TODO 没想好怎么写
            return this;
        }
        public IExpSelect<TTemp> AsTable<TTemp>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp, string? alias = null)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            this.HandleResult(flatExp, null);
            return new SelectProvider1<TTemp>(Executor, SqlBuilder);
        }

        //public IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp, string? alias = null)
        //{
        //    var flatExp = FlatTypeSet.Default.Flat(exp);
        //    this.HandleResult(flatExp, null);
        //    return this.HandleSubQuery<TTemp>(alias);
        //}

        public IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            this.HandleResult(flatExp, null);
            return new TempProvider<TTemp>(name, SqlBuilder);
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            this.HandleResult(flatExp, null);
            return this.InternalToList<TReturn>();
        }

        public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            this.HandleResult(flatExp, null);
            return this.InternalToListAsync<TReturn>();
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            this.HandleResult(flatExp, null);
            return this.InternalToList<TReturn>();
        }

        public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            this.HandleResult(flatExp, null);
            return this.InternalToListAsync<TReturn>();
        }

        public string ToSql(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatTypeSet.Default.Flat(exp);
            this.HandleResult(flatExp, null);
            return ToSql();
        }

        public string ToSql() => SqlBuilder.ToSqlString();

        
    }
}
