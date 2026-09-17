namespace LightORMTest.SqlGenerate;

public class SelectSql_Json : TestBase
{
    /// <summary>
    /// 测试Json对象嵌套Json对象列查询
    /// </summary>
    [TestMethod]
    public void TestJsonObjectWithNestJsonObjectColumnSelect()
    {
        var sql1 = Db.Select<JsonTestModel>()
             // JSON_EXTRACT(a.`JSON_DATA`,'$.NestJson.Name') = 'test'
             .Where(j => j.Json!.NestJson!.Name == "test")
             .ToSqlWithParameters();
        Console.WriteLine(sql1);

        var sql2 = Db.Select<JsonTestModel>()
             // JSON_EXTRACT(a.`JSON_DATA`,'$.NestJson.Name') = 'test'
             .Where(j => j.Json!.NestJson!.Name == "test")
             .ToSqlWithParameters();
        Console.WriteLine(sql2);
    }

    [TestMethod]
    public void TestJsonObjectWithNestJsonObjectColumnSelect_Contains()
    {
        var sql1 = Db.Select<JsonTestModel>()
             // JSON_EXTRACT(a.`JSON_DATA`,'$.NestJson.Name') = 'test'
             .Where(j => j.Json!.NestJson!.Name!.Contains("test"))
             .ToSqlWithParameters();
        Console.WriteLine(sql1);

        var sql2 = Db.Select<JsonTestModel>()
             // JSON_EXTRACT(a.`JSON_DATA`,'$.NestJson.Name') = 'test'
             .Where(j => j.Json!.NestJson!.Name!.Contains("test"))
             .ToSqlWithParameters();
        Console.WriteLine(sql2);
    }

    [TestMethod]
    public void TestJsonObjectWithNestJsonArrayColumnSelect()
    {
        var index = 3;
        var sql = Db.Select<JsonTestModel>()
             // JSON_EXTRACT(a.`JSON_DATA`,'$.NestArray[index].NestJson.Name') = 'test'
             //.Where(j => SqlFn.JsonQuery(j.Json!, $"$.NestArray[{index}].NestJson.Name", "test"))
             .Where(j => j.Json!.NestList![index].NestJson!.Name == "test")
             .ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void TestJsonArrayColumnSelect()
    {
        var sql = Db.Select<JsonTestModel>()
             // JSON_EXTRACT(a.`JSON_DATA`,'$[0].Name') = 'test'
             .Where(j => j.JsonList![0].Name == "test")
             .ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void TestJsonArrayNestJsonArrayColumnSelect()
    {
        var sql = Db.Select<JsonTestModel>()
             // JSON_EXTRACT(a.`JSON_DATA`,'$[0].NestArray[1].NestJson.NestArray[2].Name') = 'test'
             .Where(j => j.JsonList![0].NestList![1].NestJson!.NestList![2].Name == "test")
             .ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void TestJsonElementIndexer()
    {
        var sql = Db.Select<JsonTestModel>()
             // CAST(JSON_EXTRACT(a.`JSON_DATA_ELEMENT`,'$["prop1"]["prop2"]') AS TEXT) = 'test'
             .Where(j => j.JsonObject["prop1"]!["prop2"]!.ToString() == "test")
             .ToSqlWithParameters();
        Console.WriteLine(sql);
    }

    /// <summary>
    /// 读取路径<b>复合末级成员</b>的取值表达式探针(仅打印 SQL, 不连库)：
    /// 末级为对象(Data.NestJson) / 数组元素(Arr[1]) / List 元素(Lst[0]) 时, 各方言应生成
    /// "按 JSON 解析取文档"的表达式(JSON_QUERY / ->) 而非只返回标量的 JSON_VALUE。
    /// 对照用例末尾的 Obj["City"]["Name"](索引路径, 末级仍是标量) 必须保持标量取值语义。
    /// </summary>
    [TestMethod]
    public void TestJsonCompositeLeafMemberRead()
    {
        var nested = Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 100)
            .ToSql(j => new { j.Id, N = j.Data!.NestJson });
        Console.WriteLine($"复合末级-对象: {nested}");

        var arrayItem = Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 100)
            .ToSql(j => new { j.Id, I = j.Arr![1] });
        Console.WriteLine($"复合末级-数组元素: {arrayItem}");

        var listItem = Db.Select<JsonExecTestModel>()
            .Where(j => j.Id == 100)
            .ToSql(j => new { j.Id, L = j.Lst![0] });
        Console.WriteLine($"复合末级-List元素: {listItem}");

        var scalarInObj = Db.Select<JsonExecTestModel>()
            .Where(j => j.Obj["City"]!["Name"]!.GetValue<string>() == "x")
            .ToSqlWithParameters();
        Console.WriteLine($"对照-索引路径标量: {scalarInObj}");
    }
}
