using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Models;

public class DataBaseOption
{
    public DataBaseOption(ISqlMethodResolver methodResolver)
    {
        MethodResolver = methodResolver;
    }
    public string? DbKey { get; set; }
    public string? MasterConnectionString { get; set; }
    public string[]? SalveConnectionStrings { get; set; }
    public ISqlMethodResolver MethodResolver { get; }
}
