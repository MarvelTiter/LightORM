using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbEntity.Attributes
{
    public class DisplayAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public DisplayAttribute(string name)
        {
            DisplayName = name;
        }
    }
}
