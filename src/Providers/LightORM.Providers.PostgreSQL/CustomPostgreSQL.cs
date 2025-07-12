using LightORM.Implements;
using LightORM.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.PostgreSQL;

internal class CustomPostgreSQL() : CustomDatabase(new PostgreSQLMethodResolver())
{
    internal readonly static CustomPostgreSQL Instance = new CustomPostgreSQL();

    public override string Prefix => "@";

    public override string Emphasis => "\"\"";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        // PostgreSQL 使用 LIMIT 和 OFFSET 进行分页
        sql.AppendLine();
        sql.Append(" LIMIT ");
        sql.Append(builder.PageSize);
        sql.Append(" OFFSET ");
        sql.Append((builder.PageIndex - 1) * builder.PageSize);
    }

    public override string HandleBooleanValue(bool value)
    {
        return value ? "TRUE" : "FALSE";
    }

}
