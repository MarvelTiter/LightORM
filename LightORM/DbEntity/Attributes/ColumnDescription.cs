using System;
using System.Collections.Generic;
using System.Text;

namespace MDbEntity.Attributes {
    public class ColumnDescription :Attribute {
        public string Description { get; set; }
        public ColumnDescription(string description) {
            Description = description;
        }
    }   
}
