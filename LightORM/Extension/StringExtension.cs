using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.Extension {
    internal static class StringExtension {
        public static string If(this string self, bool condition) {
            return condition ? self : "";
        }        
    }
}
