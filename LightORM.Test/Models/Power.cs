﻿using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Test.Models
{
    public enum PowerType
    {
        [Display(Name = "页面")]
        Page,
        [Display(Name = "按钮")]
        Button
    }

    [TableName("POWERS")]
    public class Power
    {
        [PrimaryKey]
        [TableHeader("权限ID")]
        [ColumnName("POWER_ID")]
        public string PowerId { get; set; }
        [TableHeader("权限名称")]
        [ColumnName("POWER_NAME")]
        public string PowerName { get; set; }
        [TableHeader("父级权限")]
        [ColumnName("PARENT_ID")]
        public string ParentId { get; set; }
        [TableHeader("权限类型")]
        [ColumnName("POWER_TYPE")]
        public PowerType PowerType { get; set; }
        [TableHeader("图标")]
        [ColumnName("ICON")]
        public string Icon { get; set; }
        [TableHeader("路径")]
        [ColumnName("PATH")]
        public string Path { get; set; }
        [TableHeader("排序")]
        [ColumnName("SORT")]
        public int Sort { get; set; }
    }
}
