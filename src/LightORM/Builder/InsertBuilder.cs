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
    protected override void HandleResult(ICustomDatabase database, ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions.SqlType == SqlPartial.Insert)
        {
            Members.AddRange(result.Members!);
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
                 return c.GetValue(TargetObject!) != null;
             });
        return [.. cols];
    }
    public void CreateInsertBatchSql(ICustomDatabase database)
    {
        if (batchDone)
        {
            return;
        }
        ResolveExpressions(database);

        var insertColumns = GetInsertColumns();

        BatchInfos = insertColumns.GenBatchInfos(TargetObjects, 2000 - DbParameters.Count);
        foreach (var item in BatchInfos)
        {
            StringBuilder sb = CreateInsertBuilder();//new(insert);//
            bool firstRow = true;
            foreach (var dic in item.Parameters)
            {
                if (!firstRow)
                {
                    sb.Append(',');
                    sb.AppendLine();
                }
                firstRow = false;
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


    public override string ToSqlString(ICustomDatabase database)
    {
        if (IsBatchInsert)
        {
            CreateInsertBatchSql(database);
            // ToSqlString由内部或者测试项目调用，批量情况下查看SQL使用BatchInfos属性
            return string.Empty;
        }

        if (TargetObject == null)
        {
            throw new LightOrmException("insert null entity");
        }
        ResolveExpressions(database);

        var insertColumns = GetInsertColumns();

        StringBuilder columns = new();
        StringBuilder values = new();
        if (insertColumns.Length == 0)
        {
            throw new LightOrmException("需要插入的列数为0");
        }
        foreach (var item in insertColumns)
        {
            columns.AppendEmphasis(item.ColumnName, database);
            columns.Append(',');
            var val = item.GetValue(TargetObject!);
            if (val is bool b)
            {
                var boolValue = database.HandleBooleanValue(b);
                values.Append(boolValue);
            }
            else
            {
                DbParameters.Add(item.PropertyName, val!);
                values.WithPrefix(item.PropertyName, database);
            }
            values.Append(',');
        }
        columns.RemoveLast(1);
        values.RemoveLast(1);
        StringBuilder sb = new("INSERT INTO");
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