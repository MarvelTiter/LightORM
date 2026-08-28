using LightORM.Extension;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LightORM.Builder;
//#if NET8_0_OR_GREATER
//[System.Diagnostics.CodeAnalysis.RequiresDynamicCode("[Delete]语句使用了导航属性，需要动态构建表达式，AOT可能存在问题")]
//#endif
internal class DeleteBuilder<T> : SqlBuilder
{
    //public new T? TargetObject { get; set; }
    public T[] TargetObjects { get; set; } = [];
    private bool batchDone = false;
    public bool IsBatchDelete { get; set; }
    public bool FullDelete { get; set; }
    public bool Truncate { get; set; }
    HashSet<string> Members { get; set; } = [];
    public List<BatchSqlInfo>? BatchInfos { get; set; }
#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "根据已知类型构建导航条件")]
    [UnconditionalSuppressMessage("AOT", "IL2026", Justification = "根据已知类型构建导航条件")]
#endif
    protected override void HandleResult(IDatabaseAdapter database, ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions.SqlType == SqlPartial.Where)
        {
            if (result.UseNavigate)
            {
                if (result.NavigateDeep == 0) result.NavigateDeep = 1;
                foreach (var navColumn in MainTable.GetNavigateColumns())
                {
                    if (!result.NavigateMembers!.Contains(navColumn.PropertyName))
                    {
                        continue;
                    }
                    var navSqlBuilder = SelectBuilder.GetSelectBuilder();
                    navSqlBuilder.IsSubQuery = true;
                    navSqlBuilder.Depth = 1;
                    navSqlBuilder.AddTableInfo(MainTable with { Depth = 1 });
                    var selectMain = navSqlBuilder.MainTable;
                    var navInfo = navColumn.NavigateInfo!;
                    var mainCol = selectMain.GetColumnInfo(navInfo.MainName!);
                    var targetType = navInfo.NavigateType;
                    var targetTable = TableInfo.Create(targetType, navSqlBuilder.NextTableIndex);
                    //navSqlBuilder.SelectValue = $"{selectMain.Alias}.{database.AttachEmphasis(mainCol.ColumnName)}";
                    var mainParameter = Expression.Parameter(selectMain.Type);
                    var tarParameter = Expression.Parameter(targetType);
                    var mainMember = Expression.Property(mainParameter, navInfo.MainName!);
                    if (navInfo.MappingType != null)
                    {
                        var mappingParameter = Expression.Parameter(navInfo.MappingType);
                        var targetNav = targetTable.GetNavigateColumns(c => c.NavigateInfo?.MappingType == navInfo.MappingType).First().NavigateInfo!;
                        var targetCol = targetTable.GetColumnInfo(targetNav.MainName!);
                        //var mapTable = TableInfo.Create(navInfo.MappingType, 1);
                        //var subCol = mapTable.GetColumnInfo(navInfo.SubName!);
                        //TryJoin(mapTable);
                        //navSqlBuilder.AddJoin(new JoinInfo(mapTable)
                        //{
                        //    JoinType = TableLinkType.InnerJoin,
                        //    Where = $"( {selectMain.Alias}.{database.AttachEmphasis(mainCol.ColumnName)} = {mapTable.Alias}.{database.AttachEmphasis(subCol.ColumnName)} )"
                        //});
                        var subMember = Expression.Property(mappingParameter, navInfo.SubName!);
                        var body = Expression.Equal(mainMember, subMember);
                        var joinWhere = Expression.Lambda(body, mainParameter, mappingParameter);
                        navSqlBuilder.JoinHandle(navInfo.MappingType, joinWhere, TableLinkType.InnerJoin);

                        //subCol = mapTable.GetColumnInfo(targetNav.SubName!);
                        //targetTable.Index += 1;
                        //navSqlBuilder.AddJoin(new JoinInfo(targetTable)
                        //{
                        //    JoinType = TableLinkType.InnerJoin,
                        //    Where = $"( {targetTable.Alias}.{database.AttachEmphasis(targetCol.ColumnName)} = {mapTable.Alias}.{database.AttachEmphasis(subCol.ColumnName)} )"
                        //});

                        var tarMainMember = Expression.Property(tarParameter, targetNav.MainName!);
                        var tarSubMember = Expression.Property(mappingParameter, targetNav.SubName!);
                        var tarBody = Expression.Equal(tarMainMember, tarSubMember);
                        var joinTarWhere = Expression.Lambda(tarBody, mainParameter, mappingParameter, tarParameter);
                        navSqlBuilder.JoinHandle(navInfo.NavigateType, joinTarWhere, TableLinkType.InnerJoin);

                        
                    }
                    else
                    {
                        var tarMainMember = Expression.Property(mainParameter, navInfo.MainName!);
                        var tarSubMember = Expression.Property(tarParameter, navInfo.SubName!);
                        var tarBody = Expression.Equal(tarMainMember, tarSubMember);
                        var joinTarWhere = Expression.Lambda(tarBody, mainParameter, tarParameter);
                        navSqlBuilder.JoinHandle(navInfo.NavigateType, joinTarWhere, TableLinkType.InnerJoin);
                        //var targetCol = targetTable.GetColumnInfo(navInfo.SubName!);
                        //TryJoin(targetTable);
                        //navSqlBuilder.AddJoin(new JoinInfo(targetTable)
                        //{
                        //    JoinType = TableLinkType.InnerJoin,
                        //    Where = $"( {selectMain.Alias}.{database.AttachEmphasis(mainCol.ColumnName)} = {targetTable.Alias}.{database.AttachEmphasis(targetCol.ColumnName)} )"
                        //});
                        //var n = result.MemberOfNavigateMember;
                        //if (n is not null)
                        //{
                        //    var c = targetTable.GetColumn(n);
                        //    if (c is not null)
                        //    {
                        //        string mainColWhere;
                        //        var indexOfLeft = result.SqlString?.IndexOf('(');
                        //        if (indexOfLeft > -1)
                        //        {
                        //            mainColWhere = result.SqlString!.Insert(indexOfLeft.Value + 1, $"{targetTable.Alias}.{database.AttachEmphasis(c.ColumnName)}");
                        //        }
                        //        else
                        //        {
                        //            mainColWhere = $"{targetTable.Alias}.{database.AttachEmphasis(c.ColumnName)}{result.SqlString}";
                        //        }
                        //        navSqlBuilder.Where.Add(mainColWhere);
                        //    }
                        //}
                    }

                    if (result.NavigateWhereExpression.TryGetLambdaExpression(out var l)
                        && l!.Parameters[0].Type == navSqlBuilder.GetJoins().LastOrDefault()?.EntityInfo?.Type)
                    {

                        List<ParameterExpression> ps = [.. navSqlBuilder.AllTables().Select(t => Expression.Parameter(t.TableEntityInfo.Type!))];
                        ps.RemoveAt(ps.Count - 1);
                        var newWhereExpression = Expression.Lambda(l.Body, [.. ps, l.Parameters[0]]);
                        var ee = new ExpressionInfo(SqlResolveOptions.Where, newWhereExpression);
                        navSqlBuilder.Expressions.Add(ee);
                    }

                    navSqlBuilder.Expressions.Add(new( SqlResolveOptions.Select,Expression.Lambda(mainMember, mainParameter)));
                    using var _ = StringBuilderPool.Get(out var sb);
                    navSqlBuilder.Build(sb, database, navSqlBuilder.Depth);
                    Where.Add($"{database.AttachEmphasis(mainCol.ColumnName)} IN ({N}{sb})");
                    ResolvedValues.AddRange(navSqlBuilder.ResolvedValues);
                }
            }
            else
            {
                Where.Add(result.SqlString!);
                Members.AddRange(result.Members);
            }
        }
    }
    private void CreateBatchDeleteSql(IDatabaseAdapter database)
    {
        if (batchDone)
        {
            return;
        }
        ResolveExpressions(database);
        var columns = MainTable.TableEntityInfo.Columns
                   .Where(c => c.IsPrimaryKey || c.IsVersionColumn).ToArray();
        if (columns.Length == 0 && Where.Count == 0)
        {
            throw new LightOrmException("没有主键并且未设置Where条件");
        }
        BatchInfos = columns.GenBatchInfos(TargetObjects, database, 2000 - DbParameters.Count);
        //var delete = $"DELETE FROM {GetTableName(database, MainTable, false)}";
        database.HandleBatchDelete<T>(new(this, columns, BatchInfos, DbParameters, database));

        batchDone = true;
    }
    public override string ToSqlString(IDatabaseAdapter database)
    {
        if (QuoteIdentifiers.HasValue)
        {
            database = new ScopedDatabaseAdapter(database, QuoteIdentifiers.Value);
        }
        if (IsBatchDelete)
        {
            CreateBatchDeleteSql(database);
            // ToSqlString由内部或者测试项目调用，批量情况下查看SQL使用BatchInfos属性
            return string.Empty;
            //return string.Join(",", BatchInfos?.Select(b => b.Sql) ?? []);
        }

        ResolveExpressions(database);
        using var _ = StringBuilderPool.Get(out var sql);
        WriteTags(sql);
        if (FullDelete)
        {
            if (Truncate)
            {
                sql.Append("TRUNCATE TABLE ");
                sql.Append(GetTableName(database, MainTable, false));
            }
            else
            {
                sql.Append("DELETE FROM ");
                sql.Append(GetTableName(database, MainTable, false));
            }
            return sql.ToString();
        }

        if (Where.Count == 0 && TargetObject is null)
        {
            throw new LightOrmException("Where Condition is null and not provider a entity value");
        }
        sql.Append("DELETE FROM ");
        //sql.AppendLine(GetTableName(database, MainTable, false));
        sql.AppendTableName(database, MainTable, false).AppendLine();
        // 没有设置Where条件, 且提供实体值, 则使用主键作为Where条件
        bool first = true;
        if (TargetObject is not null)
        {
            var keyedColumns = MainTable.TableEntityInfo.Columns.Where(f => f.IsPrimaryKey || f.IsVersionColumn).ToArray();
            if (keyedColumns.Length == 0)
            {
                throw new LightOrmException($"Where Condition is null and Model of [{MainTable.Type}] do not has a PrimaryKey");
            }
            //var wheres = keyedColumns.Select(c =>
            //{
            //    DbParameters.Add(c.ColumnName, c.GetValue(TargetObject!)!);
            //    return $"{database.AttachEmphasis(c.ColumnName)} = {database.AttachPrefix(c.ColumnName)}";
            //});
            //Where.AddRange(wheres);
            sql.Append("WHERE ");
            foreach (var col in keyedColumns)
            {
                if (Members.Contains(col.PropertyName))
                {
                    continue;
                }
                DbParameters.Add(col.PropertyName, col.GetValue(TargetObject)!);
                if (!first)
                {
                    sql.Append(" AND ");
                }
                first = false;
                sql.Append('(');
                sql.AppendEmphasis(col.ColumnName, database);
                sql.Append(" = ");
                sql.WithPrefix(col.PropertyName, database);
                sql.Append(')');
            }
        }

        if (Where.Count > 0)
        {
            if (first)
            {
                sql.Append("WHERE ");
            }
            else
            {
                sql.Append(" AND ");
            }
            //sql.AppendLine($"WHERE {string.Join(" AND ", Where)}");
            sql.AppendJoined(Where, " AND ");
        }
        HandleSqlParameters(sql, database);
        return sql.Trim();
    }
}
