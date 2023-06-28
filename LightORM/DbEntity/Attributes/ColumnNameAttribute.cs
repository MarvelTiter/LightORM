﻿using MDbContext.DbStruct;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDbEntity.Attributes
{
    public class ColumnNameAttribute : Attribute
    {
        public string Name { get; set; }
        public ColumnNameAttribute(string name)
        {
            Name = name;
        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 是否是主键
        /// </summary>
        public bool PrimaryKey { get; set; }
        /// <summary>
        /// 是否自增
        /// </summary>
        public bool AutoIncrement { get; set; }
        /// <summary>
        /// 是否不为空
        /// </summary>
        public bool NotNull { get; set; }
        /// <summary>
        /// 数据长度
        /// </summary>
        public int? Length { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public object? Default { get; set; }
        /// <summary>
        /// 注释
        /// </summary>
        public string? Comment { get; set; }
    }
}
