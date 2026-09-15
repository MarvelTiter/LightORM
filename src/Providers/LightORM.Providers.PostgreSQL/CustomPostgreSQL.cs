using LightORM.Builder;
using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using LightORM.Providers.PostgreSQL.Utils;
using LightORM.Utils;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Reflection;
using System.Text;

namespace LightORM.Providers.PostgreSQL;

#pragma warning disable CS9113 // 参数未读。
internal sealed partial class CustomPostgreSQL(ISqlMethodResolver methodResolver, PostgreSQLTableOptions tableOptions) : CustomDatabaseAdapter(methodResolver)
{
    internal readonly static CustomPostgreSQL Instance = new(new PostgreSQLMethodResolver(), new());

    public override string Prefix => "@";

    public override string Emphasis => "\"\"";
    public override void Paging(SelectBuilder builder, StringBuilder sql)
    {
        // PostgreSQL 使用 LIMIT 和 OFFSET 进行分页
        sql.AppendLine();
        sql.Append(" LIMIT ");
        sql.Append(builder.Take);
        sql.Append(" OFFSET ");
        sql.Append(builder.Skip);
    }

    public override string FormatBooleanValue(bool value)
    {
        return value ? "TRUE" : "FALSE";
    }

    public override void HandleDateValue(StringBuilder sql, DateTime dateTime)
    {
        sql.Append("TO_TIMESTAMP('");
        sql.Append(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        sql.Append("', 'YYYY-MM-DD HH24:MI:SS')");
    }

    /// <summary>
    /// 方言参数绑定: 参数携带列元数据时按列语义做类型化。
    /// json 列: 参数值保持原始 CLR 值(框架不再预先序列化), 在此序列化为 JSON 文本并声明为 jsonb,
    /// 使 INSERT/UPDATE 无需依赖驱动对 TEXT 的推断(Npgsql 直接按 jsonb 解析, 不会双重编码)。
    /// 未接管(返回 false)时由框架按 CLR 类型默认推断。
    /// </summary>
    public override bool BindParameter(IDataParameter parameter, ITableColumnInfo? column, object? value)
    {
        if (column?.IsJsonColumn == true && parameter is NpgsqlParameter np)
        {
            np.NpgsqlDbType = NpgsqlDbType.Jsonb;
            np.Value = value is null ? null : JsonParameterHelper.Serialize(value);
            return true;
        }

        // CLR DateTime 在 LightORM 中默认映射为 PostgreSQL 的 timestamp without time zone(见 Utils.FormatType.TransformType)。
        // 若交由框架按 DbType.DateTime 兜底绑定, Npgsql 会把参数推断为 timestamp with time zone,
        // 从而拒绝 Kind=Local 的 DateTime(仅支持 UTC)。此处按 Kind 显式类型化(Kind 感知):
        //   · Utc 值 → timestamptz(带时区): Npgsql 原生接受 UTC, 保留服务端时区语义(历史行为);
        //   · Local/Unspecified 值 → 无时区 timestamp: 按墙上时间字面写入, 与自建 no-tz 列一致。
        // 避免"一律无时区"令 Kind=Utc 触发反向抛错(Cannot write DateTime with Kind=UTC to timestamp without time zone)。
        if (value is DateTime dateTime && parameter is NpgsqlParameter npTime)
        {
            npTime.NpgsqlDbType = dateTime.Kind == DateTimeKind.Utc ? NpgsqlDbType.TimestampTz : NpgsqlDbType.Timestamp;
            npTime.Value = dateTime;
            return true;
        }

        return false;
    }

    public override void HandleJsonColumn(JsonColumnContext context)
    {
        if (context.Options.SqlType == SqlPartial.Update)
        {
            if (context.HasIndexInfo())
            {
                context.Sql.AppendEmphasis(context.Column.ColumnName, this);
                context.Sql.Append(" = ");
                context.Sql.Append("JSONB_SET");
                context.Sql.Append('(');
                if (context.Options.RequiredTableAlias)
                {
                    context.Sql.Append(context.Table.Alias);
                    context.Sql.Append('.');
                }
                context.Sql.AppendEmphasis(context.Column.ColumnName, this);
                context.Sql.Append("::JSONB");
                context.Sql.Append(",'{");
                while (context.Members.Count > 0)
                {
                    var current = context.Members.Pop();
                    if (current.Member is not null)
                    {
                        context.Sql.Append(current.Member.Name);
                    }
                    if (current.IndexValue.HasValue)
                    {
                        current.IndexValue.Format(i =>
                        {
                            if (i.IsIntValue)
                            {
                                context.Sql.Append(i.IntValue);
                            }
                            else if (i.IsStringValue)
                            {
                                context.Sql.Append(i.StringValue);
                            }
                            context.Sql.Append(',');
                        });
                        context.Sql.RemoveLast(1);
                    }
                    if (context.Members.Count > 0)
                    {
                        context.Sql.Append(',');
                    }
                }
                context.Sql.Append("}',");
                context.Sql.Append(Prefix);
                context.Sql.Append(context.Column.PropertyName);
                context.Sql.Append("::JSONB");
                context.Sql.Append(')');
            }
            else
            {
                // 整列更新: "col" = @Prop::JSONB (参数按列元数据绑定为 jsonb)
                context.Sql.AppendEmphasis(context.Column.ColumnName, this);
                context.Sql.Append(" = ");
                context.Sql.Append(Prefix);
                context.Sql.Append(context.Column.PropertyName);
                context.Sql.Append("::JSONB");
            }
        }
        else
        {
            context.Sql.Append('(');
            if (context.Options.RequiredTableAlias)
            {
                context.Sql.Append(context.Table.Alias);
                context.Sql.Append('.');
            }
            context.Sql.AppendEmphasis(context.Column.ColumnName, this);
            MemberPathInfo current = default;
            bool lastMemberHasIndex = false;
            while (context.Members.Count > 0)
            {
                current = context.Members.Pop();
                if (current.Member is not null)
                {
                    if (current.IndexValue.Count > 0 || context.Members.Count > 0)
                    {
                        context.Sql.Append("->");
                    }
                    else
                    {
                        context.Sql.Append("->>");
                    }
                    context.Sql.Append('\'');
                    context.Sql.Append(current.Member.Name);
                    context.Sql.Append('\'');
                }
                lastMemberHasIndex = current.IndexValue.HasValue;
                if (current.IndexValue.HasValue)
                {
                    current.IndexValue.Format(i =>
                    {
                        if (i.IsIntValue)
                        {
                            if (current.IndexValue.Count > 0 || context.Members.Count > 0)
                            {
                                context.Sql.Append("->");
                            }
                            else
                            {
                                context.Sql.Append("->>");
                            }
                            context.Sql.Append(i.IntValue);
                        }
                        else if (i.IsStringValue)
                        {
                            if (current.IndexValue.Count > 0 || context.Members.Count > 0)
                            {
                                context.Sql.Append("->");
                            }
                            else
                            {
                                context.Sql.Append("->>");
                            }
                            context.Sql.Append('\'');
                            context.Sql.Append(i.StringValue);
                            context.Sql.Append('\'');
                        }
                    });
                }
            }
            context.Sql.Append(')');
            if (TryGetTypeFromBinary(out var transformType))
            {
                context.Sql.Append("::");
                context.Sql.Append(transformType);
            }
            else if (lastMemberHasIndex)
            {
                context.Sql.Append("::TEXT");
            }
            else if (current.Member is not null)
            {
                var memberType = current.Member.MemberType switch
                {
                    MemberTypes.Property => ((PropertyInfo)current.Member).PropertyType,
                    MemberTypes.Field => ((FieldInfo)current.Member).FieldType,
                    _ => throw new LightOrmException($"获取{current.Member.Name}类型错误")
                };
                transformType = memberType.TransformType();
                context.Sql.Append("::");
                context.Sql.Append(transformType);
            }
        }

        bool TryGetTypeFromBinary(out string? type)
        {
            if (context.Resolver.CurrentBinary is null)
            {
                type = null;
                return false;
            }
            var valueType = LightORM.Utils.ResolveHelper.ExtracExpressionValueType(context.Resolver.CurrentBinary.Right)
                ?? LightORM.Utils.ResolveHelper.ExtracExpressionValueType(context.Resolver.CurrentBinary.Left);
            if (valueType is null)
            {
                type = null;
                return false;
            }
            type = valueType.TransformType();
            return true;
        }
    }
}
