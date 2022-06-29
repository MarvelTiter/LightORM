using LightORM.Test2.Models;
using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Test2.DemoModel
{
    public enum PowerType
    {
        [Display(Name = "页面")]
        Page,
        [Display(Name = "按钮")]
        Button
    }

    [Table(Name = "POWERS")]
    public class Power
    {
        [PrimaryKey]
        [TableHeader("权限ID")]
        [Column(Name = "POWER_ID")]
        public string PowerId { get; set; }
        [TableHeader("权限名称")]
        [Column(Name = "POWER_NAME")]
        public string PowerName { get; set; }
        [TableHeader("父级权限")]
        [Column(Name = "PARENT_ID")]
        public string ParentId { get; set; }
        [TableHeader("权限类型")]
        [Column(Name = "POWER_TYPE")]
        public PowerType PowerType { get; set; }
        [TableHeader("图标")]
        [Column(Name = "ICON")]
        public string Icon { get; set; }
        [TableHeader("路径")]
        [Column(Name = "PATH")]
        public string Path { get; set; }
        [TableHeader("排序")]
        [Column(Name = "SORT")]
        public int Sort { get; set; }
    }
}
