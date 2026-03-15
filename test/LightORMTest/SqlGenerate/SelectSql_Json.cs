using System;
using System.Collections.Generic;
using System.Text;

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
}
