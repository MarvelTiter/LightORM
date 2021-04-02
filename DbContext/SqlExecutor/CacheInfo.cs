using MDbContext.Extension;
using MDbContext.SqlExecutor.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    internal class CacheInfo {
        internal static Dictionary<Certificate, CacheInfo> cache = new Dictionary<Certificate, CacheInfo>();

        public Action<IDbCommand, object> ParameterReader { get; set; }

        public Func<IDataReader, object> Deserializer { get; set; }

        internal static CacheInfo GetCacheInfo(Certificate certificate, object parameters) {
            if (cache.TryGet(certificate, out CacheInfo value))
                return value;

            CacheInfo info = new CacheInfo();
            if (certificate.ParameterType != null) {
                Action<IDbCommand, object> action;
                if (parameters is IEnumerable<KeyValuePair<string, object>>) {
                    action = (cmd, obj) => {
                        IDbParameterHandle handler = new EnumerableParameterHandler(certificate);
                        handler.AddDbParameter(cmd, obj);
                    };
                } else {
                    action = (cmd, obj) => {
                        IDbParameterHandle handler = new EntityParameterHandler(certificate);
                        handler.AddDbParameter(cmd, obj);
                    };
                }
                info.ParameterReader = action;
            }
            cache.TryAdd(certificate, info);
            return info;
        }
    }
}
