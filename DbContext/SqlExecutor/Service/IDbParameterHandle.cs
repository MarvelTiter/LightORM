using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor.Service {
    internal interface IDbParameterHandle {
        void AddDbParameter(IDbCommand cmd, object param);
    }
}
