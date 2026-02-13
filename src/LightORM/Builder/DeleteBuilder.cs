using LightORM.Extension;
using System.Text;

namespace LightORM.Builder;

internal record DeleteBuilder<T> : SqlBuilder
{
    public new T? TargetObject { get; set; }
    public T[] TargetObjects { get; set; } = [];
    private bool batchDone = false;
    public bool IsBatchDelete { get; set; }
    public bool FullDelete { get; set; }
    public bool Truncate { get; set; }
    HashSet<string> Members { get; set; } = [];
    public List<BatchSqlInfo>? BatchInfos { get; set; }
    protected override void HandleResult(ICustomDatabase database, ExpressionInfo expInfo, ExpressionResolvedResult result)
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
                    navSqlBuilder.Level = 1;
                    navSqlBuilder.SelectedTables.Add(MainTable);
                    var navInfo = navColumn.NavigateInfo!;
                    var mainCol = MainTable.GetColumnInfo(navInfo.MainName!);
                    var targetType = navInfo.NavigateType;
                    var targetTable = TableInfo.Create(targetType, navSqlBuilder.NextTableIndex);

                    navSqlBuilder.SelectValue = $"{MainTable.Alias}.{database.AttachEmphasis(mainCol.ColumnName)}";
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
                            Where = $"( {MainTable.Alias}.{database.AttachEmphasis(mainCol.ColumnName)} = {mapTable.Alias}.{database.AttachEmphasis(subCol.ColumnName)} )"
                        });

                        subCol = mapTable.GetColumnInfo(targetNav.SubName!);
                        targetTable.Index += 1;
                        navSqlBuilder.Joins.Add(new JoinInfo
                        {
                            EntityInfo = targetTable,
                            JoinType = TableLinkType.InnerJoin,
                            Where = $"( {targetTable.Alias}.{database.AttachEmphasis(targetCol.ColumnName)} = {mapTable.Alias}.{database.AttachEmphasis(subCol.ColumnName)} )"
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
                            Where = $"( {MainTable.Alias}.{database.AttachEmphasis(mainCol.ColumnName)} = {targetTable.Alias}.{database.AttachEmphasis(targetCol.ColumnName)} )"
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
                                    mainColWhere = result.SqlString!.Insert(indexOfLeft.Value + 1, $"{targetTable.Alias}.{database.AttachEmphasis(c.ColumnName)}");
                                }
                                else
                                {
                                    mainColWhere = $"{targetTable.Alias}.{database.AttachEmphasis(c.ColumnName)}{result.SqlString}";
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
                Members.AddRange(result.Members);
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
        if (columns.Length == 0 && Where.Count == 0)
        {
            throw new LightOrmException("没有主键并且未设置Where条件");
        }
        BatchInfos = columns.GenBatchInfos(TargetObjects, 2000 - DbParameters.Count);
        //var delete = $"DELETE FROM {GetTableName(database, MainTable, false)}";
        foreach (var batch in BatchInfos)
        {
            StringBuilder sb = new("DELETE FROM ");
            //sb.AppendLine(GetTableName(database, MainTable, false));
            sb.AppendTableName(database, MainTable, false).AppendLine();
            sb.Append("WHERE ");
            if (TargetObjects.Length == 0)
            {
                sb.Append("1=0");
                batch.Sql = sb.ToString();
                break;
            }
            sb.Append('(');
            for (int rowIndex = 0; rowIndex < batch.Parameters.Count; rowIndex++)
            {
                List<SimpleColumn>? row = batch.Parameters[rowIndex];
                if (columns.Length > 1)
                {
                    sb.Append('(');
                    for (var i = 0; i < row.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(" AND ");
                        }
                        sb.AppendEmphasis(row[i].ColumnName, database);
                        sb.Append(" = ");
                        sb.WithPrefix(row[i].ParameterName, database);
                    }
                    sb.Append(')');
                    if (rowIndex < batch.Parameters.Count - 1)
                        sb.Append(" OR ");
                }
                else
                {
                    if (rowIndex == 0)
                    {
                        // 这里直接访问索引0是安全的，因为进入到else分支的话，说明row.Count == 1, 而row.Count是跟前面的columns的数量是一致的
                        sb.AppendEmphasis(row[0].ColumnName, database);
                        sb.Append(" IN (");

                    }
                    sb.WithPrefix(row[0].ParameterName, database);
                    if (rowIndex < batch.Parameters.Count - 1)
                        sb.Append(',');
                    else
                        sb.Append(')');
                }
            }
            sb.Append(')');

            foreach (var w in Where)
            {
                sb.AppendLine();
                sb.Append("AND ");
                sb.Append(w);
            }
            HandleSqlParameters(sb, database);
            batch.Sql = sb.ToString();
        }
        batchDone = true;
    }
    public override string ToSqlString(ICustomDatabase database)
    {
        if (IsBatchDelete)
        {
            CreateBatchDeleteSql(database);
            // ToSqlString由内部或者测试项目调用，批量情况下查看SQL使用BatchInfos属性
            return string.Empty;
            //return string.Join(",", BatchInfos?.Select(b => b.Sql) ?? []);
        }
        ResolveExpressions(database);
        if (FullDelete)
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
            if (Where.Count == 0 && TargetObject is null)
            {
                throw new LightOrmException("Where Condition is null and not provider a entity value");
            }
            StringBuilder sql;
            sql = new("DELETE FROM ");
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
}
