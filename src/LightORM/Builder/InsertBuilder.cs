using LightORM.Extension;

namespace LightORM.Builder;

internal class InsertBuilder<T> : SqlBuilder
{
    //public new T? TargetObject { get; set; }
    public T[] TargetObjects { get; set; } = [];
    public List<BatchSqlInfo>? BatchInfos { get; set; }
    HashSet<string> IgnoreMembers { get; set; } = [];
    HashSet<string> Members { get; set; } = [];
    public bool UpdateOnConflict { get; set; }
    public bool IgnoreOnConflict { get; set; }
    public bool IsReturnIdentity { get; set; }

    public void AddMember(string member, object? value)
    {
        Members.Add(member);
        if (value is not null)
        {
            DbParameters.TryAdd(member, new DbParameterValue(null, value));
        }
    }

    protected override void HandleResult(IDatabaseAdapter database, ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions.SqlType == SqlPartial.Insert)
        {
            if (expInfo.AdditionalParameter is null)
            {
                Members.AddRange(result.Members!);
            }
            else if (expInfo.AdditionalParameter is SpecificValue v)
            {
                if (v.Value is not null)
                {
                    var member = result.Members![0];
                    Members.Add(member);
                    DbParameters.Add(member, new DbParameterValue(null, v.Value));
                }
            }
        }
        else if (expInfo.ResolveOptions.SqlType == SqlPartial.Ignore)
        {
            IgnoreMembers.AddRange(result.Members!);
        }
    }
    public bool IsBatchInsert { get; set; }
    bool batchDone;
    private ITableColumnInfo[] GetInsertColumns()
    {
        if (Members.Count == 0)
        {
            Members.AddRange(MainTable.TableEntityInfo.Columns.Where(c => !c.IsNavigate && !c.IsNotMapped && !c.AutoIncrement && !c.IsAggregated && !c.IsIgnoreInsert).Select(c => c.PropertyName));
        }
        else
        {
            var necessaryColumns = MainTable.TableEntityInfo.Columns.Where(c => (c.IsPrimaryKey && !c.AutoIncrement) || c.IsVersionColumn).Select(c => c.PropertyName);
            Members.AddRange(necessaryColumns);
        }
        var cols = MainTable.TableEntityInfo.Columns
             .Where(c =>
             {
                 if (IgnoreMembers.Count > 0 && IgnoreMembers.Contains(c.PropertyName))
                 {
                     return false;
                 }
                 if (c.IsNotMapped || c.IsNavigate)
                 {
                     return false;
                 }
                 // 如果是默认行为, 前面已经排除了IsIgnoreInsert的列, 如果是指定更新列, 就应该忽略IsIgnoreInsert
                 if (!Members.Contains(c.PropertyName))
                 {
                     return false;
                 }
                 return true;
             });
        return [.. cols];
    }

    public void CreateInsertBatchSql(IDatabaseAdapter database)
    {
        if (batchDone)
        {
            return;
        }
        ResolveExpressions(database);

        var insertColumns = GetInsertColumns();

        // TODO 批量插入对JSON列处理
        BatchInfos = insertColumns.GenBatchInfos(TargetObjects, database, 2000 - DbParameters.Count);

        database.HandleBatchInsert<T>(new(this, insertColumns, BatchInfos, DbParameters, database));

        batchDone = true;

    }

    public override string ToSqlString(IDatabaseAdapter database)
    {
        if (QuoteIdentifiers.HasValue)
        {
            database = new ScopedDatabaseAdapter(database, QuoteIdentifiers.Value);
        }
        if (IsBatchInsert)
        {
            CreateInsertBatchSql(database);
            // ToSqlString由内部或者测试项目调用，批量情况下查看SQL使用BatchInfos属性
            return string.Empty;
        }
        ResolveExpressions(database);

        if (TargetObject == null && DbParameters.Count == 0)
        {
            throw new LightOrmException("插入的实体为空或者没有需要插入的值");
        }

        var insertColumns = GetInsertColumns();

        //StringBuilder columns = new();
        //StringBuilder values = new();
        Dictionary<ITableColumnInfo, MapEntry> columnValueMap = new(TableColumnInfoEqual.Default);
        if (insertColumns.Length == 0)
        {
            throw new LightOrmException("需要插入的列数为0");
        }
        for (int i = 0; i < insertColumns.Length; i++)
        {
            ITableColumnInfo? item = insertColumns[i];
            object? val;
            if (!DbParameters.TryGetValue(item.PropertyName, out var paramEntry))
            {
                if (TargetObject is null)
                {
                    if (item.IsVersionColumn)
                    {
                        val = VersionDefaultValue(item.ColumnType);
                    }
                    else
                    {
                        throw new LightOrmException($"无法获取{item.PropertyName}的值，因为插入实体是null，并且参数字典也未包含该值");
                    }
                }
                else
                {
                    val = item.GetValue(TargetObject);
                    if (val is null)
                        continue;
                }
                DbParameters.Add(item.PropertyName, new DbParameterValue(item, val));
            }
            else
            {
                val = paramEntry.Value;
            }
            //columns.AppendEmphasis(item.ColumnName, database);
            //columns.Append(',');
            if (val is bool b)
            {
                //var boolValue = database.HandleBooleanValue(b);
                //values.Append(boolValue);
                //database.HandleBooleanValue(values, b);
                DbParameters.Remove(item.PropertyName);
                columnValueMap.Add(item, new(database.AttachEmphasis(item.ColumnName), database.FormatBooleanValue(b)));
            }
            else
            {
                // json 列与普通列一致, 以 "列 = @参数" 插入; 参数值保持原始 CLR 值,
                // 由方言的 IDatabaseParameterBinder 在绑定阶段序列化并按驱动类型化(如 PostgreSQL 的 jsonb),
                // 框架不再在此预先序列化为 JSON 文本。
                //values.WithPrefix(item.PropertyName, database);
                columnValueMap.Add(item, new(database.AttachEmphasis(item.ColumnName), database.AttachPrefix(item.PropertyName)));
            }
            //values.Append(',');
        }
        //columns.RemoveLast(1);
        //values.RemoveLast(1);
        using var _ = StringBuilderPool.Get(out var sb);
        WriteTags(sb);
        if ((UpdateOnConflict || IgnoreOnConflict) && insertColumns.Any(i => i.IsPrimaryKey))
        {
            database.HandleInsertOrUpdate(new(this, sb, columnValueMap, DbParameters, IgnoreOnConflict, database));
        }
        else
        {
            sb.Append("INSERT INTO ");
            //sb.AppendLine($" {GetTableName(database, MainTable, false)} ");
            sb.AppendTableName(database, MainTable, false).AppendLine();
            sb.Append('(');
            sb.AppendEntryColumns(columnValueMap.Values);
            sb.AppendLine(")");
            sb.AppendLine("VALUES");
            sb.Append('(');
            sb.AppendEntryValues(columnValueMap.Values);
            sb.AppendLine(")");

            if (IsReturnIdentity)
            {
                sb.Append(';');
                // 自增主键回读属可选能力: 仅 SqlServer/MySql/Sqlite/Dameng 支持;
                // 其余方言(Oracle/PG/KingbaseES)能力探测失败时在此明确报"不支持", 而非 base 空桩。
                if (AdapterCapability.TryGet<IReturnIdentity>(database, out var identity))
                {
                    identity!.ReturnIdentitySql(sb);
                }
                else
                {
                    throw new NotSupportedException(
                        $"数据库适配器 {database.GetType().Name} 不支持 ReturnIdentity(自增主键回读)。支持方: SqlServer / MySql / Sqlite / Dameng。");
                }
            }
        }

        HandleSqlParameters(sb, database);
        return sb.Trim();
    }

}