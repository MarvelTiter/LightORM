using System.Text.Json.Nodes;

namespace LightORMTest.SqlGenerate;

public class UpdateSql_Json : TestBase
{
    [TestMethod]
    public void UpdateJsonField()
    {
        var sql = Db.Update<JsonTestModel>()
            .Set(j => j.Json!.NestJson!.Name, "test")
            .Where(j => j.Id == 5)
            .ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void UpdateJsonFieldBySqlFn()
    {
        var sql = Db.Update<JsonTestModel>()
            .Set(j => SqlFn.JsonSet(j.Json!, "$.NestJson.Name", "test"))
            .Where(j => j.Id == 5)
            .ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    /// <summary>
    /// UpdateColumns 指定单个 json 列(值取自更新实体)：应生成整列赋值(col = @p), 且参数绑定到该列。
    /// </summary>
    [TestMethod]
    public void UpdateColumnsJsonField()
    {
        var entity = new JsonTestModel()
        {
            Id = 1,
            Json = new JsonData() { Name = "ColsName", Value = 1 }
        };
        var sql = Db.Update(entity)
            .UpdateColumns(j => j.Json)
            .ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    /// <summary>
    /// UpdateColumns 指定多个 json 列：每列各自生成一段赋值, 不应出现重复赋值或合并到同一列。
    /// </summary>
    [TestMethod]
    public void UpdateColumnsMultiJsonField()
    {
        var entity = new JsonTestModel()
        {
            Id = 1,
            Json = new JsonData() { Name = "ColsName", Value = 1 },
            JsonObject = new JsonObject() { ["Name"] = "ColsNode" }
        };
        var sql = Db.Update(entity)
            .UpdateColumns(j => new { j.Json, j.JsonObject })
            .ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    /// <summary>
    /// Set 整列 json(无索引、无成员路径)：必须生成整列直接赋值(col = @p), 不得走路径函数。
    /// 依据: 路径函数无法安全承担整列替换 —— MySQL 的 JSON_SET / Oracle 的 JSON_TRANSFORM
    /// 在首参为 NULL 时整体返回 NULL(值静默丢失), 且对文本参数按字符串字面量处理(双重编码)。
    /// </summary>
    [TestMethod]
    public void UpdateJsonWholeColumn()
    {
        var sql = Db.Update<JsonTestModel>()
            .Set(j => j.Json, new JsonData() { Name = "Whole", Value = 9 })
            .Where(j => j.Id == 5)
            .ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    /// <summary>
    /// 批量 Update json 列(整列赋值)：应生成 col = CASE WHEN 主键 = @p THEN @val ... END,
    /// 每行 json 参数独立并按各自实体取值, 不应排除 json 列。用于核对方言生成的批量更新 SQL 与参数。
    /// </summary>
    [TestMethod]
    public void UpdateJsonModel_Batch()
    {
        var e1 = new JsonTestModel()
        {
            Id = 1,
            Json = new JsonData() { Name = "Batch1", Value = 11 },
            JsonObject = new JsonObject { ["Name"] = "Node1", ["Age"] = 1 }
        };
        var e2 = new JsonTestModel()
        {
            Id = 2,
            Json = new JsonData() { Name = "Batch2", Value = 22 },
            JsonObject = new JsonObject { ["Name"] = "Node2", ["Age"] = 2 }
        };
        var sql = Db.Update([e1, e2]).ToSqlWithParameters();
        Console.WriteLine(sql);
    }
}
