namespace LightORM.Models;

internal struct SpecificValue
{
    //public string? Path { get; set; }
    public object? Value { get; set; }
}

/// <summary>
/// <c>UpdateColumns&lt;TUpdate&gt;(Expression&lt;Func&lt;T, TUpdate&gt;&gt;)</c> 的表达式标记。
/// <para>
/// 它只声明"要更新哪些列", 值取自更新实体(TargetObject), 因此 json 列也按普通列处理(col = @p),
/// 由方言的参数绑定负责类型化, 不做整列 JSON 赋值封装(否则多列会被合并进同一条赋值 SQL)。
/// </para>
/// </summary>
internal class UpdateColumnsFlags
{
    private UpdateColumnsFlags()
    {
    }
    public static readonly UpdateColumnsFlags Instance = new();
}
