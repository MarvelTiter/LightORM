using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Providers.Oracle;

public record OracleTableOptions : TableOptions
{

    /// <summary>
    /// 当前仅Oracle对此属性有支持，高级版本的Oracle，适用自增列语法 GENERATED ALWAYS AS IDENTITY
    /// </summary>
    public bool OverVersion { get; set; }
    public string? TableSpace { get; set; }
    public string? UserId { get; set; }
}
