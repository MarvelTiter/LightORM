using LightORM.Extension;
using System.Text;

namespace LightORM.Builder;

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
    public SelectBuilder()
    {
        //DbType = dbType;
        IncludeContext = new IncludeContext();
        //Indent = new Lazy<string>(() => new string(' ', 4 * Level));
    }
    public string Id { get; } = $"{Guid.NewGuid():N}";
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
    //private Lazy<string> Indent { get; }
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
    public int TableIndexFix { get; set; }
    public object? AdditionalValue { get; set; }
    public int NextTableIndex => SelectedTables.Count + Joins.Count + TableIndexFix;
    //protected override Lazy<TableInfo[]> GetAllTables()
    //{
    //    return new(() => [.. SelectedTables, .. Joins.Select(j => j.EntityInfo)]);
    //}

    public override IEnumerable<TableInfo> AllTables()
    {
        foreach (var item in SelectedTables)
        {
            yield return item;
        }
        foreach (var item in Joins)
        {
            yield return item.EntityInfo!;
        }
        //return [.. SelectedTables, .. Joins.Select(j => j.EntityInfo!)];
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
            //context.ModifyAlias(t => t.Alias = t.Alias?.Replace("a", $"s{Level}_"));
            context.SetParamPrefix("s");
        }
    }
    protected override void HandleResult(ICustomDatabase database, ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions?.SqlType == SqlPartial.Where)
        {
            if (result.UseNavigate)
            {
                if (result.NavigateDeep == 0) result.NavigateDeep = 1;
                ScanNavigate(database, result, MainTable);
                IsDistinct = true;
                if (result.NavigateWhereExpression.TryGetLambdaExpression(out var l)
                    && l!.Parameters[0].Type == Joins.LastOrDefault()?.EntityInfo?.Type)
                {
                    List<ParameterExpression> ps = [.. AllTables().Select(t => Expression.Parameter(t.TableEntityInfo.Type!))];
                    ps.RemoveAt(ps.Count - 1);
                    var newWhereExpression = Expression.Lambda(l.Body, [.. ps, l.Parameters[0]]);
                    var ee = new ExpressionInfo()
                    {
                        ResolveOptions = SqlResolveOptions.Where,
                        Expression = newWhereExpression,
                    };
                    var eeResult = ee.Expression.Resolve(ee.ResolveOptions, ResolveCtx!);
                    Where.Add(eeResult.SqlString!);
                    if (eeResult.DbParameters?.Count > 0)
                        DbParameterInfos.AddRange(eeResult.DbParameters);
                }
            }
            else
            {
                Where.Add(result.SqlString!);
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
            if (result.UseNavigate)
            {
                ScanNavigate(database, result, MainTable);
                foreach (var item in result.WindowFnPartials ?? [])
                {
                    if (item.Expression is MemberExpression m)
                    {
                        List<ParameterExpression> ps = [.. AllTables().Select(t => Expression.Parameter(t.TableEntityInfo.Type!))];
                        var p = ps.First(p => p.Type == m.Expression?.Type);
                        var lambda = Expression.Lambda(Expression.Property(p, m.Member.Name), ps);
                        var rr = lambda.Resolve(expInfo.ResolveOptions, ResolveCtx!);
                        result.SqlString = result.SqlString?.Replace(item.Idenfity, rr.SqlString);
                    }
                }
            }
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

    private void ScanNavigate(ICustomDatabase database, ExpressionResolvedResult result, TableInfo mainTableInfo)
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
            var targetTable = TableInfo.Create(targetType, NextTableIndex);
            if (navInfo.MappingType != null)
            {
                var targetNav = targetTable.GetNavigateColumns(c => c.NavigateInfo?.MappingType == navInfo.MappingType).First().NavigateInfo!;
                var targetCol = targetTable.GetColumnInfo(targetNav.MainName!);
                var mapTable = TableInfo.Create(navInfo.MappingType, NextTableIndex);
                var subCol = mapTable.GetColumnInfo(navInfo.SubName!);
                //TryJoin(mapTable);
                Joins.Add(new JoinInfo
                {
                    EntityInfo = mapTable,
                    JoinType = TableLinkType.InnerJoin,
                    Where = $"( {mainTableInfo.Alias}.{database.AttachEmphasis(mainCol.ColumnName)} = {mapTable.Alias}.{database.AttachEmphasis(subCol.ColumnName)} )"
                });

                subCol = mapTable.GetColumnInfo(targetNav.SubName!);
                targetTable.Index += 1;
                Joins.Add(new JoinInfo
                {
                    EntityInfo = targetTable,
                    JoinType = TableLinkType.InnerJoin,
                    Where = $"( {targetTable.Alias}.{database.AttachEmphasis(targetCol.ColumnName)} = {mapTable.Alias}.{database.AttachEmphasis(subCol.ColumnName)} )"
                });
            }
            else
            {
                var targetCol = targetTable.GetColumnInfo(navInfo.SubName!);
                //TryJoin(targetTable);
                Joins.Add(new JoinInfo
                {
                    EntityInfo = targetTable,
                    JoinType = TableLinkType.InnerJoin,
                    Where = $"( {mainTableInfo.Alias}.{database.AttachEmphasis(mainCol.ColumnName)} = {targetTable.Alias}.{database.AttachEmphasis(targetCol.ColumnName)} )"
                });
                var n = result.Members?.FirstOrDefault(m => m != navColumn.PropertyName);
                if (n is not null)
                {
                    var c = targetTable.GetColumn(n);
                    if (c is not null)
                    {
                        var where = $"{targetTable.Alias}.{database.AttachEmphasis(c.ColumnName)}{result.SqlString}";
                        Where.Add(where);
                    }
                }
            }

            if (result.NavigateDeep > 1)
            {
                ScanNavigate(database, result, targetTable);
                result.NavigateDeep--;
            }
        }
    }

    private void BuildFromString(StringBuilder sql, ICustomDatabase database)
    {
        if (SelectedTables.Count == 1)
        {
            sql.AppendTableName(database, MainTable);
        }
        else
        {
            bool first = true;
            foreach (var table in SelectedTables)
            {
                if (!first)
                {
                    sql.Append(", ");
                }
                first = false;
                sql.AppendTableName(database, table);
            }
        }
        sql.AppendLine();
    }

    public override string ToSqlString(ICustomDatabase database)
    {
        //SubQuery?.ResolveExpressions();
        StringBuilder sql = new();
        Build(sql, database, Level);
        HandleSqlParameters(sql, database);
        sql.Trim();
        return sql.ToString();

    }

    public void Build(StringBuilder sql, ICustomDatabase database, int currentLevel)
    {
        ResolveExpressions(database);
        var ident = new string(' ', 4 * currentLevel);
        if (InsertInfo.HasValue)
        {
            //sb.AppendLine($"INSERT INTO {database.AttachEmphasis(InsertInfo.Value.TableName)}");
            //sb.AppendLine($"({InsertInfo.Value.InsertColumns})");
            // INSERT INTO table
            // (columns...)
            sql.Append("INSERT INTO ")
                .AppendEmphasis(InsertInfo.Value.TableName, database)
                .AppendLine()
                .Append('(').Append(InsertInfo.Value.InsertColumns).AppendLine(")");
        }

        if (TempViews.Count > 0)
        {
            sql.Append("WITH");
            foreach (var item in TempViews)
            {
                //sql.Append(item.ToSqlString(database));
                //item.Level = Level + 1;
                item.Build(sql, database, currentLevel + 1);
                sql.Append(',');
                DbParameters.TryAddDictionary(item.DbParameters);
            }
            sql.RemoveLast(1);
        }
        if (IsTemp)
        {
            //Level += 1;
            //sb.AppendLine($" {TempName} AS (");
            //$" {TempName} AS (";
            sql.Append(' ').Append(TempName).Append(' ').AppendLine(" AS (");
        }

        //if (GroupBy.Count > 0)
        //{
        //    if (SelectValue == "*")
        //    {
        //        SelectValue = string.Join(",", GroupBy);
        //    }
        //}
        //var dist = IsDistinct ? "DISTINCT " : string.Empty;
        // $"{ident}SELECT {dist}{SelectValue}"
        sql.Append(ident).Append("SELECT ");//.AppendLine(SelectValue);
        if (IsDistinct)
        {
            sql.Append("DISTINCT ");
        }
        if (GroupBy.Count > 0 && SelectValue == "*")
        {
            sql.AppendJoined(GroupBy, ",");
        }
        else
        {
            sql.AppendLine(SelectValue);
        }
        if (SubQuery == null)
        {
            // $"{ident}FROM {BuildFromString(database)}"
            sql.Append(ident).Append("FROM ");
            BuildFromString(sql, database);
        }
        else
        {
            //SubQuery.Level = Level + 1;
            //sb.AppendLine($"{ident}FROM (");
            //sb.Append(SubQuery.ToSqlString(database));
            //sb.AppendLine($"{ident}) {MainTable.Alias}");
            sql.Append(ident).AppendLine("FROM (");
            //.Append(SubQuery.ToSqlString(database))
            SubQuery.Build(sql, database, currentLevel + 1);
            sql.Append(ident).Append(") ").AppendLine(MainTable.Alias);
            DbParameters.TryAddDictionary(SubQuery.DbParameters);
        }

        foreach (var item in Joins)
        {
            if (item.IsSubQuery && item.SubQuery is not null)
            {
                //item.SubQuery!.Level = Level + 1;
                //sb.AppendLine($"{ident}{item.JoinType.ToLabel()} (");
                //sb.Append(item.SubQuery.ToSqlString(database));
                //sb.AppendLine($"{ident}) {item.EntityInfo!.Alias!} ON {item.Where}");
                sql.Append(ident).Append(item.JoinType.ToLabel()).AppendLine(" (");
                //.Append(item.SubQuery.ToSqlString(database))
                item.SubQuery.Build(sql, database, currentLevel + 1);
                sql.Append(ident).Append(") ").Append(item.EntityInfo!.Alias!).Append(" ON ").AppendLine(item.Where);
                DbParameters.TryAddDictionary(item.SubQuery.DbParameters);
            }
            else
            {
                // $"{ident}{item.JoinType.ToLabel()} {GetTableName(database, item.EntityInfo!)} ON {item.Where}"
                sql.Append(ident).Append(item.JoinType.ToLabel()).Append(' ').AppendTableName(database, item.EntityInfo!).Append(" ON ").AppendLine(item.Where);
            }
        }

        if (Where.Count > 0)
        {
            // $"{ident}WHERE {string.Join($" AND ", Where)}"
            sql.Append(ident).Append("WHERE ").AppendJoined(Where, " AND ").AppendLine();
        }
        if (GroupBy.Count > 0)
        {
            if (IsRollup)
            {
                // $"{ident}GROUP BY ROLLUP ({string.Join(", ", GroupBy)})"
                sql.Append(ident).Append("GROUP BY ROLLUP (").AppendJoined(GroupBy, ",").AppendLine(")");
            }
            else
            {
                // $"{ident}GROUP BY {string.Join(", ", GroupBy)}"
                sql.Append(ident).Append("GROUP BY ").AppendJoined(GroupBy, ",").AppendLine();
            }
        }
        if (Having.Count > 0)
        {
            // $"{ident}HAVING {string.Join(", ", Having)}"
            sql.Append(ident).Append("HAVING ").AppendJoined(Having, ",").AppendLine();
        }
        if (OrderBy.Count > 0)
        {
            // $"{ident}ORDER BY {string.Join(", ", OrderBy)} {AdditionalValue}"
            sql.Append(ident).Append("ORDER BY ").AppendJoined(OrderBy, ",").Append(' ');
            if (AdditionalValue is not null)
            {
                sql.Append(AdditionalValue.ToString());
            }
            sql.AppendLine();
        }
        if (Take > 0)
        {
            if (Skip < 0) Skip = 0;
            database.Paging(this, sql);
        }

        if (IsTemp)
        {
            sql.AppendLine(")");
        }

        // union
        if (Unions.Count > 0)
        {
            foreach (var item in Unions)
            {
                //item.SqlBuilder.Level = Level;
                var union = item.IsAll ? "UNION ALL" : "UNION";
                // $"{ident}{union}"
                sql.Append(ident).AppendLine(union);
                item.SqlBuilder.Build(sql, database, currentLevel);
                //sql.Append(item.SqlBuilder.ToSqlString(database));
                DbParameters.TryAddDictionary(item.SqlBuilder.DbParameters);
            }
        }
    }
}
