namespace TestProject1;

/// <summary>
/// 产品
/// </summary>
public class Product
{
    [LightColumn(PrimaryKey = true)]
    public int ProductId { get; set; }

    public int CategoryId { get; set; }

    public string ProductCode { get; set; }

    public string ProductName { get; set; }

    public bool DeleteMark { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? ModifyTime { get; set; }
    public DateTime? Last { get; set; } = DateTime.Now;
}
