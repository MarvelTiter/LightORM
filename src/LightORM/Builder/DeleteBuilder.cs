using LightORM.Extension;
using System.Text;

namespace LightORM.Builder;
internal record DeleteBuilder<T> : SqlBuilder
{
    public new T? TargetObject { get; set; }
    public IEnumerable<T> TargetObjects { get; set; } = [];
    private bool batchDone = false;
    public bool IsBatchDelete { get; set; }
    public bool ForceDelete { get; set; }
    public bool Truncate { get; set; }
    public List<BatchSqlInfo>? BatchInfos { get; set; }
    protected override void HandleResult(ICustomDatabase database, ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions?.SqlType == SqlPartial.Where)
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
                    var navSqlBuilder = new SelectBuilder
                    {
                        IsSubQuery = true,
                        Level = 1
                    };
                    navSqlBuilder.SelectedTables.Add(MainTable);
                    var navInfo = navColumn.NavigateInfo!;
                    var mainCol = MainTable.GetColumnInfo(navInfo.MainName!);
                    var targetType = navInfo.NavigateType;
                    var targetTable = TableInfo.Create(targetType, navSqlBuilder.NextTableIndex);

                    navSqlBuilder.SelectValue = $"{database.AttachEmphasis(MainTable.Alias)}.{database.AttachEmphasis(mainCol.ColumnName)}";
                    if (navInfo.MappingType != null)
                    {
                        var targetNav = targetTable.GetNavigateColumns(c => c.NavigateInfo?.MappingType == navInfo.MappingType).First().NavigateInfo!;
                        var targetCol = targetTable.GetColumnInfo(targetNav.MainName!);
                        var mapTable = TableInfo.Create(navInfo.MappingType, 1);
                        var subCol = mapTable.GetColumnInfo(navInfo.SubName!);
                        //TryJoin(mapTable);
                        navSqlBuilder.Joins.Add(new JoinInfo
                        {
                            EntityInfo = mapTable,
                            JoinType = TableLinkType.InnerJoin,
                            Where = $"( {database.AttachEmphasis(MainTable.Alias)}.{database.AttachEmphasis(mainCol.ColumnName)} = {database.AttachEmphasis(mapTable.Alias)}.{database.AttachEmphasis(subCol.ColumnName)} )"
                        });

                        subCol = mapTable.GetColumnInfo(targetNav.SubName!);
                        targetTable.Index += 1;
                        navSqlBuilder.Joins.Add(new JoinInfo
                        {
                            EntityInfo = targetTable,
                            JoinType = TableLinkType.InnerJoin,
                            Where = $"( {database.AttachEmphasis(targetTable.Alias)}.{database.AttachEmphasis(targetCol.ColumnName)} = {database.AttachEmphasis(mapTable.Alias)}.{database.AttachEmphasis(subCol.ColumnName)} )"
                        });
                        if (result.NavigateWhereExpression.TryGetLambdaExpression(out var l)
                        && l!.Parameters[0].Type == navSqlBuilder.Joins.LastOrDefault()?.EntityInfo?.Type)
                        {
                            List<ParameterExpression> ps = [.. navSqlBuilder.AllTables().Select(t => Expression.Parameter(t.TableEntityInfo.Type!))];
                            ps.RemoveAt(ps.Count - 1);
                            var newWhereExpression = Expression.Lambda(l.Body, [.. ps, l.Parameters[0]]);
                            var ee = new ExpressionInfo()
                            {
                                ResolveOptions = SqlResolveOptions.Where,
                                Expression = newWhereExpression,
                            };
                            var eeResult = ee.Expression.Resolve(ee.ResolveOptions, ResolveCtx!);
                            navSqlBuilder.Where.Add(eeResult.SqlString!);
                            if (eeResult.DbParameters?.Count > 0)
                                DbParameterInfos.AddRange(eeResult.DbParameters);
                        }
                    }
                    else
                    {
                        var targetCol = targetTable.GetColumnInfo(navInfo.SubName!);
                        //TryJoin(targetTable);
                        navSqlBuilder.Joins.Add(new JoinInfo
                        {
                            EntityInfo = targetTable,
                            JoinType = TableLinkType.InnerJoin,
                            Where = $"( {database.AttachEmphasis(MainTable.Alias)}.{database.AttachEmphasis(mainCol.ColumnName)} = {database.AttachEmphasis(targetTable.Alias)}.{database.AttachEmphasis(targetCol.ColumnName)} )"
                        });
                        var n = result.MemberOfNavigateMember;
                        if (n is not null)
                        {
                            var c = targetTable.GetColumn(n);
                            if (c is not null)
                            {
                                string mainColWhere;
                                var indexOfLeft = result.SqlString?.IndexOf('(');
                                if (indexOfLeft > -1)
                                {
                                    mainColWhere = result.SqlString!.Insert(indexOfLeft.Value + 1, $"{database.AttachEmphasis(targetTable.Alias)}.{database.AttachEmphasis(c.ColumnName)}");
                                }
                                else
                                {
                                    mainColWhere = $"{database.AttachEmphasis(targetTable.Alias)}.{database.AttachEmphasis(c.ColumnName)}{result.SqlString}";
                                }
                                navSqlBuilder.Where.Add(mainColWhere);
                            }
                        }
                    }

