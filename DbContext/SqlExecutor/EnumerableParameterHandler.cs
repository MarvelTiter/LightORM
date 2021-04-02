using MDbContext.SqlExecutor.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    internal class EnumerableParameterHandler : IDbParameterHandle {
        private readonly Certificate certificate;

        public EnumerableParameterHandler(Certificate certificate) {
            this.certificate = certificate;
        }
        public void AddDbParameter(IDbCommand cmd, object param) {
            throw new NotImplementedException();
        }
    }
}
