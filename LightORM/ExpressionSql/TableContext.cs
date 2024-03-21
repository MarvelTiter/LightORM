namespace LightORM.ExpressionSql;

internal static class TableLinkTypeEx
{
    internal static string ToLabel(this TableLinkType self)
    {
        switch (self)
        {
            case TableLinkType.LeftJoin:
                return "LEFT JOIN";
            case TableLinkType.InnerJoin:
                return "INNER JOIN";
            case TableLinkType.RightJoin:
                return "RIGHT JOIN";
            default:
                throw new ArgumentException($"未知的TableLinkType {self}");
        }
    }
}
internal enum TableLinkType
{
    None,
    LeftJoin,
    InnerJoin,
    RightJoin,
}
