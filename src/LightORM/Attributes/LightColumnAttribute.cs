namespace LightORM;

[AttributeUsage(AttributeTargets.Property)]
public class LightColumnAttribute : Attribute
{
    /// <summary>
    /// 列名
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 是否是主键
    /// </summary>
    public bool PrimaryKey { get; set; }
    /// <summary>
    /// 是否自增
    /// </summary>
    public bool AutoIncrement { get; set; }
    /// <summary>
    /// 是否不为空
    /// </summary>
    public bool NotNull { get; set; }
    /// <summary>
    /// 数据长度
    /// </summary>
    public int Length { get; set; }
    /// <summary>
    /// 默认值
    /// </summary>
    public object? Default { get; set; }
    /// <summary>
    /// 注释
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 是否作为版本列
    /// </summary>
    public bool Version { get; set; }
    /// <summary>
    /// 忽略此列
    /// </summary>
    public bool Ignore { get; set; }
    /// <summary>
    /// 忽略更新
    /// </summary>
    public bool IgnoreUpdate { get; set; }
    /// <summary>
    /// 忽略插入
    /// </summary>
    public bool IgnoreInsert { get; set; }
}
