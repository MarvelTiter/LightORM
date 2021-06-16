using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext {
    public static class ConnectionEx {
        public static DbContext DbContext(this IDbConnection self) {
            return new DbContext(self);
        }
    }
}
