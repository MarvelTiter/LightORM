﻿using LightORM.Implements;
using LightORM.Interfaces;
using System.Text;

namespace LightORM.Providers.MySql;

public sealed class CustomMySql() : CustomDatabase(new MySqlMethodResolver())
{
    internal static readonly CustomMySql Instance = new CustomMySql();
    public override string Prefix => "?";
    public override string Emphasis => "``";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.AppendLine($"LIMIT {(builder.PageIndex - 1) * builder.PageSize}, {builder.PageSize}");
    }
    public override string ReturnIdentitySql() => "SELECT @@IDENTITY";
}
