using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseUtils.Template
{
    public class ClassTemplate
    {
        /// <summary>
        /// 参数顺序 命名空间 -> 表名 -> 类名 -> 内容
        /// </summary>
        public static string Class => 
$@"/*
 * 该文件由代码生成
 * 时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}
 * 作者：yaoqinglin
 */
using LightORM;
namespace {{0}};

[LightTable(Name = ""{{1}}"")]
public class {{2}}
{{{{
    {{3}}
}}}}
";
        /// <summary>
        /// 参数顺序 注释-> 列名 -> 类型 -> 属性名
        /// </summary>
        public static string Property =>
@"
    /// <summary>
    /// {0}
    /// </summary>
    [LightColumnName(""{1}"")]
    public {2} {3} {{ get; set; }}
";
        /// <summary>
        /// 参数顺序 注释-> 列名 -> 类型 -> 属性名
        /// </summary>
        public static string PropertyWidthTableColumn =>
@"
    /// <summary>
    /// {0}
    /// </summary>
    [LightColumnName(""{1}"")]
    [ColumnDefinition(""{0}"")]
    public {2} {3} {{ get; set; }}
";
    }
}
