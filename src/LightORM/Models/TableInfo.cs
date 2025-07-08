namespace LightORM.Models;

#pragma warning disable IDE0290 // 使用主构造函数
public class TableInfo
{
    private const string ALIAS = "abcdefghijklmnopqrstuvwyz";
    private int index;
    public static TableInfo Create<T>() => Create<T>(0);
    public static TableInfo Create<T>(int index) => Create(typeof(T), index);
    public static TableInfo Create(Type type) => Create(type, 0);
    public static TableInfo Create(Type type, int index) => new(type, index);

    private TableInfo(Type type, int index)
    {
        Type = type;
        this.index = index;
        Alias = ALIAS[index].ToString();
        TableEntityInfo = TableContext.GetTableInfo(type);
    }
    /// <summary>
    /// 表类型
    /// </summary>
    public Type Type { get; }
    /// <summary>
    /// 表达式中的参数名称
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 解析结果的表别名
    /// </summary>
    public string Alias { get; set; }
    /// <summary>
    /// 表达式中的参数索引
    /// </summary>
    public int Index
    {
        get => index;
        set
        {
            if (value == -1)
            {
                Alias = TableEntityInfo.TableName;
            }
            else
            {
                index = value;
                Alias = ALIAS[value].ToString();
            }
        }
    }
    /// <summary>
    /// 表信息
    /// </summary>
    public ITableEntityInfo TableEntityInfo { get; }
}
