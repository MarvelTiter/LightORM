using LightORM.Cache;
using LightORM.ExpressionSql;
using LightORM.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Builder
{
    internal class SelectBuilder : SqlBuilder
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public bool IsDistinct { get; set; }
        public bool IsRollup { get; set; }
        public string SelectValue { get; set; } = "*";
        public List<JoinInfo> Joins { get; set; } = [];
        public List<string> GroupBy { get; set; } = [];
        public List<string> OrderBy { get; set; } = [];
        public List<string> Having { get; set; } = [];
        public List<IncludeInfo> Includes { get; set; } = [];
        public IncludeContext IncludeContext { get; set; } = default!;

        public object? AdditionalValue { get; set; }

        protected override Lazy<ITableEntityInfo[]> GetAllTables()
        {
            return new(() => [.. SelectedTables, .. Joins.Select(j => j.EntityInfo)]);
        }

        protected override void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result)
        {
            if (expInfo.ResolveOptions?.SqlType == SqlPartial.Where)
            {
                Where.Add(result.SqlString!);
                if (result.UseNavigate)
                {
                    if (result.NavigateDeep == 0) result.NavigateDeep = 1;
                    ScanNavigate(result, MainTable);
                    IsDistinct = true;
                }
            }
            else if (expInfo.ResolveOptions?.SqlType == SqlPartial.Join)
            {
                var joinInfo = Joins.FirstOrDefault(j => j.ExpressionId == expInfo.Id);
                if (joinInfo != null)
                {
                    joinInfo.Where = result.SqlString!;
                }
            }
            else if (expInfo.ResolveOptions?.SqlType == SqlPartial.Select)
            {
                if (!string.IsNullOrWhiteSpace(result.SqlString) && !MainTable.IsAnonymousType)
                {
                    SelectValue = result.SqlString;
                }
            }
            else if (expInfo.ResolveOptions?.SqlType == SqlPartial.GroupBy)
            {
                GroupBy.Add(result.SqlString!);
            }
            else if (expInfo.ResolveOptions?.SqlType == SqlPartial.OrderBy)
            {
                OrderBy.Add(result.SqlString!);
                AdditionalValue = expInfo.AdditionalParameter;
            }
            else if (expInfo.ResolveOptions?.SqlType == SqlPartial.Having)
            {
                Having.Add(result.SqlString!);
            }
        }

        private void ScanNavigate(ExpressionResolvedResult result, ITableEntityInfo mainTableInfo)
        {
            foreach (var navColumn in mainTableInfo.GetNavigateColumns())
            {
                if (!result.NavigateMembers!.Contains(navColumn.PropertyName))
                {
                    continue;
                }
                var navInfo = navColumn.NavigateInfo!;
                var mainCol = mainTableInfo.GetColumnInfo(navInfo.MainName!);
                var targetType = navInfo.NavigateType;
                var targetTable = TableContext.GetTableInfo(targetType);
                if (navInfo.MappingType != null)
                {
                    var targetNav = targetTable.GetNavigateColumns(c => c.NavigateInfo?.MappingType == navInfo.MappingType).First().NavigateInfo!;
                    var targetCol = targetTable.GetColumnInfo(targetNav.MainName!);
                    var mapTable = TableContext.GetTableInfo(navInfo.MappingType);
                    var subCol = mapTable.GetColumnInfo(navInfo.SubName!);
                    TryJoin(mapTable);
                    Joins.Add(new JoinInfo
                    {
                        EntityInfo = mapTable,
                        JoinType = TableLinkType.LeftJoin,
                        Where = $"( {AttachEmphasis(mainTableInfo.Alias!)}.{AttachEmphasis(mainCol.ColumnName)} = {AttachEmphasis(mapTable.Alias!)}.{AttachEmphasis(subCol.ColumnName)} )"
                    });

                    subCol = mapTable.GetColumnInfo(targetNav.SubName!);
                    TryJoin(targetTable);
                    Joins.Add(new JoinInfo
                    {
                        EntityInfo = targetTable,
                        JoinType = TableLinkType.LeftJoin,
                        Where = $"( {AttachEmphasis(targetTable.Alias!)}.{AttachEmphasis(targetCol.ColumnName)} = {AttachEmphasis(mapTable.Alias!)}.{AttachEmphasis(subCol.ColumnName)} )"
                    });
                }
                else
                {
                    var targetCol = targetTable.GetColumnInfo(navInfo.SubName!);
                    TryJoin(targetTable);
                    Joins.Add(new JoinInfo
                    {
                        EntityInfo = targetTable,
                        JoinType = TableLinkType.LeftJoin,
                        Where = $"( {AttachEmphasis(mainTableInfo.Alias!)}.{AttachEmphasis(mainCol.ColumnName)} = {AttachEmphasis(targetTable.Alias!)}.{AttachEmphasis(targetCol.ColumnName)} )"
                    });
                }

                if (result.NavigateDeep > 1)
                {
                    ScanNavigate(result, targetTable);
                    result.NavigateDeep--;
                }
            }
        }

        void TryJoin(ITableEntityInfo joined)
        {
            if (Joins.Any(j => j.EntityInfo?.Type == joined.Type) || MainTable.Type == joined.Type)
            {
                joined.Alias = (joined.Alias!.Replace('a', 'j'));
            }
        }

        private string BuildFromString()
        {
            if (SelectedTables.Count == 0)
            {
                return GetTableName(MainTable);
            }
            return string.Join(", ", SelectedTables.Select(t => GetTableName(t)));
        }

        public override string ToSqlString()
        {
            ResolveExpressions();
            StringBuilder sb = new StringBuilder();

            if (GroupBy.Count > 0)
            {
                var groupby = string.Join(",", GroupBy);
                if (SelectValue == "*")
                {
                    SelectValue = groupby;
                }
                //else
                //{
                //    SelectValue = $"{groupby}, {SelectValue}";
                //}
            }

            sb.AppendFormat("SELECT {0} \nFROM {1}\n", SelectValue, BuildFromString());
            if (IsDistinct)
            {
                sb.Insert(6, " DISTINCT");
            }
            foreach (var item in Joins)
            {
                sb.AppendFormat("{0} {1} ON {2}\n", item.JoinType.ToLabel(), GetTableName(item.EntityInfo!), item.Where);
            }
            if (Where.Count > 0)
            {
                sb.AppendFormat("WHERE {0}\n", string.Join("\nAND ", Where));
            }
            if (GroupBy.Count > 0)
            {
                sb.AppendFormat("GROUP BY {0}\n", string.Join(", ", GroupBy));
            }
            if (Having.Count > 0)
            {
                sb.AppendFormat("HAVING {0}\n", string.Join(", ", Having));
            }
            if (OrderBy.Count > 0)
            {
                sb.AppendFormat("ORDER BY {0} {1}\n", string.Join("\n, ", OrderBy), $"{AdditionalValue}");
            }
            if (PageIndex * PageSize > 0)
            {
                DbHelper.Paging(this, sb);
            }

            return sb.ToString();
        }
    }
}
