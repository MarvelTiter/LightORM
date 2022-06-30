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
}
