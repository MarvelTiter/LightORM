using System.Text;

namespace LightORM.Providers.Dameng;

internal partial class CustomDamengAdapter
{
    public override void HandleDateValue(StringBuilder sql, DateTime dateTime)
    {
        // Dameng 使用 TO_DATE 函数来处理日期值
        sql.Append("TO_DATE('");
        sql.Append(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        sql.Append("', 'YYYY-MM-DD HH24:MI:SS')");
    }

    //public override string FormatDateTimeValue(DateTime value)
    //{
    //    throw new NotImplementedException();
    //}
}
