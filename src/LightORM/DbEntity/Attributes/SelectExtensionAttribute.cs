using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.DbEntity.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal class SelectExtensionAttribute : Attribute
    {
        public int ArgumentCount { get; set; }
    }
}
