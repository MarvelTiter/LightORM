using LightORM.Extension;
using LightORM.Implements;
using LightORM.Interfaces;
using LightORM.Models;
using LightORM.Providers.KingbaseES.Utils;
using System.Reflection;
using System.Text;

namespace LightORM.Providers.KingbaseES;

internal class CustomKingbaseESAdapter(ISqlMethodResolver methodResolver, TableOptions tableOptions) : CustomDatabaseAdapter(methodResolver)
{
    internal readonly static CustomKingbaseESAdapter Instance = new(new KingbaseESMethodResolver(), new());

    public override string Prefix => "@";

    public override string Emphasis => "\"\"";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        // PostgreSQL 使用 LIMIT 和 OFFSET 进行分页
        sql.AppendLine();
        sql.Append(" LIMIT ");
        sql.Append(builder.Take);
        sql.Append(" OFFSET ");
        sql.Append(builder.Skip);
    }

    public override string HandleBooleanValue(bool value)
    {
        return value ? "TRUE" : "FALSE";
    }

    public override void HandleJsonParameter(JsonColumnParameterContext context)
    {
        if (context.ActionType == ActionType.Parameterized && context.Sql is not null)
        {
            context.Sql.Append("::JSON");
        }
        else if (context.ActionType == ActionType.ParameterValue && context.Parameters is not null && context.Column is not null)
        {
            if (!context.Parameters.TryGetValue(context.Column.PropertyName, out var value))
            {
                value = context.Value;
            }
            if (value is null) return;
            if (context.JsonHelper is not null)
            {
                var json = context.JsonHelper.Serialize(value);
                context.Parameters[context.Column.PropertyName] = json;
            }
            else
            {
                context.Parameters[context.Column.PropertyName] = $"\"{value}\"";
            }
        }
    }

    public override void HandleJsonColumn(JsonColumnContext context)
    {
        if (context.Options.SqlType == SqlPartial.Update)
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
