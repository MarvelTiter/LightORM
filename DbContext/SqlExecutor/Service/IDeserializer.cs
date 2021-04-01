using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor.Service {
    public interface IDeserializer {
        Func<IDataReader, object> BuildDeserializer(IDataReader reader, Type targetType);
    }
}
