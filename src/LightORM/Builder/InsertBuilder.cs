﻿using LightORM.Extension;
using System.Linq;
using System.Text;

namespace LightORM.Builder;

internal record InsertBuilder<T>(DbBaseType type) : SqlBuilder(type)
{
    public new T? TargetObject { get; set; }
    public IEnumerable<T> TargetObjects { get; set; } = Enumerable.Empty<T>();
    public List<BatchSqlInfo>? BatchInfos { get; set; }
    public List<string> IgnoreMembers { get; set; } = [];
    public bool IsReturnIdentity { get; set; }
    protected override void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result)
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
    public void CreateInsertBatchSql()
    {
        if (batchDone)
        {
            return;
        }
        ResolveExpressions();

        if (Members.Count == 0)
        {
            Members.AddRange(MainTable.Columns.Where(c => !c.IsNavigate && !c.IsNotMapped).Select(c => c.PropertyName));
        }
        var insertColumns = MainTable.Columns
            .Where(c => !IgnoreMembers.Contains(c.PropertyName))
            .Where(c => Members.Contains(c.PropertyName) && !c.IsNotMapped && !c.IsNavigate).ToArray();

        BatchInfos = insertColumns.GenBatchInfos(TargetObjects.ToList(), 2000 - DbParameters.Count);
        var insert = $"INSERT INTO {GetTableName(MainTable, false)} \n({string.Join(", ", insertColumns.Select(c => AttachEmphasis(c.ColumnName)))}) \nVALUES \n";
        foreach (var item in BatchInfos)
        {
            StringBuilder sb = new StringBuilder(insert);
            List<string> values = new List<string>();
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
                     return AttachPrefix(c.ParameterName);
                 });
                values.Add($"({string.Join(", ", rowValues)})");
            }
            sb.AppendLine($"{string.Join(",\n", values)}");
            item.Sql = sb.ToString();
        }
        batchDone = true;

    }


    public override string ToSqlString()
    {
        if (IsBatchInsert)
        {
            CreateInsertBatchSql();
            return string.Join(",", BatchInfos?.Select(b => b.Sql) ?? []);
        }

        if (TargetObject == null) LightOrmException.Throw("insert null entity");
        ResolveExpressions();
        StringBuilder sb = new StringBuilder();
        if (Members.Count == 0)
        {
            Members.AddRange(MainTable.Columns.Where(c => !c.IsNavigate && !c.IsNotMapped).Select(c => c.PropertyName));
        }
        var insertColumns = MainTable.Columns
            .Where(c => c.GetValue(TargetObject!) != null)
            .Where(c => !IgnoreMembers.Contains(c.PropertyName))
            .Where(c => Members.Contains(c.PropertyName) && !c.IsNotMapped && !c.IsNavigate).ToArray();
        foreach (var item in insertColumns)
        {
            var val = item.GetValue(TargetObject!);
            DbParameters.Add(item.PropertyName, val!);
        }
        sb.AppendFormat("INSERT INTO {0} \n({1}) \nVALUES \n({2})"
            , GetTableName(MainTable, false)
            , string.Join(", ", insertColumns.Select(c => AttachEmphasis(c.ColumnName)))
            , string.Join(", ", insertColumns.Select(c => AttachPrefix(c.PropertyName))));

        if (IsReturnIdentity)
        {
            sb.Append(';');
            sb.Append(DbHelper.ReturnIdentitySql());
        }

        return sb.ToString();
    }
}