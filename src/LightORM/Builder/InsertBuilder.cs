using LightORM.Extension;
using System.Text;

namespace LightORM.Builder;

internal record InsertBuilder<T> : SqlBuilder
{
    public new T? TargetObject { get; set; }
    public IEnumerable<T> TargetObjects { get; set; } = [];
    public List<BatchSqlInfo>? BatchInfos { get; set; }
    public List<string> IgnoreMembers { get; set; } = [];
    public bool IsReturnIdentity { get; set; }
    protected override void HandleResult(ICustomDatabase database, ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        if (expInfo.ResolveOptions?.SqlType == SqlPartial.Insert)
        {
            Members.AddRange(result.Members!);
        }
        else if (expInfo.ResolveOptions?.SqlType == SqlPartial.Ignore)
        {
            IgnoreMembers.AddRange(result.Members!);
        }
    }
    public bool IsBatchInsert { get; set; }
    bool batchDone;
    public void CreateInsertBatchSql(ICustomDatabase database)
    {
        if (batchDone)
        {
            return;
        }
        ResolveExpressions(database);

        if (Members.Count == 0)
        {
            Members.AddRange(MainTable.TableEntityInfo.Columns.Where(c => !c.IsNavigate && !c.IsNotMapped && !c.AutoIncrement && !c.IsAggregated).Select(c => c.PropertyName));
        }
        var insertColumns = MainTable.TableEntityInfo.Columns
            .Where(c => !IgnoreMembers.Contains(c.PropertyName))
            .Where(c => Members.Contains(c.PropertyName) && !c.IsNotMapped && !c.IsNavigate).ToArray();

        BatchInfos = insertColumns.GenBatchInfos(TargetObjects.ToList(), 2000 - DbParameters.Count);
        var insert = $"INSERT INTO {GetTableName(database, MainTable, false)} {N}({string.Join(", ", insertColumns.Select(c => database.AttachEmphasis(c.ColumnName)))}) {N}VALUES {N}";
        foreach (var item in BatchInfos)
        {
            StringBuilder sb = new(insert);
            List<string> values = [];
            foreach (var dic in item.Parameters)
            {
                var rowValues = dic.Select(c =>
                 {
                     var val = c.Value;
                     if (val is null)
                     {
                         return "NULL";
                     }
                     //if (c.UnderlyingType.IsNumber() || c.UnderlyingType == typeof(string))
                     //{
                     //    return $"'{val}'";
                     //}
                     return database.AttachPrefix(c.ParameterName);
                 });
                values.Add($"({string.Join(", ", rowValues)})");
            }
            sb.AppendLine($"{string.Join($", {N}", values)}");
            HandleSqlParameters(sb, database);
            item.Sql = sb.ToString();
        }
        batchDone = true;

    }


    public override string ToSqlString(ICustomDatabase database)
    {
        if (IsBatchInsert)
        {
            CreateInsertBatchSql(database);
            return string.Join(",", BatchInfos?.Select(b => b.Sql) ?? []);
        }

        if (TargetObject == null) LightOrmException.Throw("insert null entity");
        ResolveExpressions(database);
        if (Members.Count == 0)
        {
            Members.AddRange(MainTable.TableEntityInfo.Columns.Where(c => !c.IsNavigate && !c.IsNotMapped && !c.IsAggregated && !c.AutoIncrement).Select(c => c.PropertyName));
        }
        var insertColumns = MainTable.TableEntityInfo.Columns
            .Where(c => Members.Contains(c.PropertyName) && !c.IsNotMapped && !c.IsNavigate)
            .Where(c => !IgnoreMembers.Contains(c.PropertyName))
            .Where(c => c.GetValue(TargetObject!) != null)
            .ToArray();
        StringBuilder columns = new();
        StringBuilder values = new();
        if (insertColumns.Length == 0)
        {
            throw new LightOrmException("需要插入的列数为0");
        }
        foreach (var item in insertColumns)
        {
            columns.Append(database.AttachEmphasis(item.ColumnName));
            columns.Append(", ");
            var val = item.GetValue(TargetObject!);
            if (val is bool b)
            {
                var boolValue = database.HandleBooleanValue(b);
                values.Append(boolValue);
            }
            else
            {
                DbParameters.Add(item.PropertyName, val!);
                values.Append(database.AttachPrefix(item.PropertyName));
            }
            values.Append(", ");
        }
        columns.RemoveLast(2);
        values.RemoveLast(2);
        StringBuilder sb = new("INSERT INTO");
        sb.AppendLine($" {GetTableName(database, MainTable, false)} ");
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