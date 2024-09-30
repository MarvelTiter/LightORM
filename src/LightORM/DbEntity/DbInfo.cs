namespace LightORM;

[LightTable(Name = "DB_INITIAL_INFO")]
public class DbInfo
{
    [LightColumn(Name = "CREATED_TIME")]
    public DateTime CreatedTime { get; } = DateTime.Now;
    [LightColumn(Name = "INITIALIZED")]
    public bool Initialized { get; internal set; }
    [LightColumn(Name = "APP_NAME")]
    public string? AppName { get; set; }
}
