using LightORM.Cache;
using LightORM.ExpressionSql;
using LightORM.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LightORM.Builder
{
    internal struct UnionItem(SelectBuilder select, bool all)
    {
        public SelectBuilder SqlBuilder { get; set; } = select;
        public bool IsAll { get; set; } = all;
    }

    internal struct SelectInsert(string tableName, string columns)
    {
        public string TableName { get; set; } = tableName;
        public string InsertColumns { get; set; } = columns;
    }

    internal record SelectBuilder : SqlBuilder, ISelectSqlBuilder
    {
        public SelectBuilder(DbBaseType dbType) : base(dbType)
        {
            DbType = dbType;
            IncludeContext = new IncludeContext(dbType);
            Indent = new Lazy<string>(() => new string(' ', 4 * Level));
        }
        public string Id { get; } = $"{Guid.NewGuid():N}";
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        private Lazy<string> Indent { get; }
        public List<SelectBuilder> TempViews { get; } = [];
        public SelectBuilder? SubQuery { get; set; }
        public List<UnionItem> Unions { get; } = [];
        public bool IsSubQuery { get; set; }
        public bool IsTemp { get; set; }
        public bool IsUnion { get; set; }
        public SelectInsert? InsertInfo { get; set; }
        public int UnionIndex { get; set; }
        //public bool UseTemp { get; set; }
        public string? TempName { get; set; }
        public bool IsDistinct { get; set; }
        public bool IsRollup { get; set; }
        public string SelectValue { get; set; } = "*";
        public int Level { get; set; }
        public List<JoinInfo> Joins { get; set; } = [];
        public List<string> Having { get; set; } = [];
        public List<IncludeInfo> Includes { get; set; } = [];
        public IncludeContext IncludeContext { get; set; } = default!;

        public List<string> GroupBy { get; set; } = [];
        public List<string> OrderBy { get; set; } = [];
        public object? AdditionalValue { get; set; }

        protected override Lazy<ITableEntityInfo[]> GetAllTables()
        {
            return new(() => [.. SelectedTables, .. Joins.Select(j => j.EntityInfo)]);
        }

        protected override void BeforeResolveExpressions(ResolveContext context)
        {
            context.Level = Level;
            if (IsTemp)
            {
                context.SetParamPrefix(TempName);
            }
            else if (IsSubQuery)
            {
                context.ModifyAlias(t => t.Alias = t.Alias?.Replace("a", $"s{Level}_"));
                context.SetParamPrefix("s");
            }
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
                    if (expInfo.AdditionalParameter is int i && i > 0)
                    {
                        if (i > SelectedTables.Count - 1)
                        {
                            LightOrmException.Throw($"当前Select的表的数量是{SelectedTables.Count}, 已超出可以Join的数量");
                        }
                        joinInfo.EntityInfo = SelectedTables[i];
                    }
                }
            }
            else if (expInfo.ResolveOptions?.SqlType == SqlPartial.Select)
            {
                if (!string.IsNullOrWhiteSpace(result.SqlString))
                {
                    SelectValue = result.SqlString!;
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
            if (SelectedTables.Count == 1 || Joins.Count > 0)
            {
                return GetTableName(MainTable);
            }
            return string.Join(", ", SelectedTables.Select(t => GetTableName(t)));
        }

        public override string ToSqlString()
        {
            //SubQuery?.ResolveExpressions();
            ResolveExpressions();
            StringBuilder sb = new StringBuilder();
            if (InsertInfo.HasValue)
            {
                sb.AppendLine($"INSERT INTO {AttachEmphasis(InsertInfo.Value.TableName)}");
                sb.AppendLine($"({InsertInfo.Value.InsertColumns})");
            }

            if (TempViews.Count > 0)
            {
                sb.Append("WITH ");
                foreach (var item in TempViews)
                {
                    sb.Append(item.ToSqlString());
                    sb.Append(',');
                    DbParameters.TryAddDictionary(item.DbParameters);
                }
                sb.Remove(sb.Length - 1, 1);
            }
            if (IsTemp)
            {
                Level += 1;
                sb.AppendLine($"{TempName} AS (");
            }

            if (GroupBy.Count > 0)
            {
                var groupby = string.Join(",", GroupBy);
                if (SelectValue == "*")
                {
                    SelectValue = groupby;
                }
            }
            var dist = IsDistinct ? "DISTINCT " : "";
            sb.AppendLine($"{Indent.Value}SELECT {dist}{SelectValue}");

            if (SubQuery == null)
            {
                sb.AppendLine($"{Indent.Value}FROM {BuildFromString()}");
            }
            else
            {
                SubQuery.Level = Level + 1;
                sb.AppendLine($"{Indent.Value}FROM (");
                sb.Append(SubQuery.ToSqlString());
                sb.AppendLine($"{Indent.Value}) {AttachEmphasis(MainTable.Alias!)}");
                DbParameters.TryAddDictionary(SubQuery.DbParameters);
            }

            foreach (var item in Joins)
            {
                if (item.IsSubQuery)
                {
                    item.SubQuery!.Level = Level + 1;
                    sb.AppendLine($"{Indent.Value}{item.JoinType.ToLabel()} (");
                    sb.Append(item.SubQuery.ToSqlString());
                    sb.AppendLine($"{Indent.Value}) {AttachEmphasis(item.EntityInfo!.Alias!)} ON {item.Where}");
                    DbParameters.TryAddDictionary(item.SubQuery.DbParameters);
                }
                else
                {
                    sb.AppendLine($"{Indent.Value}{item.JoinType.ToLabel()} {GetTableName(item.EntityInfo!)} ON {item.Where}");
                }
            }

            if (Where.Count > 0)
            {
                sb.AppendLine($"{Indent.Value}WHERE {string.Join($" AND ", Where)}");
            }
            if (GroupBy.Count > 0)
            {
                if (IsRollup)
                {
                    sb.AppendLine($"{Indent.Value}GROUP BY ROLLUP ({string.Join(", ", GroupBy)})");
                }
                else
                {
                    sb.AppendLine($"{Indent.Value}GROUP BY {string.Join(", ", GroupBy)}");
                }
            }
            if (Having.Count > 0)
            {
                sb.AppendLine($"{Indent.Value}HAVING {string.Join(", ", Having)}");
            }
            if (OrderBy.Count > 0)
            {
                sb.AppendLine($"{Indent.Value}ORDER BY {string.Join(", ", OrderBy)} {AdditionalValue}");
            }
            if (PageIndex * PageSize > 0)
            {
                DbHelper.Paging(this, sb);
            }

            if (IsTemp)
            {
                sb.AppendLine(")");
            }

            // union
            if (Unions.Count > 0)
            {
                foreach (var item in Unions)
                {
                    item.SqlBuilder.Level = Level;
                    var union = item.IsAll ? "UNION ALL" : "UNION";
                    sb.AppendLine($"{Indent.Value}{union}");
                    sb.Append(item.SqlBuilder.ToSqlString());
                    DbParameters.TryAddDictionary(item.SqlBuilder.DbParameters);
                }
            }

            return sb.ToString();
        }
    }
}
