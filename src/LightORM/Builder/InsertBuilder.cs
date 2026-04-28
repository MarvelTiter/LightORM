using LightORM.Extension;
using System.Text;

namespace LightORM.Builder;

internal record InsertBuilder<T> : SqlBuilder
{
    public new T? TargetObject { get; set; }
    public T[] TargetObjects { get; set; } = [];
    public List<BatchSqlInfo>? BatchInfos { get; set; }
    HashSet<string> IgnoreMembers { get; set; } = [];
    HashSet<string> Members { get; set; } = [];

    public bool IsReturnIdentity { get; set; }

    public void AddMember(string member, object? value)
    {
        Members.Add(member);
        if (value is not null)
        {
#if NET462
            if (!DbParameters.ContainsKey(member))
            {
                DbParameters.Add(member, value);
            }
#else
            DbParameters.TryAdd(member, value);
#endif
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
                    DbParameters.Add(member, v.Value);
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
        BatchInfos = insertColumns.GenBatchInfos(TargetObjects, 2000 - DbParameters.Count);
        foreach (var item in BatchInfos)
        {
            StringBuilder sb = CreateInsertBuilder();//new(insert);//
            for (int i = 0; i < item.Parameters.Count; i++)
            {
                List<SimpleColumn>? dic = item.Parameters[i];
                if (i > 0)
                {
                    sb.Append(',');
                    sb.AppendLine();
                }
                sb.Append('(');
                foreach (var c in dic)
                {
                    if (c.Value is null)
                    {
                        sb.Append("NULL");
                    }
                    else
                    {
                        sb.WithPrefix(c.ParameterName, database);
                    }
                    sb.Append(',');
                }
                sb.RemoveLast(1);
                sb.Append(')');
            }
            HandleSqlParameters(sb, database);
            item.Sql = sb.ToString();
        }
        batchDone = true;

        StringBuilder CreateInsertBuilder()
        {
            var sb = new StringBuilder("INSERT INTO ");
            //sb.Append(GetTableName(database, MainTable, false));
            sb.AppendTableName(database, MainTable, false).AppendLine();
            sb.Append('(');
            foreach (var item in insertColumns)
            {
                sb.AppendEmphasis(item.ColumnName, database);
                sb.Append(',');
            }
            sb.RemoveLast(1);
            sb.Append(')');
            sb.AppendLine();
            sb.AppendLine("VALUES");
            return sb;
        }
    }

    public override string ToSqlString(IDatabaseAdapter database)
    {
        if (IsBatchInsert)
        {
            CreateInsertBatchSql(database);
            // ToSqlString由内部或者测试项目调用，批量情况下查看SQL使用BatchInfos属性
            return string.Empty;
        }

        if (TargetObject == null && DbParameters.Count == 0)
        {
            throw new LightOrmException("插入的实体为空或者没有需要插入的值");
        }
        ResolveExpressions(database);

        var insertColumns = GetInsertColumns();

        StringBuilder columns = new();
        StringBuilder values = new();
        if (insertColumns.Length == 0)
        {
            throw new LightOrmException("需要插入的列数为0");
        }
        for (int i = 0; i < insertColumns.Length; i++)
        {
            ITableColumnInfo? item = insertColumns[i];
            if (!DbParameters.TryGetValue(item.PropertyName, out object? val))
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
                DbParameters.Add(item.PropertyName, val);
            }
            columns.AppendEmphasis(item.ColumnName, database);
            columns.Append(',');

            if (val is bool b)
            {
                var boolValue = database.HandleBooleanValue(b);
                values.Append(boolValue);
                DbParameters.Remove(item.PropertyName);
            }
            else
            {
                // 处理JSON列的插入
                if (item.IsJsonColumn)
                {
                    var jsonHandler = ExpressionSqlOptions.Instance.Value.GetJsonHandler();
                    var jsonString = jsonHandler.Serialize(val);
                    DbParameters[item.PropertyName] = jsonString;
                    values.WithPrefix(item.PropertyName, database);
                    // TODO 暂时做法，兼容postgresql，在后面追加::JSONB
                    database.HandleJsonParameter(new(ActionType.Parameterized, item, values, DbParameters));
                }
                else
                {
                    values.WithPrefix(item.PropertyName, database);
                }
            }
            values.Append(',');
        }
        columns.RemoveLast(1);
        values.RemoveLast(1);
        StringBuilder sb = new("INSERT INTO ");
        //sb.AppendLine($" {GetTableName(database, MainTable, false)} ");
        sb.AppendTableName(database, MainTable, false).AppendLine();
        sb.Append('(');
        sb.Append(columns);
        sb.AppendLine(")");
        sb.AppendLine("VALUES");
        sb.Append('(');
        sb.Append(values);
        sb.AppendLine(")");

        if (IsReturnIdentity)
        {
            sb.Append(';');
            sb.Append(database.ReturnIdentitySql());
        }
        HandleSqlParameters(sb, database);
        return sb.Trim();
    }
}