using LightORM.Interfaces;
using LightORM.Models;

namespace LightORMTest.Models;

/// <summary>
/// 产品
/// </summary>
[LightTable(Name = "PRODUCTS")]
public class Product
{
    [LightColumn(Name = "PRODUCT_ID", PrimaryKey = true, AutoIncrement = true, Comment = "产品ID")]
    public int ProductId { get; set; }
    [LightColumn(Name = "CATEGORY_ID", Comment = "产品类型")]
    public int CategoryId { get; set; }
    [LightColumn(Name = "PRODUCT_CODE", Comment = "产品代码")]
    public string ProductCode { get; set; } = string.Empty;
    [LightColumn(Name = "PRODUCT_NAME", Comment = "产品名称")]
    public string ProductName { get; set; } = string.Empty;
    [LightColumn(Name = "DELETE_MARK", Comment = "删除")]
    public bool DeleteMark { get; set; }
    [LightColumn(Name = "CREATE_TIME", Comment = "创建时间")]
    public DateTime CreateTime { get; init; } = DateTime.Now;
    [LightColumn(Name = "MODIFY_TIME", Comment = "修改时间")]
    public DateTime? ModifyTime { get; set; }
    [LightORM.Ignore]
    public IEnumerable<Product> Products { get; set; } = [];
}