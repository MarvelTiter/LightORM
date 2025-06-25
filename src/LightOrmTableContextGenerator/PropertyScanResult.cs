using System;
using System.Collections.Generic;
using System.Text;

namespace LightOrmTableContextGenerator;

struct PropertyScanResult
{
    public string CustomName { get; set; }
    public string PrimaryKey { get; set; }
    public string IsNotMap { get; set; }
    public string AutoIncrement { get; set; }
    public string NotNull { get; set; }
    public string? Len { get; set; }
    public string? DefaultValue { get; set; }
    public string? Comment { get; set; }
    public string CanRead { get; set; }
    public string CanWrite { get; set; }
    public string CanInit { get; set; }
    public string NavInfo { get; set; }
}
