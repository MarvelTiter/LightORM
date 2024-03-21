#nullable disable
namespace TestProject1.Models;


[LightTable(Name = "RUN_LOG")]
public class RunLog
{
    [LightColumn(Name = "LOG_ID", PrimaryKey = true)]
    public int? LogId { get; set; }

    [LightColumn(Name = "USER_ID")]
    public string UserId { get; set; }

    [LightColumn(Name = "ACTION_MODULE")]
    public string ActionModule { get; set; }

    [LightColumn(Name = "ACTION_NAME")]
    public string ActionName { get; set; }

    [LightColumn(Name = "ACTION_TIME")]
    public DateTime ActionTime { get; init; } = DateTime.Now;

    [LightColumn(Name = "ACTION_RESULT")]
    public string ActionResult { get; set; }

    [LightColumn(Name = "ACTION_MESSAGE")]
    public string ActionMessage { get; set; }
}
