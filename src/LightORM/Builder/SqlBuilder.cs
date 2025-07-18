﻿using LightORM.Builder;
using LightORM.Extension;
using LightORM.Implements;
using System;
using System.Linq;
using System.Text;

namespace LightORM.Builder;

internal abstract record SqlBuilder : ISqlBuilder
{
    protected SqlBuilder(DbBaseType type)
    {
        DbType = type;
        dbHelperLazy = new Lazy<ICustomDatabase>(() => DbType.GetDbCustom());
    }
    public static string N { get; } = Environment.NewLine;
    public DbBaseType DbType { get; set; }

    public IExpressionInfo Expressions { get; } = new ExpressionInfoProvider();
    public TableInfo MainTable => SelectedTables[0];
    public List<TableInfo> SelectedTables { get; set; } = [];
    public int SelectedTableCount => SelectedTables.Count;
    public Dictionary<string, object> DbParameters { get; } = [];
    public List<string> Where { get; set; } = [];
    public object? TargetObject { get; set; }
    public List<string> Members { get; set; } = [];
    private readonly Lazy<ICustomDatabase> dbHelperLazy;
    public ICustomDatabase DbHelper => dbHelperLazy.Value;
    public string AttachPrefix(string content) => DbHelper.AttachPrefix(content);
    public string AttachEmphasis(string content) => DbHelper.AttachEmphasis(content);
    public int DbParameterStartIndex { get; set; }

    public void TryAddParameters(string sql, object? value)
    {
        if (value is null) return;
        var dic = DbParameterReader.ReadToDictionary(sql, value);
        DbParameters.TryAddDictionary(dic);
    }
    protected virtual void BeforeResolveExpressions(ResolveContext context)
    {

    }
    protected ResolveContext? ResolveCtx { get; set; }
    protected void ResolveExpressions()
    {
        if (Expressions.Completed)
        {
            return;
        }
        ResolveCtx = new ResolveContext(DbHelper);
        BeforeResolveExpressions(ResolveCtx);
        foreach (var item in Expressions.ExpressionInfos.Values.Where(item => !item.Completed))
        {
            //item.ResolveOptions!.DbType = DbType;
            item.ResolveOptions!.ParameterIndex = DbParameterStartIndex;
            var result = item.Expression.Resolve(item.ResolveOptions!, ResolveCtx);
            DbParameterStartIndex = item.ResolveOptions!.ParameterIndex;
            item.Completed = true;
            if (!string.IsNullOrEmpty(item.Template))
            {
                result.SqlString = string.Format(item.Template, result.SqlString);
            }

            HandleResult(item, result);

            DbParameters.TryAddDictionary(result.DbParameters);
        }
    }

    public string GetTableName(TableInfo ti, bool useAlias = true, bool useEmphasis = true)
    {
        return $"{NpTableName(ti.TableEntityInfo)}{((useAlias && !string.IsNullOrEmpty(ti.Alias)) ? $" {AttachEmphasis(ti.Alias)}" : "")}";
    }

    //TODO Oracle?
    protected string NpTableName(ITableEntityInfo table)
    {
        if (table.IsTempTable)
        {
            return table.TableName;
        }
        var tablename = table.TableName;
        if (!tablename.Contains('.'))
        {
            return AttachEmphasis(tablename);
        }
        else
        {
            var prs = tablename.Split('.');
            return string.Join(".", prs.Select(AttachEmphasis));
        }
    }

    protected abstract void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result);

    public abstract string ToSqlString();

    protected static object VersionPlus(object? oldVersion)
    {
        return oldVersion switch
        {
            int i => i + 1,
            long l => l + 1,
            double d => d + 1,
            float f => f + 1,
            DateTime => DateTime.UtcNow,
            DateTimeOffset => DateTimeOffset.UtcNow,
            Guid => Guid.NewGuid(),
            string s when Guid.TryParse(s, out _) => Guid.NewGuid().ToString(),
            _ => throw new NotSupportedException("不支持的Version列类型"),
        };
    }
}