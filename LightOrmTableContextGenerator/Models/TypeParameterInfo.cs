using System;
using System.Collections.Generic;
using System.Text;

namespace LightOrmTableContextGenerator.Models
{
    internal class TypeParameterInfo(string name, string[] constraints)
    {
        public string Name { get; set; } = name;
        public string[] Constraints { get; set; } = constraints;
    }
}
