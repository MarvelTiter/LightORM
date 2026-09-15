using System.Text.Json.Nodes;

namespace LightORMTest.SqlGenerate;

public class InsertSql_Json : TestBase
{
    [TestMethod]
    public void InsertJsonModel()
    {
        var jsonString = """
            {
                "Name": "Hello",
                "Age": 18,
                "Role": "Admin",
                "Values": [1, 2, 3],
                "City": {
                    "Name": "Dongguan"
                }
            }
            """;
        var testJson = new JsonData()
        {
            Name = "World",
            Value = 20,
            NestArray = [new() { Name = "Nest Array", Value = 30 }],
            NestList = [new() { Name = "Nest List", Value = 40 }],
            NestJson = new() { Name = "Nest Object", Value = 50 }
        };
        var entity = new JsonTestModel()
        {
            Json = testJson,
            JsonArray = [testJson],
            JsonList = [testJson],
            JsonObject = System.Text.Json.JsonSerializer.Deserialize<JsonObject>(jsonString)!
        };
        var sql = Db.Insert(entity).ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    /// <summary>
    /// 批量 Insert json 列：多行 VALUES 展开, 每行 json 参数独立(Json_0/Json_1...),
    /// json 值应为序列化后的 JSON 文本。用于核对方言生成的批量插入 SQL 与参数。
    /// </summary>
    [TestMethod]
    public void InsertJsonModel_Batch()
    {
        var sql = Db.Insert([CreateEntity("B1", 1), CreateEntity("B2", 2)]).ToSqlWithParameters();
        Console.WriteLine(sql);

        static JsonTestModel CreateEntity(string name, int value)
        {
            var json = new JsonData()
            {
                Name = name,
                Value = value,
                NestJson = new() { Name = $"Nest {name}", Value = value * 10 }
            };
            return new JsonTestModel()
            {
                Json = json,
                JsonArray = [json],
                JsonList = [json],
                JsonObject = new JsonObject { ["Name"] = name, ["Values"] = new JsonArray { value } }
            };
        }
    }
}