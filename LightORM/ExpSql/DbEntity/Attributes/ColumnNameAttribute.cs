using System;
using System.Collections.Generic;
using System.Text;

namespace MDbEntity.Attributes {
    public class ColumnNameAttribute : Attribute {
        public string Name { get; set; }
        public ColumnNameAttribute(string name) {
            Name = name;
        }
    }
}
