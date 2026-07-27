using System;
using System.Collections.Generic;
using System.Text;

namespace LightORMTest.Models;

[LightTable(Name = "sales")]
public class Sales
{
    [LightColumn(Name = "region")]
    public string? Region { get; set; }
    [LightColumn(Name = "province")]
    public string? Province { get; set; }
    [LightColumn(Name = "product")]
    public string? Product { get; set; }
    [LightColumn(Name = "amount")]
    public int Amount { get; set; }
}
