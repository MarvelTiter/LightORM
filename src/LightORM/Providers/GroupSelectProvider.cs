using LightORM.Utils.Vistors;
using System.Text;

namespace LightORM.Providers
{
    internal class GroupSelectProvider<TGroup, TTables> : IExpSelectGroup<TGroup, TTables>
    {
        public SelectBuilder SqlBuilder { get; }
        public LambdaExpression KeySelector { get; }
        public ISqlExecutor Executor { get; }
        public ICustomDatabase Database => Executor.Database.CustomDatabase;
        public bool IsSubQuery { get; set; }

        public GroupSelectProvider(ISqlExecutor executor, SelectBuilder builder, LambdaExpression keySelector)
        {
            Executor = executor;
            SqlBuilder = builder;
            KeySelector = keySelector;
        }
        public IExpSelectGroup<TGroup, TTables> Having(Expression<Func<IExpSelectGrouping<TGroup, TTables>, bool>> exp)
        {
            var flatExp = FlatGrouping.Default.Flat(exp, KeySelector);
            SqlBuilder.Expressions.Add(new()
            {
                Expression = flatExp,
                ResolveOptions = SqlResolveOptions.Having
            });
            return this;
        }

        public IExpSelectGroup<TGroup, TTables> OrderBy(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatGrouping.Default.Flat(exp, KeySelector);
            this.OrderByHandle(flatExp, true);
            return this;
        }
        public IExpSelectGroup<TGroup, TTables> OrderByDesc(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatGrouping.Default.Flat(exp, KeySelector);
            this.OrderByHandle(flatExp, false);
            return this;
        }
        public IExpSelectGroup<TGroup, TTables> Paging(int pageIndex, int pageSize)
        {
            //SqlBuilder.PageIndex = pageIndex;
            //SqlBuilder.PageSize = pageSize;
            SqlBuilder.Skip = (pageIndex - 1) * pageSize;
            SqlBuilder.Take = pageSize;
            return this;
        }

        public IExpSelectGroup<TGroup, TTables> Skip(int count)
        {
            SqlBuilder.Skip = count;
            return this;
        }

        public IExpSelectGroup<TGroup, TTables> Take(int count)
        {
            SqlBuilder.Take = count;
            return this;
        }

        public IExpSelectGroup<TGroup, TTables> Rollup()
        {
            SqlBuilder.IsRollup = true;
            return this;
        }
        
        public IExpSelect<TTemp> AsTable<TTemp>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp, string? alias = null)
        {
            var flatExp = FlatGrouping.Default.Flat(exp, KeySelector);
            this.HandleResult(flatExp, null);
            return new SelectProvider1<TTemp>(Executor, SqlBuilder);
        }

        //public IExpSelect<TTemp> AsSubQuery<TTemp>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp, string? alias = null)
        //{
        //    var flatExp = FlatGrouping.Default.Flat(exp, KeySelector);
        //    this.HandleResult(flatExp, null);
        //    return this.HandleSubQuery<TTemp>(alias);
        //}

        public IExpTemp<TTemp> AsTemp<TTemp>(string name, Expression<Func<IExpSelectGrouping<TGroup, TTables>, TTemp>> exp)
        {
            var flatExp = FlatGrouping.Default.Flat(exp, KeySelector);
            this.HandleResult(flatExp, null);
            return new TempProvider<TTemp>(name, SqlBuilder);
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp)
        {
            var flatExp = FlatGrouping.Default.Flat(exp, KeySelector);
            this.HandleResult(flatExp, null);
            return this.InternalToList<TReturn>();
        }

        public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, TReturn>> exp)
        {
            var flatExp = FlatGrouping.Default.Flat(exp, KeySelector);
            this.HandleResult(flatExp, null);
            return this.InternalToListAsync<TReturn>();
        }

        public IEnumerable<TReturn> ToList<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatGrouping.Default.Flat(exp, KeySelector);
            this.HandleResult(flatExp, null);
            return this.InternalToList<TReturn>();
        }

        public Task<IList<TReturn>> ToListAsync<TReturn>(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatGrouping.Default.Flat(exp, KeySelector);
            this.HandleResult(flatExp, null);
            return this.InternalToListAsync<TReturn>();
        }

        public string ToSql(Expression<Func<IExpSelectGrouping<TGroup, TTables>, object>> exp)
        {
            var flatExp = FlatGrouping.Default.Flat(exp, KeySelector);
            this.HandleResult(flatExp, null);
            return ToSql();
        }

        public string ToSql() => SqlBuilder.ToSqlString(Database);

        public string ToSqlWithParameters()
        {
            var sql = SqlBuilder.ToSqlString(Database);
            StringBuilder sb = new(sql);
            sb.AppendLine();
            sb.AppendLine("参数列表: ");
            foreach (var item in SqlBuilder.DbParameters)
            {
                sb.AppendLine($"{item.Key} - {item.Value}");
            }
            return sb.ToString();
        }
    }
}