                    Where.Add($"{database.AttachEmphasis(mainCol.ColumnName)} IN ({N}{navSqlBuilder.ToSqlString(database)})");
                }
            }
            else
            {
                Where.Add(result.SqlString!);
            }
        }
    }
    private void CreateBatchDeleteSql(ICustomDatabase database)
    {
        if (batchDone)
        {
            return;
        }
        ResolveExpressions(database);
        var columns = MainTable.TableEntityInfo.Columns
                   .Where(c => c.IsPrimaryKey || c.IsVersionColumn).ToArray();

        BatchInfos = columns.GenBatchInfos(TargetObjects.ToList(), 2000 - DbParameters.Count);
        var delete = $"DELETE FROM {GetTableName(database, MainTable, false)}";
        foreach (var batch in BatchInfos)
        {
            StringBuilder sb = new();
            sb.AppendLine(delete);
            List<string> autoWhereList = [];
            foreach (var p in batch.Parameters)
            {
                var where = string.Join(" AND ", p.Select(c => $"{c.ColumnName} = {c.ParameterName}"));
                autoWhereList.Add($"({where})");
            }
            var autoWhere = string.Join(" OR ", autoWhereList);
            Where.Add(autoWhere);
            sb.AppendLine($"WHERE {string.Join($"{N}AND ", Where)}");
            HandleSqlParameters(sb, database);
            batch.Sql = sb.ToString();
        }
        batchDone = true;
    }
    public override string ToSqlString(ICustomDatabase database)
    {
        //TODO 处理批量删除
        if (IsBatchDelete)
        {
            CreateBatchDeleteSql(database);
            return string.Join(",", BatchInfos?.Select(b => b.Sql) ?? []);
        }
        ResolveExpressions(database);
        if (ForceDelete)
        {
            if (Truncate)
            {
                return $"TRUNCATE TABLE {GetTableName(database, MainTable, false)}";
            }
            else
            {
                return $"DELETE FROM {GetTableName(database, MainTable, false)}";
            }
        }
        else
        {
            // 没有设置Where条件, 且提供实体值, 则使用主键作为Where条件
            if (Where.Count == 0)
            {
                if (TargetObject is null) LightOrmException.Throw("Where Condition is null and not provider a entity value");
                var primary = MainTable.TableEntityInfo.Columns.Where(f => f.IsPrimaryKey).ToArray();
                if (primary.Length == 0) LightOrmException.Throw($"Where Condition is null and Model of [{MainTable.Type}] do not has a PrimaryKey");
                var wheres = primary.Select(c =>
                 {
                     DbParameters.Add(c.ColumnName, c.GetValue(TargetObject!)!);
                     return $"{database.AttachEmphasis(c.ColumnName)} = {database.AttachPrefix(c.ColumnName)}";
                 });
                Where.AddRange(wheres);
            }
            StringBuilder sql;
            sql = new("DELETE FROM ");
            sql.AppendLine(GetTableName(database, MainTable, false));
            if (Where.Count > 0)
            {
                sql.AppendLine($"WHERE {string.Join(" AND ", Where)}");
            }
            HandleSqlParameters(sql, database);
            return sql.Trim();
        }
    }


}
