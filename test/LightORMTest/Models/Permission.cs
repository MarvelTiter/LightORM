using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable disable
namespace LightORMTest.Models;

public enum PermissionType
{
    [Display(Name = "页面")]
    Page,
    [Display(Name = "按钮")]
    Button
}

public interface IPermission
{
    string PermissionId { get; set; }
    string PermissionName { get; set; }
    string ParentId { get; set; }
    PermissionType PermissionType { get; set; }
    int PermissionLevel { get; set; }
    string Icon { get; set; }
    string Path { get; set; }
    int Sort { get; set; }
    bool GenerateCRUDButton { get; set; }
    IEnumerable<IPermission> Children { get; set; }
}

[LightTable(Name = "PERMISSIONS")]
public class Permission : IPermission
{
    [LightColumn(Name = "ID", PrimaryKey = true, AutoIncrement = true, Comment = "自增ID")]
    public int Id { get; set; }
    [Required]
    [LightColumn(Name = "PERMISSION_ID", PrimaryKey = true, Comment = "权限ID")]
    public string PermissionId { get; set; }
    [Required]
    [LightColumn(Name = "PERMISSION_NAME", Comment = "权限名称")]
    public string PermissionName { get; set; }
    [Required]
    [LightColumn(Name = "PARENT_ID", Comment = "父级权限")]
    public string ParentId { get; set; }
    [Required]
    [LightColumn(Name = "PERMISSION_TYPE", Comment = "权限类型")]
    public PermissionType PermissionType { get; set; }

    [LightColumn(Name = "PERMISSION_LEVEL", Comment = "权限等级")]
    public int PermissionLevel { get; set; }

    [LightColumn(Name = "ICON", Comment = "图标")]
    public string Icon { get; set; }
    [LightColumn(Name = "PATH", Comment = "路径")]
    public string Path { get; set; }
    [LightColumn(Name = "SORT", Comment = "排序")]
    public int Sort { get; set; }

    [LightNavigate(typeof(RolePermission), nameof(PermissionId), nameof(RolePermission.PermissionId))]
    public IEnumerable<Role> Roles { get; set; }

    //[LightNavigate(typeof())]
    //public ICollection<User> Users { get; set; }

    [LightNavigate(nameof(PermissionId), nameof(ParentId))]
    public IEnumerable<Permission> Children { get; set; }


    [NotMapped]
    public bool GenerateCRUDButton { get; set; }
    IEnumerable<IPermission> IPermission.Children { get => Children; set => Children = value.Cast<Permission>().ToList(); }
}
