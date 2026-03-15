using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LightORMTest.Models;

[LightTable(Name = "JSON_TEST")]
public class JsonTestModel
{
    [LightColumn(Name = "ID", PrimaryKey = true, Comment = "自增ID")]
    public int Id { get; set; }
    [LightColumn(Name = "JSON_DATA", Comment = "JSON数据")]
    [LightJsonMap]
    public JsonData? Json { get; set; }

    [LightColumn(Name = "JSON_DATA_LIST", Comment = "JSON数据")]
    [LightJsonMap]
    public List<JsonData>? JsonList { get; set; }

    [LightColumn(Name = "JSON_DATA_ARRAY", Comment = "JSON数据")]
    [LightJsonMap]
    public JsonData[]? JsonArray { get; set; }

    [LightColumn(Name = "JSON_DATA_ELEMENT", Comment = "JSON数据")]
    [LightJsonMap]
    public JsonObject JsonObject { get; set; } = null!;
}

public class JsonData
{
    public string? Name { get; set; }
    public int? Value { get; set; }
    public JsonData? NestJson { get; set; }
    public List<JsonData>? NestList { get; set; }
    public JsonData[]? NestArray { get; set; }
}

public class JsonTestModelDto
{
    public int Id { get; set; }
    public JsonData? Json { get; set; }
}