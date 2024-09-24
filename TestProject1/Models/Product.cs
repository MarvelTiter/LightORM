using LightORM.Interfaces;
using LightORM.Models;

namespace TestProject1.Models;

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
    [LightORM.Ignore]
    public IEnumerable<Product> Products { get; set; } = [];
    [LightORM.Ignore]
    public Product[] Products2 { get; set; } = [];
}