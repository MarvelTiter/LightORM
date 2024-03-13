using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.SqlExecutor.Service;
internal interface IDeserializer
{
    Func<IDataReader, object> BuildDeserializer<T>(IDataReader reader);
}
