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

public interface IPower
{
    string PowerId { get; set; }
    string PowerName { get; set; }
    string ParentId { get; set; }
    PowerType PowerType { get; set; }
    int PowerLevel { get; set; }
    string Icon { get; set; }
    string Path { get; set; }
    int Sort { get; set; }
    bool GenerateCRUDButton { get; set; }
    IEnumerable<IPower> Children { get; set; }
}

[LightTable(Name = "POWERS")]
public class Power : IPower
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

    [LightNavigate(typeof(RolePower), nameof(PowerId), nameof(RolePower.PowerId))]
    public IEnumerable<Role> Roles { get; set; }

    //[LightNavigate(typeof())]
    //public ICollection<User> Users { get; set; }

    [LightNavigate(nameof(PowerId), nameof(ParentId))]
    public IEnumerable<Power> Children { get; set; }


    [NotMapped]
    public bool GenerateCRUDButton { get; set; }
    IEnumerable<IPower> IPower.Children { get => Children; set => Children = value.Cast<Power>().ToList(); }
}
