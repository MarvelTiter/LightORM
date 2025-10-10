namespace LightORM.Models;

internal static class TableLinkTypeEx
{
    internal static string ToLabel(this TableLinkType self)
    {
        return self switch
        {
            TableLinkType.LeftJoin => "LEFT JOIN",
            TableLinkType.InnerJoin => "INNER JOIN",
            TableLinkType.RightJoin => "RIGHT JOIN",
            TableLinkType.OuterJoin => "FULL OUTER JOIN",
            _ => throw new ArgumentException($"未知的TableLinkType {self}"),
        };
    }
}
public enum TableLinkType
{
    None,
    LeftJoin,
    InnerJoin,
    RightJoin,
    OuterJoin,
}
