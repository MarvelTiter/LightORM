using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql {
    public static class StringExtension {
        public static string If(this string self, bool condition) {
            return condition ? self : "";
        }        
    }
}
