using LightORM.SqlExecutor.Service;
using System.Collections.Generic;
using System.Data;

namespace LightORM.SqlExecutor;

internal class CacheInfo
{
    internal static Dictionary<Certificate, CacheInfo> cache = new Dictionary<Certificate, CacheInfo>();

    public Action<IDbCommand, object?>? ParameterReader { get; set; }

    public Func<IDataReader, object>? Deserializer { get; set; }

    internal static CacheInfo GetCacheInfo(Certificate certificate, object? parameters)
    {
        if (cache.TryGetValue(certificate, out CacheInfo value))
            return value;

        CacheInfo info = new CacheInfo();
        if (certificate.ParameterType != null)
        {
            //Action<IDbCommand, object?> action;
            //// IDictionary, Object
            //action = (cmd, obj) =>
            //{
            //    IDbParameterHandle handler = new DbParameterHandler(obj);
            //    handler.AddDbParameter(cmd, certificate);
            //};
            //info.ParameterReader = action;
        }
        cache.Add(certificate, info);
        return info;
    }
}
