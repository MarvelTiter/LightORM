using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.PostgreSQL;

public record PostgreSQLTableOptions: TableOptions
{
    public string? TableSpace { get; set; }
}
