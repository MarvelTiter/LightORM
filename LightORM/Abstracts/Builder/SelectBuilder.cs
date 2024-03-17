using LightORM.ExpressionSql;
using LightORM.Extension;
using LightORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightORM.Abstracts.Builder
{
    internal class SelectBuilder : SqlBuilder
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public bool IsDistinct { get; set; }
        public bool IsRollup { get; set; }
       
        

        public override string ToSqlString()
        {
            ResolveExpressions();
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("SELECT {0} \nFROM {1}\n", SelectValue, GetTableName(TableInfo));
            foreach (var item in Joins)
            {
                sb.AppendFormat("{0} {1} ON {2}\n", item.JoinType.ToLabel(), GetTableName(item.EntityInfo!), item.Where);
            }
            if (Where.Count > 0)
            {
                sb.AppendFormat("WHERE {0}", string.Join("\nAND", Where));
            }

            return sb.ToString();
        }
    }
}
