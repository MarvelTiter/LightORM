using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LightORM.Models
{
    internal record ExpressionInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public bool Completed { get; set; }
        /// <summary>
        /// 解析Sql选项
        /// </summary>
        public SqlResolveOptions? ResolveOptions { get; set; } 

        /// <summary>
        /// 表达式
        /// </summary>
        public Expression? Expression { get; set; }
        /// <summary>
        /// 参数索引
        /// </summary>
        public int DbParameterIndex { get; set; }
        public string? Template { get; set; }
        public object? AdditionalParameter { get; set; }
    }
}
