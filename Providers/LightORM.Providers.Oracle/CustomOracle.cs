using LightORM.Implements;
using LightORM.Interfaces;
using System.Text;

namespace LightORM.Providers.Oracle;

public sealed class CustomOracle(): CustomDatabase(new OracleMethodResolver())
{
    public override string Prefix => ":";
    public override string Emphasis => "\"\"";
    public override void Paging(ISelectSqlBuilder builder, StringBuilder sql)
    {
        sql.Insert(0, $" SELECT ROWNUM as ROWNO, SubMax.* FROM ({Environment.NewLine} ");
        sql.AppendLine(" ) SubMax WHERE ROWNUM <= ");
        sql.Append(builder.PageIndex * builder.PageSize);
        sql.Insert(0, $" SELECT * FROM ({Environment.NewLine} ");
        sql.AppendLine(" ) SubMin WHERE SubMin.ROWNO > ");
        sql.Append((builder.PageIndex - 1) * builder.PageSize);
    }
}
