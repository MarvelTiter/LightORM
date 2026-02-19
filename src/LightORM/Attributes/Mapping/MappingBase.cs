using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Attributes.Mapping;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MappingBaseAttribute: Attribute
{
    /// <summary>
    /// Attribute全名
    /// </summary>
    public string? AttributeName { get; set; }
    /// <summary>
    /// 属性名称
    /// </summary>
    public string? PropertyName { get; set; }
}
public class TableNameMappingAttribute : MappingBaseAttribute;
public class ColumnNameMappingAttribute : MappingBaseAttribute;
public class PrimaryKeyMappingAttribute : MappingBaseAttribute;
public class AutoIncrementMappingAttribute : MappingBaseAttribute;
public class NotNullMappingAttribute : MappingBaseAttribute;
public class LengthMappingAttribute : MappingBaseAttribute;
public class DefaultMappingAttribute : MappingBaseAttribute;
public class CommentMappingAttribute : MappingBaseAttribute;
public class VersionMappingAttribute : MappingBaseAttribute;



