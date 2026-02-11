namespace LightORM.Models;

internal record ExpressionResolvedResult
{
    public ExpressionResolvedResult(ExpressionResolver resolve)
    {
        SqlString = resolve.Sql.ToString();
        Members = resolve.ResolvedMembers;
        MemberOfNavigateMember = resolve.MemberOfNavigateMember;
        UseNavigate = resolve.UseNavigate;
        NavigateDeep = resolve.NavigateDeep;
        NavigateMembers = resolve.NavigateMembers;
        WindowFnPartials = resolve.WindowFnPartials;
        NavigateWhereExpression = resolve.NavigateWhereExpression;
    }
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
    public List<string>? Members { get; }
    /// <summary>
    /// 是否使用了导航属性
    /// </summary>
    public bool UseNavigate { get; }
    public int NavigateDeep { get; set; }
    public bool NeedToExtractValues { get; set; }
    /// <summary>
    /// 解析到的导航属性成员
    /// </summary>
    public List<string>? NavigateMembers { get; }
    public string? MemberOfNavigateMember { get; }
    public Expression? NavigateWhereExpression { get; }
    public List<WindowFnSpecification>? WindowFnPartials { get; }
}
