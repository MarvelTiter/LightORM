using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql
{
    public partial class ExpressionSqlCore<T>
    {
        void SelectHandle(bool distanct)
        {

        }
        void JoinHandle(string joinType, Expression body)
        {
            var context = new SqlContext(tableContext);
            context.Append("(");
            ExpressionVisit.Visit(body, SqlConfig.Join, context);
            context.Append(")");
#if DEBUG
            Console.WriteLine($"JoinHandle: body\n{body} => \n{context}");
#endif
            StoreFragment(SqlPartial.Join);
        }
        void UpdateHandle(Expression body, Expression ignore)
        {
            var context = new SqlContext(tableContext);
            if (ignore != null)
            {
                ExpressionVisit.Visit(ignore, SqlConfig.Update, context);
#if DEBUG
                Console.WriteLine($"UpdateHandle: ignore\n{ignore} => \n{context}");
#endif
            }
            ExpressionVisit.Visit(body, SqlConfig.Update, context);
#if DEBUG
            Console.WriteLine($"UpdateHandle: body\n{body} => \n{context}");
            Console.WriteLine(context.UpdateSql());
#endif
            StoreFragment(SqlPartial.Update);
        }
        void InsertHandle(Expression body)
        {
            var context = new SqlContext(tableContext);
            ExpressionVisit.Visit(body, SqlConfig.Update, context);
#if DEBUG
            Console.WriteLine($"InsertHandle: body\n{body} => \n{context}");
            Console.WriteLine(context.InsertSql());
#endif
            StoreFragment(SqlPartial.Insert);
        }
        void WhereHandle(Expression body)
        {
            sqlContext.Append("(");
            ExpressionVisit.Visit(body, SqlConfig.Where, sqlContext);
            sqlContext.Append(")");
#if DEBUG
            Console.WriteLine($"WhereHandle: body\n{body} => \n{sqlContext}");
#endif
            StoreFragment(SqlPartial.Where);
        }

        void StoreFragment(SqlPartial partial)
        {
            var sql = sqlContext.Store();
            sqls.Add((partial, sql));
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in sqls)
            {
                builder.AppendLine($"{item.Item1}: {item.Item2}");
            }
            return builder.ToString();
        }
    }
}
