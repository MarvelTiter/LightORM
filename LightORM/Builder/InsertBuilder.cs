namespace LightORM.Builder;

internal class InsertBuilder : SqlBuilder
{
    protected override void HandleResult(ExpressionInfo expInfo, ExpressionResolvedResult result)
    {
        throw new NotImplementedException();
    }
    public bool IsInsertList { get; set; }
    public override string ToSqlString()
    {
        throw new NotImplementedException();
    }


}