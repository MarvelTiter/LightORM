namespace LightORM.DbEntity;

[Table(Name = "DB_INITIAL_INFO")]
public class DbInfo
{
    [Column(Name = "CREATED_TIME")]
    public DateTime CreatedTime { get; } = DateTime.Now;
    [Column(Name = "INITIALIZED")]
    public bool Initialized { get; internal set; }
    [Column(Name = "APP_NAME")]
    public string? AppName { get; set; }
}
