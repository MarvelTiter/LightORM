namespace TestProject1.Models;

[LightTable(Name = "SMS_LOG", DatabaseKey = "Cache")]
public class SmsLog
{
    [LightFlat]
    public SmsReceive Recive { get; set; } = new();

    [LightColumn(Name = "CREATE_TIME")]
    public DateTime CreateTime { get; set; } = DateTime.Now;
}

public class SmsReceive
{
    [LightColumn(Name = "CODE")]
    public int Code { get; set; } = 100;
    [LightColumn(Name = "MSG")]
    public string? Msg { get; set; } = "测试";
    [LightColumn(Name = "UUID")]
    public string? Uuid { get; set; } = Guid.NewGuid().ToString();
}