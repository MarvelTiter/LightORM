using LightORM.ExpressionSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightORM.Models;

internal record JoinInfo
{
    /// <summary>
    /// 联接类型
    /// </summary>
    public TableLinkType JoinType { get; set; }

    /// <summary>
    /// 条件
    /// </summary>
    public string? Where { get; set; }
    public string? ExpressionId { get; set; }
    public TableEntity? EntityInfo { get; set; }
}
