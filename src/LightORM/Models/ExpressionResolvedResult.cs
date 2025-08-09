namespace LightORM.Models;

internal class ExpressionResolvedResult
{
    /// <summary>
    /// 解析生成的sql语句
    /// </summary>
    public string? SqlString { get; set; }
    /// <summary>
    /// 解析到的参数
    /// </summary>
    public List<DbParameterInfo>? DbParameters { get; set; }
    /// <summary>
    /// 解析到的成员
    /// </summary>
    public List<string>? Members { get; set; }
    /// <summary>
    /// 是否使用了导航属性
    /// </summary>
    public bool UseNavigate { get; set; }
    public int NavigateDeep {  get; set; }
    /// <summary>
    /// 解析到的导航属性成员
    /// </summary>
    public List<string>? NavigateMembers { get; set; }
    public string? MemberOfNavigateMember { get; set; }
    public Expression? NavigateWhereExpression { get; set; }
    public List<WindowFnSpecification>? WindowFnPartials { get; set; }
}
