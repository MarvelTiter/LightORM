using LightORM.Implements;
using LightORM.Interfaces;
using System.Text;

namespace LightORM.Providers.MySql;

public sealed class MySqlProvider : DatabaseProvider
{
    public MySqlProvider():base(new MySqlMethodResolver())
    {
        
    }
    public override string Prefix => "?";
    public override string Emphasis => "``";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.AppendLine($"LIMIT {(builder.PageIndex - 1) * builder.PageSize}, {builder.PageSize}");
    }
    public override string ReturnIdentitySql() => "SELECT @@IDENTITY";
}
