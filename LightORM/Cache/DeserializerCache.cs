using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LightORM.Cache;

internal class DeserializerCache
{
    public static Func<IDataReader, T> GetDeserializer<T>(IDataReader reader)
    {
        throw new NotImplementedException();
    }
}
