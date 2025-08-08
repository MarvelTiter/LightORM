namespace LightORM.Models;

internal class ExpressionResolvedResult
{
    public string? SqlString { get; set; }
    //public Dictionary<string,object>? DbParameters { get; set; }
    public List<DbParameterInfo>? DbParameters { get; set; }
    public List<string>? Members { get; set; }
    //public List<TableInfo>? UsedTables { get; set; }
    public bool UseNavigate { get; set; }
    public int NavigateDeep {  get; set; }
    public List<string>? NavigateMembers { get; set; }
    public Expression? NavigateWhereExpression { get; set; }

}
