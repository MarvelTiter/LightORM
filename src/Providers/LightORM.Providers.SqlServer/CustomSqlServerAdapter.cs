using LightORM.Builder;
using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using LightORM.Utils;
using System.Data;
using System.Reflection;
using System.Text;

namespace LightORM.Providers.SqlServer;

#pragma warning disable CS9113 // 参数未读。
internal sealed partial class CustomSqlServerAdapter(SqlServerVersion version, ISqlMethodResolver methodResolver, SqlServerTableOptions tableOptions) : CustomDatabaseAdapter(methodResolver), IReturnIdentity
{
    public SqlServerVersion Version { get; } = version;
    public override string Prefix => "@";
    public override string Emphasis => "[]";

    public override void Paging(SelectBuilder builder, StringBuilder sql)
    {
        if (Version == SqlServerVersion.Over2012)
        {
            sql.AppendLine($"OFFSET {builder.Skip} ROWS");
            sql.AppendLine($"FETCH NEXT {builder.Take} ROWS ONLY");
        }
        else
        {
            var orderByString = "";
            var orderByType = (builder.AdditionalValue == null ? " ASC" : $" {builder.AdditionalValue}");
            if (builder.SelectedMembers.Count == 0)
            {
                if (builder.OrderByMembers.Count > 0)
                {
                    orderByString = string.Join(", ", builder.MainTable.TableEntityInfo.Columns.Where(c => builder.OrderByMembers.Contains(c.PropertyName)).Select(c => $"Sub.{this.AttachEmphasis(c.ColumnName)}"));
                }
                else
                {
                    var col = builder.MainTable.TableEntityInfo.Columns.First(c => c.IsPrimaryKey);
                    orderByString = $"Sub.{col.ColumnName}";
                }
            }
            else
            {
                if (builder.OrderByMembers.Count > 0)
                {
                    var outerOrderColumns = builder.SelectedMembers.Where(s => s.Source is not null && builder.OrderByMembers.Contains(s.Source)).Select(s => s.Column);
                    orderByString = string.Join(", ", outerOrderColumns.Select(c => $"Sub.{this.AttachEmphasis(c)}"));
                }
                else
                {
                    var c = builder.SelectedMembers.First().Column;
                    orderByString = $"Sub.{this.AttachEmphasis(c)}";
                }
            }
            sql.Insert(6, " TOP (100) PERCENT");
            sql.Insert(0, $"SELECT ROW_NUMBER() OVER(ORDER BY {orderByString}{orderByType}) ROWNO, Sub.* FROM (\n");
            sql.AppendLine("  ) Sub");
            // 子查询筛选 ROWNO
            sql.Insert(0, "SELECT * FROM (\n");
            sql.AppendLine(") Paging");
            sql.AppendLine($"WHERE Paging.ROWNO > {builder.Skip}");
            sql.Append($"AND Paging.ROWNO <= {builder.Skip + builder.Take}");
        }
    }
    public override string HandleBooleanValueForBulkCopy(bool value)
    {
        return value ? "true" : "false";
    }
    public void ReturnIdentitySql(StringBuilder sql) => sql.Append("SELECT SCOPE_IDENTITY()");

