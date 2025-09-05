using LightORM.Implements;
using LightORM.Interfaces;
using System.Text;

namespace LightORM.Providers.Dameng;

public sealed class CustomDameng() : CustomDatabase(new DamengMethodResolver())
{
    internal static readonly CustomDameng Instance = new CustomDameng();
    public override string Prefix => ":";
    public override string Emphasis => "\"\"";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.Insert(0, $"SELECT ROWNUM as ROWNO, SubMax.* FROM (\n");
        sql.AppendLine($") SubMax WHERE ROWNUM <= {builder.Skip + builder.Take}");
        sql.Insert(0, "SELECT * FROM (\n");
        sql.AppendLine($") SubMin WHERE SubMin.ROWNO > {builder.Skip}");
    }
    public override string ReturnIdentitySql() => "SELECT @@IDENTITY";
}
