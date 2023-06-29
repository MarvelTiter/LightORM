using MDbContext.DbStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbEntity.Attributes
{
    public class TableNameAttribute : Attribute
    {
        public string TableName { get; set; }
        public TableNameAttribute(string name)
        {
            TableName = name;
        }
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        public string? Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TableIndexAttribute : Attribute
    {
        public IList<string>? Indexs { get; set; }
        public IndexType DbIndexType { get; set; }
        public string? Name { get; set; }
        public bool IsUnique { get; set; }
        public bool IsClustered { get; set; }
        public TableIndexAttribute(params string[] indexs)
        {
            Indexs = indexs;
        }
    }
}
