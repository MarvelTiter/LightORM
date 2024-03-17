using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Models;

internal class ExpressionResolvedResult
{
    public string? SqlString { get; set; }
    public Dictionary<string,object>? DbParameters { get; set; }
}
