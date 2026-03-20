using LightORM.Extension;

namespace LightORM.Models;

internal record ExpressionResolvedResult
{
    public ExpressionResolvedResult(ExpressionResolver resolve)
    {
        SqlString = ReplaceConstantValue(resolve);
        Members = [.. resolve.ResolvedMembers];
        MemberOfNavigateMember = resolve.MemberOfNavigateMember;
        UseNavigate = resolve.UseNavigate;
        NavigateDeep = resolve.NavigateDeep;
        NavigateMembers = [.. resolve.NavigateMembers];
        if (resolve.WindowFnPartials is not null)
            WindowFnPartials = [.. resolve.WindowFnPartials];
        NavigateWhereExpression = resolve.NavigateWhereExpression?.FirstOrDefault();
    }

    private static string ReplaceConstantValue(ExpressionResolver resolve)
    {
        var constValues = resolve.DbParameters.Where(x => x.Type == ExpValueType.ConstantNull).ToArray();
        for (int i = 0; i < constValues.Length; i++)
        {
            ResolvedValueInfo item = constValues[i];
            resolve.Sql.ReplaceNull(item.Name);
            resolve.DbParameters.Remove(item);
        }
        return resolve.Sql.ToString();
    }

    /// <summary>
    /// 解析生成的sql语句
    /// </summary>
    public string? SqlString { get; set; }
    /// <summary>
    /// 解析到的参数
    /// </summary>
    public List<ResolvedValueInfo>? ResolvedValues { get; set; }
    /// <summary>
    /// 解析到的成员
    /// </summary>
    public IReadOnlyList<string>? Members { get; }
    /// <summary>
    /// 是否使用了导航属性
    /// </summary>
    public bool UseNavigate { get; }
    public int NavigateDeep { get; set; }
    public bool NeedToExtractValues { get; set; }
    /// <summary>
    /// 解析到的导航属性成员
    /// </summary>
    public IReadOnlyList<string> NavigateMembers { get; }
    public string? MemberOfNavigateMember { get; }
    public Expression? NavigateWhereExpression { get; }
    public IReadOnlyList<WindowFnSpecification>? WindowFnPartials { get; }
}
