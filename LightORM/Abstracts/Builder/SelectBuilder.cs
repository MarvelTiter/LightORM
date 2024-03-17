using LightORM.Context;
using LightORM.ExpressionSql;
using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Abstracts.Builder
{
    internal class SelectBuilder : ISqlBuilder
    {
        public IExpressionInfo Expressions { get; }
        public DbBaseType DbType { get; set; }
        public TableEntity TableInfo { get; set; }
        public Dictionary<string, object> DbParameters { get; } = [];
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public bool IsDistinct { get; set; }
        public bool IsRollup { get; set; }
        public string? SelectValue { get; set; }
        public List<JoinInfo> Joins { get; set; } = [];
        public List<string> Where { get; set; } = [];
        public List<string> GroupBy { get; set; } = [];
        public List<string> OrderBy { get; set; } = [];
        public SelectBuilder()
        {
            Expressions = new ExpressionInfoProvider();
            TableInfo = new();
        }

        public virtual void ResolveExpressions()
        {
            if (Expressions.Completed)
            {
                return;
            }

            foreach (var item in Expressions.ExpressionInfos)
            {
                if (item.Completed)
                {
                    continue;
                }
                item.ResolveOptions!.DbType = DbType;
                var result = item.Expression.Resolve(item.ResolveOptions!);
                item.Completed = true;
                if (!string.IsNullOrEmpty(item.Template))
                {
                    result.SqlString = string.Format(item.Template, result.SqlString);
                }
                if (item.ResolveOptions?.SqlType == SqlPartial.Where)
                {
                    Where.Add(result.SqlString!);
                }
                else if (item.ResolveOptions?.SqlType == SqlPartial.Join)
                {
                    var joinInfo = Joins.FirstOrDefault(j => j.ExpressionId == item.Id);
                    if (joinInfo != null)
                    {
                        joinInfo.Where = result.SqlString!;
                    }
                }
                else if (item.ResolveOptions?.SqlType == SqlPartial.Select)
                {
                    if (!string.IsNullOrWhiteSpace(result.SqlString))
                    {
                        SelectValue = result.SqlString;
                    }
                }
                else if (item.ResolveOptions?.SqlType == SqlPartial.GroupBy)
                {
                    SelectValue = result.SqlString;
                    GroupBy.Add(result.SqlString!);
                }
                else if (item.ResolveOptions?.SqlType == SqlPartial.OrderBy)
                {
                    OrderBy.Add(result.SqlString!);
                }

                DbParameters.TryAddDictionary(result.DbParameters);
            }
        }

        private string GetTableName(TableEntity table)
        {
            return $"{DbType.AttachEmphasis(table.TableName!)} {DbType.AttachEmphasis(table.Alias!)}";
        }

        public string ToSqlString()
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
