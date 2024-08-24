using LightORM.Interfaces;
using LightORM.Models;

namespace TestProject1;

/// <summary>
/// 产品
/// </summary>
[LightTable]
public class Product
{
    [LightColumn(PrimaryKey = true)]
    public int ProductId { get; set; }

    public int CategoryId { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public bool DeleteMark { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? ModifyTime { get; set; }
    public DateTime? Last { get; set; } = DateTime.Now;
    public IEnumerable<Product> Products { get; set; } = [];
    public Product[] Products2 { get; set; } = [];
}


internal sealed record ProductContext : ITableEntityInfo
{
    public Type Type { get; } = typeof(Product);
    public string TableName => CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常");
    public string? Alias { get; set; } = "a1";
    public string? CustomName { get; set; }
    public bool IsAnonymousType => false;
    public string? TargetDatabase => null;
    public string? Description => null;
    ITableColumnInfo[]? columns;
    public ITableColumnInfo[] Columns => columns ??= CollectColumnInfo();

    private ITableColumnInfo[] CollectColumnInfo()
    {
        columns = new ITableColumnInfo[10];
        //columns[0] = new ColumnInfo(this)
        return columns;
    }

    public object? GetValue(ITableColumnInfo col, object target)
    {
        var p = target as Product;
        ArgumentNullException.ThrowIfNull(p);
        if (!col.CanRead)
            return null;
        switch (col.PropertyName)
        {
            case "ProductId":
                return p.ProductId;
            case "CategoryId":
                return p.CategoryId;
            default:
                throw new ArgumentException();
        }
    }

    public void SetValue(ITableColumnInfo col, object target, object? value)
    {
        var p = target as Product;
        ArgumentNullException.ThrowIfNull(p);
        if (value == null) return;
        switch (col.PropertyName)
        {
            case "ProductId":
                p.ProductId = (int)value;
                break;
            case "CategoryId":
                p.CategoryId = (int)value;
                break;
            case "Products":
                p.Products = (List<Product>)value;
                break;
            default:
                throw new ArgumentException();
        }
    }
}