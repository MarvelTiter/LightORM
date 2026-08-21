using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.MySql;

public record MySqlTableOptions : TableOptions
{
    public string? Engine { get; set; } = "InnoDB";
    public string? Charset { get; set; } = "utf8mb4";
    public string? Collation { get; set; } = "utf8mb4_unicode_ci";
}
