using System.Text.Json.Nodes;

namespace LightORMTest.Models;

/// <summary>
/// ExecutionTest 的 json 列专用表：覆盖 json 对象 / 数组 / List / JsonObject 四类 json 列的写入与读回验证。
/// 语义对齐 <see cref="JsonTestModel"/>: Data/Arr/Lst 存强类型 <see cref="JsonData"/>,
/// Obj 存任意 JsonObject 文档(含 City/Values 等结构, 用于字符串索引/数组索引/JsonSet 等 JsonTest 已覆盖场景)。
/// </summary>
[LightTable(Name = "JSON_EXEC_TEST")]
public class JsonExecTestModel
{
    [LightColumn(Name = "ID", PrimaryKey = true, Comment = "主键ID")]
    public int Id { get; set; }

    [LightColumn(Name = "DATA", Comment = "json对象")]
    [LightJsonMap]
    public JsonData? Data { get; set; }

    [LightColumn(Name = "ARR", Comment = "json数组")]
    [LightJsonMap]
    public JsonData[]? Arr { get; set; }

    [LightColumn(Name = "LST", Comment = "json列表")]
    [LightJsonMap]
    public List<JsonData>? Lst { get; set; }

    [LightColumn(Name = "OBJ", Comment = "JsonObject文档")]
    [LightJsonMap]
    public JsonObject Obj { get; set; } = null!;
}
