using MDbEntity.Attributes;
using System.ComponentModel.DataAnnotations;

namespace InjectTest.Controllers
{
    public enum PowerType
    {
        [Display(Name = "页面")]
        Page,
        [Display(Name = "按钮")]
        Button
    }

    [Table(Name = "powers")]
    public class Power
    {
        [Column(Name = "powerId", PrimaryKey = true)]
        public string PowerId { get; set; }
        [Column(Name = "powerName")]
        public string PowerName { get; set; }
        [Column(Name = "parentId")]
        public string ParentId { get; set; }
        [Column(Name = "powerType")]
        public PowerType PowerType { get; set; }
        [Column(Name = "icon")]
        public string Icon { get; set; }
        [Column(Name = "path")]
        public string Path { get; set; }
        [Column(Name = "sort")]
        public int Sort { get; set; }
    }
}
