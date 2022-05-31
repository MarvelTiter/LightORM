using MDbContext.NewExpSql.ExpressionVisitor;
using MDbContext.NewExpSql.Interface;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.Providers
{
    internal partial class SelectProvider<T> : BasicProvider<T>, IExpSelect<T>
    {
        StringBuilder? select;
        StringBuilder? join;
        StringBuilder? where;
        public SelectProvider(string key, Func<string, (ITableContext context, DbConnectInfo info)> getDbInfos)
       : base(key, getDbInfos) { }
        public IExpSelect<T> ToList(Expression<Func<T, bool>> exp)
        {
            throw new NotImplementedException();
        }
        public IExpSelect<T> InnerJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle<T1>(TableLinkType.INNERJOIN, exp.Body);
            return this;
        }

        public IExpSelect<T> LeftJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle<T1>(TableLinkType.LEFTJOIN, exp.Body);
            return this;
        }

        public IExpSelect<T> RightJoin<T1>(Expression<Func<T, T1, object>> exp)
        {
            JoinHandle<T1>(TableLinkType.RIGHTJOIN, exp.Body);
            return this;
        }

        public string ToSql()
        {
            var main = tables[0];
            StringBuilder sql = new StringBuilder();
            sql.Append($"SELECT {select} FROM ");
            sql.Append(main.TableName);
            sql.Append(main.Alias);
            for (int i = 1; i < tables.Count; i++)
            {
                var temp = tables[i];
                sql.Append($"\n{temp.TableType} {temp.TableName} {temp.Alias} ON {join}");
            }
            sql.Append($"\nWHERE {where}");
            return sql.ToString();
        }

        public IExpSelect<T> Where(Expression<Func<T, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public IExpSelect<T> Where<T1>(Expression<Func<T1, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public IExpSelect<T> Where<T1>(Expression<Func<T, T1, bool>> exp)
        {
            WhereHandle(exp.Body);
            return this;
        }

        public IExpSelect<T> WhereIf(bool condition, Expression<Func<T, bool>> exp)
        {
            if (condition) WhereHandle(exp.Body);
            return this;
        }

        public int Execute()
        {
            throw new NotImplementedException();
        }

        public IExpSelect<T> Count(out long total)
        {
            select ??= new StringBuilder();
            select.Append("COUNT(1)");
            total = 0;
            return this;
        }

        internal void JoinHandle<TAnother>(TableLinkType tableLinkType, Expression body)
        {
            tables.Add(context.AddTable(typeof(TAnother), tableLinkType));
            join ??= new StringBuilder();
            context.SetStringBuilder(join);
            ExpressionVisit.Visit(body, SqlConfig.Join, context);
        }

        internal void WhereHandle(Expression body)
        {
            where ??= new StringBuilder();
            if (where.Length > 0) where.Append("\nAND ");
            where.Append("(");
            context.SetStringBuilder(where);
            ExpressionVisit.Visit(body, SqlConfig.Where, context);
            where.Append(")");
        }
    }
}
