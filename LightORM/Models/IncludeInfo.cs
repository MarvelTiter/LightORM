using LightORM.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Models;

internal class IncludeInfo
{
    public TableEntity? SelectedTable { get; set; }
    public NavigateInfo? NavigateInfo { get; set; }
    public ColumnInfo? ParentNavigateColumn { get; set; }
    public ColumnInfo? ParentWhereColumn { get; set; }
    public SelectBuilder? SqlBuilder { get; set; }
    public ExpressionResolvedResult? ExpressionResolvedResult { get; set; }

}
