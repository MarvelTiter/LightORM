using LightORM.Implements;
using LightORM.Interfaces;
using System.Text;

namespace LightORM.Providers.MySql;

public sealed class CustomMySql(ISqlMethodResolver methodResolver, TableOptions tableOptions) : CustomDatabase(methodResolver)
{
    internal static readonly CustomMySql Instance = new CustomMySql(new MySqlMethodResolver(), new());
    public override string Prefix => "?";
    public override string Emphasis => "``";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.AppendLine($"LIMIT {builder.Skip}, {builder.Take}");
    }
    public override string ReturnIdentitySql() => "SELECT @@IDENTITY";
}
