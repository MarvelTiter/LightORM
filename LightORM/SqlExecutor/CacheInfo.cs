﻿using MDbContext.Extension;
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
            if (cache.TryGetValue(certificate, out CacheInfo value))
                return value;

            CacheInfo info = new CacheInfo();
            if (certificate.ParameterType != null) {
                Action<IDbCommand, object> action;
                // IDictionary, Object
                action = (cmd, obj) => {
                    IDbParameterHandle handler = new DbParameterHandler(obj);
                    handler.AddDbParameter(cmd, certificate);
                };
                info.ParameterReader = action;
            }
            cache.Add(certificate, info);
            return info;
        }
    }
}
