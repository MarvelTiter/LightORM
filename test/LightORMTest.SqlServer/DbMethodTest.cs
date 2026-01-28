using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace LightORMTest.SqlServer;

[TestClass]
public class DbMethodTest : LightORMTest.DbMethodTest
{
    public override DbBaseType DbType => DbBaseType.SqlServer;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlServer(LightORM.Providers.SqlServer.SqlServerVersion.V1, ConnectString.Value);
        option.UseInterceptor<LightOrmAop>();
    }

    [TestMethod]
    public void CCC()
    {
        var sql = Db.Select<VmasRatingData>()
             .InnerJoin<Inspectotrs>((v, i) => v.InspectorID == i.InspectorID)
             .Where(i => i.Season == 3)
             .GroupBy(g => new { g.Tb2.StationID, g.Tb2.StationName })
             .OrderByDesc(g => g.Average(g.Tables.Tb1.Scores))
             //.AsTable(g => new {Name = g.Group.StationName , Scores = g.Average(g.Tables.Tb1.Scores)})
             .ToSql(g => new RRR()
             {
                 Name = g.Group.StationName,
                 Scores = g.Average(g.Tables.Tb1.Scores)
             });
        Console.WriteLine(sql);
    }
}
public class RRR
{
    public string? Name { get; set; }
    public double Scores { get; set; }
}
[LightTable(Name = "VmasRatingData")]
public class VmasRatingData
{

    /// <summary>
    /// 
    /// </summary>
    [LightColumn(Name = "DetectID")]
    public int DetectID { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [LightColumn(Name = "InspectorID")]
    public int InspectorID { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [LightColumn(Name = "BeginTime")]
    public DateTime BeginTime { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [LightColumn(Name = "EndTime")]
    public DateTime EndTime { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [LightColumn(Name = "Status")]
    public int Status { get; set; }

    [LightColumn(Name = "Scores")]
    public double Scores { get; set; }

    /// <summary>
    /// 比赛编号
    /// </summary>
    [LightColumn(Name = "Season")]
    public int Season { get; set; }

    [LightColumn(Name = "StationId")]
    public string? StationId { get; set; }

    [LightColumn(Name = "StationName")]
    public string? StationName { get; set; }

    [LightColumn(Name = "District")]
    public string? District { get; set; }
}

[LightTable(Name = "Inspectotrs")]
public class Inspectotrs
{

    /// <summary>
    /// 
    /// </summary>
    [LightColumn(Name = "InspectorID", PrimaryKey = true, AutoIncrement = true)]
    public int InspectorID { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [LightColumn(Name = "InspectorName")]
    public string InspectorName { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [LightColumn(Name = "StationID")]
    public string StationID { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [LightColumn(Name = "StationName")]
    public string StationName { get; set; }


    /// <summary>
    /// 0 - 未开始(初始状态)
    /// 1 - 待开始(分配了检测线)
    /// 2 - 已开始(上线检测中)
    /// 3 - 已完成(完成所有检测)
    /// 4 - 归档
    /// </summary>
    [LightColumn(Name = "Status")]
    public int Status { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [LightColumn(Name = "Line")]
    public int? Line { get; set; }

    [LightColumn(Name = "District")]
    public string District { get; set; }

}