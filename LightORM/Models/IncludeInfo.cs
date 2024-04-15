using LightORM.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Models;

internal class IncludeContext
{
    public IncludeContext(DbBaseType dbBase)
    {
        DbType = dbBase;
    }
    public DbBaseType DbType { get; set; }
    public List<IncludeInfo> Includes { get; set; } = [];
    public IncludeContext? ThenInclude { get; set; }
}

internal class IncludeInfo
{
    public TableEntity? SelectedTable { get; set; }
    public NavigateInfo? NavigateInfo { get; set; }
    public ColumnInfo? ParentNavigateColumn { get; set; }
    public ColumnInfo? ParentWhereColumn { get; set; }
    public SelectBuilder? SqlBuilder { get; set; }
    public ExpressionResolvedResult? ExpressionResolvedResult { get; set; }

}