    public override void HandleDateValue(StringBuilder sql, DateTime dateTime)
    {
        //CONVERT(DATETIME, 'yyyy-MM-dd HH:mm:ss', 120)
        sql.Append("CONVERT(DATETIME, '");
        sql.Append(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        sql.Append("', 120)");
    }

    /// <summary>
    /// SqlServer json 列参数绑定。
    /// <para>
    /// SqlServer 没有"把 JSON 文本解析为 JSON 值"的函数: <c>JSON_MODIFY</c> 的第三参数若为 nvarchar
    /// 会被当作字符串写入(双重编码), 只有 <c>JSON_QUERY(...)</c> 的结果才具备 JSON 语义。因此按值形状区分:
    /// · 复合值(对象/数组) → 序列化为 JSON 文本下发, SQL 侧以 <c>JSON_QUERY(@p)</c> 包装;
    /// · 标量值 → 不接管(返回 false), 由框架按 CLR 类型以<b>原生值</b>下发, JSON_MODIFY 可正确写入。
    /// </para>
    /// </summary>
    public override bool BindParameter(IDataParameter parameter, ITableColumnInfo? column, object? value)
    {
        if (column?.IsJsonColumn == true && JsonParameterHelper.IsCompositeJsonValue(value))
        {
            parameter.DbType = DbType.String;
            parameter.Value = JsonParameterHelper.Serialize(value);
            return true;
        }
        return false;
    }

    public override void HandleJsonColumn(JsonColumnContext context)
    {
        if (context.Options.SqlType == SqlPartial.Update)
        {
            // 无索引、无路径成员的 json 列引用 = 整列替换(如 Set(j => j.Data, obj))。
            // JSON_MODIFY 的路径参数不接受 '$'（报 Unsupported JSON path），须按整列赋值处理，
            // 与 PG(`"DATA" = @Data::JSONB`)、Sqlite(`col = @p`) 的整列分支保持一致。
            if (!context.HasIndexInfo())
            {
                context.Sql.AppendEmphasis(context.Column.ColumnName, this);
                context.Sql.Append(" = ");
                context.Sql.Append(Prefix);
                context.Sql.Append(context.Column.PropertyName);
                return;
            }
            context.Sql.AppendEmphasis(context.Column.ColumnName, this);
            context.Sql.Append(" = ");
            context.Sql.Append("JSON_MODIFY");
        }
        else
        {
            context.Sql.Append("JSON_VALUE");
        }
        context.Sql.Append('(');
        if (context.Options.RequiredTableAlias)
        {
            context.Sql.Append(context.Table.Alias);
            context.Sql.Append('.');
        }
        context.Sql.AppendEmphasis(context.Column.ColumnName, this);
        context.Sql.Append(",'$");
        MemberInfo? leafMember = null;
        while (context.Members.Count > 0)
        {
            var mi = context.Members.Pop();
            if (mi.Member is not null)
            {
                leafMember = mi.Member;
                context.Sql.Append('.');
                context.Sql.Append(mi.Member.Name);
            }
            if (mi.IndexValue.HasValue)
            {
                mi.IndexValue.Format(i =>
                {
                    if (i.IsIntValue)
                    {
                        context.Sql.Append('[');
                        context.Sql.Append(i.IntValue);
                        context.Sql.Append(']');
                    }
                    else if (i.IsStringValue)
                    {
                        context.Sql.Append('.');
                        context.Sql.Append(i.StringValue);
                    }
                });
            }
            //if (context.Members.Count > 0)
            //{
            //    context.Sql.Append('.');
            //}
        }
        context.Sql.Append('\'');
        if (context.Options.SqlType == SqlPartial.Update)
        {
            // 路径更新的第三参数: 值形状由路径末端成员的类型推断(生成期已知, 由表达式结构决定, 与表达式缓存兼容)。
            // 复合值(对象/数组)必须以 JSON_QUERY(@p) 包装, JSON_MODIFY 才会按 JSON 写入;
            // 标量值直接以原生 CLR 值下发(JSON_MODIFY 对标量原生值可正确写入)。
            context.Sql.Append(',');
            if (IsCompositeLeaf(leafMember))
            {
                context.Sql.Append("JSON_QUERY(");
                context.Sql.Append(Prefix);
                context.Sql.Append(context.Column.PropertyName);
                context.Sql.Append(')');
            }
            else
            {
                context.Sql.Append(Prefix);
                context.Sql.Append(context.Column.PropertyName);
            }
        }
        // 结束
        context.Sql.Append(')');
    }

    /// <summary>
    /// 判断 json 路径末端成员的类型是否表示复合 JSON 文档(对象/数组)。
    /// 无法判定(纯索引路径、object 等)按标量处理, 与 binder 的保守策略保持一致。
    /// </summary>
    private static bool IsCompositeLeaf(MemberInfo? member)
    {
        var type = member switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            _ => null
        };
        return type is not null && JsonParameterHelper.IsCompositeJsonValue(type);
    }
}
