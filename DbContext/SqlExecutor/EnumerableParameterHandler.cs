using MDbContext.SqlExecutor.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    internal class EnumerableParameterHandler : IDbParameterHandle {
        public void AddDbParameter(IDbCommand cmd, object param) {
            throw new NotImplementedException();
        }
    }
}
