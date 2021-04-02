using MDbContext.Extension;
using MDbContext.SqlExecutor.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    internal class EntityParameterHandler : IDbParameterHandle {
        private readonly Certificate certificate;
        private static Dictionary<Certificate, Action<IDbCommand, object>> parameterReaderCache = new Dictionary<Certificate, Action<IDbCommand, object>>();
        private static Dictionary<Certificate, Dictionary<string, ParamInfo>> paramInfoCache = new Dictionary<Certificate, Dictionary<string, ParamInfo>>();
        public EntityParameterHandler(Certificate certificate) {
            this.certificate = certificate;
        }

        struct ParamInfo {
            public object Value { get; set; }
            public DbType? DbType { get; set; }
            public string Name { get; set; }
            public IDbDataParameter Parameter { get; set; }
        }

        public void AddDbParameter(IDbCommand cmd, object parameter) {
            if (parameterReaderCache.TryGet(certificate, out Action<IDbCommand, object> value)) {
                value.Invoke(cmd, parameter);
                return;
            }
            //
            lock (parameterReaderCache) {
                value = BuildAction();
                parameterReaderCache.TryAdd(certificate, value);
            }
        }

        private Action<IDbCommand, object> BuildAction() {

            return (cmd, parameter) => {
                if (!paramInfoCache.TryGet(certificate, out Dictionary<string, ParamInfo> dic)) {
                    var props = ExtractParameter(certificate, parameter.GetType().GetProperties());
                    foreach (PropertyInfo property in props) {
                        var temp = new ParamInfo {
                            Name = property.Name
                        };
                        if (typeMap.TryGet(property.PropertyType, out DbType dbType)) {
                            temp.DbType = dbType;
                        }
                        dic.Add(property.Name, temp);
                    }
                    paramInfoCache.TryAdd(certificate, dic);
                }

                foreach (ParamInfo item in dic.Values) {
                    IDbDataParameter p;
                    var name = item.Name;
                    if (cmd.Parameters.Contains(name))
                        p = (IDbDataParameter)cmd.Parameters[name];
                    else
                        p = cmd.CreateParameter();
                    if (item.DbType.HasValue)
                        p.DbType = item.DbType.Value;
                    p.ParameterName = item.Name;
                    p.Value = item.Value;
                }
            };
        }



        private IEnumerable<PropertyInfo> ExtractParameter(Certificate certificate, params PropertyInfo[] parameters) {
            foreach (PropertyInfo parameter in parameters) {
                if (Regex.IsMatch(certificate.Sql, "[?@:]" + parameter.Name + "([^\\p{L}\\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant)) {
                    yield return parameter;
                }
            }
        }

        readonly static Dictionary<Type, DbType> typeMap = new Dictionary<Type, DbType>(37) {
            [typeof(byte)] = DbType.Byte,
            [typeof(sbyte)] = DbType.SByte,
            [typeof(short)] = DbType.Int16,
            [typeof(ushort)] = DbType.UInt16,
            [typeof(int)] = DbType.Int32,
            [typeof(uint)] = DbType.UInt32,
            [typeof(long)] = DbType.Int64,
            [typeof(ulong)] = DbType.UInt64,
            [typeof(float)] = DbType.Single,
            [typeof(double)] = DbType.Double,
            [typeof(decimal)] = DbType.Decimal,
            [typeof(bool)] = DbType.Boolean,
            [typeof(string)] = DbType.String,
            [typeof(char)] = DbType.StringFixedLength,
            [typeof(Guid)] = DbType.Guid,
            [typeof(DateTime)] = DbType.DateTime,
            [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
            [typeof(TimeSpan)] = DbType.Time,
            [typeof(byte[])] = DbType.Binary,
            [typeof(byte?)] = DbType.Byte,
            [typeof(sbyte?)] = DbType.SByte,
            [typeof(short?)] = DbType.Int16,
            [typeof(ushort?)] = DbType.UInt16,
            [typeof(int?)] = DbType.Int32,
            [typeof(uint?)] = DbType.UInt32,
            [typeof(long?)] = DbType.Int64,
            [typeof(ulong?)] = DbType.UInt64,
            [typeof(float?)] = DbType.Single,
            [typeof(double?)] = DbType.Double,
            [typeof(decimal?)] = DbType.Decimal,
            [typeof(bool?)] = DbType.Boolean,
            [typeof(char?)] = DbType.StringFixedLength,
            [typeof(Guid?)] = DbType.Guid,
            [typeof(DateTime?)] = DbType.DateTime,
            [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
            [typeof(TimeSpan?)] = DbType.Time,
            [typeof(object)] = DbType.Object
        };
    }
}
