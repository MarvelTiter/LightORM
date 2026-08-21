using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Dameng;

public record DamengTableOptions : TableOptions
{
    public string? TableSpace { get; set; }
    public string? UserId { get; set; }
}
