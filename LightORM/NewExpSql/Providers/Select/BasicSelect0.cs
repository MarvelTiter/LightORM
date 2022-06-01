using MDbContext.NewExpSql.ExpressionVisitor;
using MDbContext.NewExpSql.Interface;
using MDbContext.NewExpSql.Interface.Select;
using MDbContext.SqlExecutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.Providers.Select
{
    internal class BasicSelect0<TSelect, T1> : BasicProvider<T1>, IExpSelect0<TSelect, T1> where TSelect : class
    {
        protected StringBuilder select;
        protected StringBuilder join;
        protected StringBuilder where;
        public BasicSelect0(string key, Func<string, ITableContext> getContext, DbConnectInfo connectInfos)
      : base(key, getContext, connectInfos) { }
        public TSelect Count(out long total)
        {
            select ??= new StringBuilder();
            select.Append("COUNT(1)");
            total = 0;
            return this as TSelect;
        }

        protected void WhereHandle(Expression body)
        {
            where ??= new StringBuilder();
            if (where.Length > 0) where.Append("\nAND ");
            where.Append("(");
            context.SetStringBuilder(where);
            ExpressionVisit.Visit(body, SqlConfig.Where, context);
            where.Append(")");
        }

        protected void JoinHandle<TAnother>(TableLinkType tableLinkType, Expression body)
        {
            context.AddTable(typeof(TAnother), tableLinkType);
            join ??= new StringBuilder();
            context.SetStringBuilder(join);
            ExpressionVisit.Visit(body, SqlConfig.Join, context);
        }

        protected void SelectHandle(Expression body)
        {
            select ??= new StringBuilder();
            select.Clear();
            context.SetStringBuilder(select);
            ExpressionVisit.Visit(body, SqlConfig.Select, context);
        }
        public IEnumerable<T1> ToList()
        {
            return ToList(t1 => new { t1 });
        }
        public IEnumerable<TReturn> ToList<TReturn>()
        {
            Expression<Func<TReturn, object>> exp = r => new { r };
            SelectHandle(exp.Body);
            var conn = dbConnect.CreateConnection();
            return conn.Query<TReturn>(ToSql(), context.GetParameters());
        }
        public IEnumerable<T1> ToList(Expression<Func<T1, object>> exp)
        {
            SelectHandle(exp.Body);
            var conn = dbConnect.CreateConnection();
            return conn.Query<T1>(ToSql(), context.GetParameters());
        }

        protected IEnumerable<TReturn> InternalQuery<TReturn>(string sql, object param)
        {
            var conn = dbConnect.CreateConnection();
            return conn.Query<TReturn>(sql, param);
        }

        public int Execute()
        {
            var conn = dbConnect.CreateConnection();
            return conn.Execute(ToSql(), context.GetParameters());
        }

        public TSelect InnerJoin<TAnother>(Expression<Func<T1, TAnother, bool>> exp)
        {
            JoinHandle<TAnother>(TableLinkType.InnerJoin, exp.Body);
            return this as TSelect;
        }

        public TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
        {
            JoinHandle<TAnother1>(TableLinkType.InnerJoin, exp.Body);
            return this as TSelect;
        }

        public TSelect InnerJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
        {
            JoinHandle<TAnother1>(TableLinkType.InnerJoin, exp.Body);
            return this as TSelect;
        }

        public TSelect LeftJoin<TAnother>(Expression<Func<T1, TAnother, bool>> exp)
        {
            JoinHandle<TAnother>(TableLinkType.LeftJoin, exp.Body);
            return this as TSelect;
        }

        public TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
        {
            JoinHandle<TAnother1>(TableLinkType.LeftJoin, exp.Body);
            return this as TSelect;
        }

        public TSelect LeftJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
        {
            JoinHandle<TAnother1>(TableLinkType.LeftJoin, exp.Body);
            return this as TSelect;
        }

        public TSelect Paging(int pageIndex, int pageSize)
        {
            throw new NotImplementedException();
        }

        public TSelect RightJoin<TAnother>(Expression<Func<T1, TAnother, bool>> exp)
        {
            JoinHandle<TAnother>(TableLinkType.RightJoin, exp.Body);
            return this as TSelect;
        }

        public TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<TAnother1, TAnother2, bool>> exp)
        {
            JoinHandle<TAnother1>(TableLinkType.RightJoin, exp.Body);
            return this as TSelect;
        }

        public TSelect RightJoin<TAnother1, TAnother2>(Expression<Func<T1, TAnother1, TAnother2, bool>> exp)
        {
            JoinHandle<TAnother1>(TableLinkType.RightJoin, exp.Body);
            return this as TSelect;
        }

        public string ToSql()
        {
            var tables = context.Tables.Values.ToArray();
            var main = tables[0];
            StringBuilder sql = new StringBuilder();
            select.Remove(select.Length - 1, 1);
            sql.Append($"SELECT {select} FROM {main.TableName} {main.Alias}");
            for (int i = 1; i < context.Tables.Count; i++)
            {
                var temp = tables[i];
                if (temp.TableType == TableLinkType.None) continue;
                sql.Append($"\n{temp.TableType.ToLabel()} {temp.TableName} {temp.Alias} ON {join}");
            }
            sql.Append($"\nWHERE {where}");
#if DEBUG
            Console.WriteLine(sql.ToString());
#endif
            return sql.ToString();
        }

        public TSelect Where<TAnother>(Expression<Func<TAnother, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this as TSelect;
        }

        public TSelect WhereIf(bool condition, Expression<Func<T1, bool>> exp)
        {
            if (condition) WhereHandle(exp.Body);
            return this as TSelect;
        }

        public TSelect WhereIf<TAnother>(bool condition, Expression<Func<TAnother, bool>> exp)
        {
            if (condition) WhereHandle(exp.Body);
            return this as TSelect;
        }

        public TMember Max<TMember>(Expression<Func<T1, TMember>> exp)
        {
            throw new NotImplementedException();
        }

        public double Sum(Expression<Func<T1, object>> exp)
        {
            throw new NotImplementedException();
        }

        public int Count(Expression<Func<T1, object>> exp)
        {
            throw new NotImplementedException();
        }


    }
}
