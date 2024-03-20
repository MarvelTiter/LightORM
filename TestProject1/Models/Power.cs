using LightORM.DbEntity.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable disable
namespace TestProject1.Models;

public enum PowerType
{
    [Display(Name = "页面")]
    Page,
    [Display(Name = "按钮")]
    Button
}

[LightTable(Name = "POWERS")]
public class Power
{
    [Required]
    [LightColumn(Name = "POWER_ID", PrimaryKey = true)]
    public string PowerId { get; set; }
    [Required]
    [LightColumn(Name = "POWER_NAME")]
    public string PowerName { get; set; }
    [Required]
    [LightColumn(Name = "PARENT_ID")]
    public string ParentId { get; set; }
    [Required]
    [LightColumn(Name = "POWER_TYPE")]
    public PowerType PowerType { get; set; }

    [LightColumn(Name = "POWER_LEVEL")]
    public int PowerLevel { get; set; }

    [LightColumn(Name = "ICON")]
    public string Icon { get; set; }
    [LightColumn(Name = "PATH")]
    public string Path { get; set; }
    [LightColumn(Name = "SORT")]
    public int Sort { get; set; }

}
