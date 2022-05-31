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
            sqlContext.Append("(");
            ExpressionVisit.Visit(body, SqlConfig.Join, sqlContext);
            sqlContext.Append(")");
            StoreFragment(SqlPartial.Join);
        }
        void UpdateHandle(Expression body, Expression ignore)
        {
            if (ignore != null)
            {
                ExpressionVisit.Visit(ignore, SqlConfig.Update, sqlContext);
            }
            ExpressionVisit.Visit(body, SqlConfig.Update, sqlContext);
            sqlContext.Append($"UPDATE {tableContext.GetTableName(false)} SET \n{sqlContext.UpdateSql()}");
            StoreFragment(SqlPartial.Update);
        }
        void InsertHandle(Expression body)
        {
            ExpressionVisit.Visit(body, SqlConfig.Update, sqlContext);
            var sql = sqlContext.InsertSql();//("", "");//
            sqlContext.Append($"INSERT INTO ({sql.Item1}) \nVALUES ({sql.Item2})");
            StoreFragment(SqlPartial.Insert);
        }
        void WhereHandle(Expression body)
        {
            sqlContext.Append("(");
            ExpressionVisit.Visit(body, SqlConfig.Where, sqlContext);
            sqlContext.Append(")");
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
                builder.AppendLine($"{item.Item1}: \n{item.Item2}");
            }
            builder.Append(sqlContext.ToString());
            return builder.ToString();
        }
    }
}
