namespace LightORMTest.Models;

[LightTable(Name = "SMS_LOG", DatabaseKey = "Cache")]
public class SmsLog
{
    [LightColumn(Name = "ID", PrimaryKey = true)]
    public int Id { get; set; }
    [LightFlat]
    public SmsReceive Recive { get; set; } = new();

    [LightColumn(Name = "CREATE_TIME")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [LightColumn(Name = "VERSION", Version = true)]
    public int Version { get; set; }
}

public class SmsReceive
{
    [LightColumn(Name = "UUID", PrimaryKey = true)]
    public string? Uuid { get; set; } = Guid.NewGuid().ToString();
    [LightColumn(Name = "CODE")]
    public int Code { get; set; } = 100;
    [LightColumn(Name = "MSG")]
    public string? Msg { get; set; } = "测试";
}