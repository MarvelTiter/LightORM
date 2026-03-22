namespace LightORM.Models;

//internal record ExpressionInfo
//{
//    public ExpressionInfo() { }
//    /// <summary>
//    /// ID
//    /// </summary>
//    public string Id { get; set; } = Guid.NewGuid().ToString();
//    public bool Completed { get; set; }
//    /// <summary>
//    /// 解析Sql选项
//    /// </summary>
//    public SqlResolveOptions ResolveOptions { get; set; }
//    /// <summary>
//    /// 表达式
//    /// </summary>
//    public Expression? Expression { get; set; }
//    /// <summary>
//    /// 参数索引
//    /// </summary>
//    public int DbParameterIndex { get; set; }
//    public string? Template { get; set; }
//    public object? AdditionalParameter { get; set; }
//}


internal record ExpressionInfo
{
    public bool IsCompleted { get; set; }
    public ExpressionInfo(SqlResolveOptions option, Expression? expression, string? template = null, object? additionalParameter = null)
    {
        Id = Guid.NewGuid().ToString();
        ResolveOptions = option;
        Expression = expression;
        Template = template;
        AdditionalParameter = additionalParameter;
    }
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 解析Sql选项
    /// </summary>
    public SqlResolveOptions ResolveOptions { get; set; }
    /// <summary>
    /// 表达式
    /// </summary>
    public Expression? Expression { get; }
    public string? Template { get; }
    public object? AdditionalParameter { get; set; }
}
