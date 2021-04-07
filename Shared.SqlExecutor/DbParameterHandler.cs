using MDbContext.Extension;
using MDbContext.SqlExecutor.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    public class DbParameterHandler : IDbParameterHandle {
        private static Dictionary<Certificate, Action<IDbCommand, object>> parameterReaderCache = new Dictionary<Certificate, Action<IDbCommand, object>>();
        private readonly Dictionary<string, ParamInfo> parameters = new Dictionary<string, ParamInfo>();
        private object temp;
        public DbParameterHandler(object parameter) {
            if (parameter is IEnumerable<KeyValuePair<string, object>>) {
                //
                ReadEnumerableParameter(parameter as IEnumerable<KeyValuePair<string, object>>);
            } else {
                //
                temp = parameter;
            }
        }

        private void ReadEnumerableParameter(IEnumerable<KeyValuePair<string, object>> enumerable) {
            foreach (KeyValuePair<string, object> item in enumerable) {
                Add(item.Key, item.Value, null, null, null);
            }
        }


        private void Add(string name, object value, DbType? dbType, ParameterDirection? pd, int? size) {
            parameters.Add(Clean(name), new ParamInfo {
                Name = name,
                DbType = dbType,
                Value = value,
                ParameterDirection = pd,
                Size = size
            });
        }

        private string Clean(string name) {
            if (!string.IsNullOrEmpty(name)) {
                char c = name[0];
                if (c == ':' || c == '?' || c == '@') {
                    return name.Substring(1);
                }
            }
            return name;
        }

        struct ParamInfo {
            public object Value { get; set; }
            public DbType? DbType { get; set; }
            public string Name { get; set; }
            public IDbDataParameter Parameter { get; set; }
            public ParameterDirection? ParameterDirection { get; set; }
            public int? Size { get; set; }
        }

        public void AddDbParameter(IDbCommand cmd, Certificate certificate) {
            if (temp != null) {
                if (!parameterReaderCache.TryGet(certificate, out var value)) {
                    // create reader
                    lock (parameterReaderCache) {
                        value = CreateReader(certificate);
                        parameterReaderCache.TryAdd(certificate, value);
                    }

                }
                // 对于 object 参数，运行Action后，参数添加到 IDbCommand 实例的 Parameters 中
                value.Invoke(cmd, temp);

                foreach (IDbDataParameter parameter in cmd.Parameters) {
                    if (!parameters.ContainsKey(parameter.ParameterName)) {
                        parameters.Add(parameter.ParameterName, new ParamInfo {
                            Parameter = parameter,
                            DbType = parameter.DbType,
                            Name = parameter.ParameterName,
                            ParameterDirection = parameter.Direction,
                            Size = parameter.Size,
                            Value = parameter.Value
                        });
                    }
                }
            }
            //

            foreach (ParamInfo item in parameters.Values) {
                IDbDataParameter p;
                var name = item.Name;
                var dbType = GetDbType(item.Value);
                var flag = !cmd.Parameters.Contains(name);
                if (flag) {
                    p = cmd.CreateParameter();
                } else {
                    p = (IDbDataParameter)cmd.Parameters[name];
                }
                if (dbType.HasValue && p.DbType != dbType)
                    p.DbType = dbType.Value;
                p.ParameterName = item.Name;
                p.Value = item.Value;

                if (flag) {
                    cmd.Parameters.Add(p);
                }
            }
        }

        private DbType? GetDbType(object value) {
            var t = value.GetType();
            if (typeMap.TryGetValue(t, out var v))
                return v;
            else return default;
        }

        public Action<IDbCommand, object> CreateReader(Certificate certificate) {
            /*
             * (cmd, obj) => { 
             *    var p = cmd.CreateParameter();
             *    p.Name = obj.XXX;
             *    p.Value = obj.XXX;
             *    cmd.Parameters.Add(p);
             * }
             */
            // (cmd, obj) => 
            MethodInfo createParameterMethodInfo = typeof(IDbCommand).GetMethod("CreateParameter");
            PropertyInfo parameterCollection = typeof(IDbCommand).GetProperty("Parameters");
            MethodInfo listAddMethodInfo = typeof(IList).GetMethod("Add");

            ParameterExpression cmdExp = Expression.Parameter(typeof(IDbCommand), "cmd");
            ParameterExpression objExp = Expression.Parameter(typeof(object), "obj");
            var objType = certificate.ParameterType;;
            var props = ExtractParameter(certificate, objType.GetProperties());
            // var temp
            var tempExp = Expression.Variable(typeof(IDataParameter), "temp");
            // var p = (Type)obj;
            var p1 = Expression.Variable(objType, "p");
            var paramExp = Expression.Assign(p1, Expression.Convert(objExp, objType));
            List<Expression> body = new List<Expression>();
            body.Add(tempExp);
            body.Add(paramExp);
            foreach (PropertyInfo prop in props) {
                // cmd.CreateParameter()
                var createParam = Expression.Call(cmdExp, createParameterMethodInfo);
                // temp = cmd.CreateParameter()                
                var pAssign = Expression.Assign(tempExp, createParam);
                var paramNameExp = Expression.Property(tempExp, "ParameterName");
                var valueExp = Expression.Property(tempExp, "Value");
                // temp.ParameterName = prop.Name
                var nameAssignExp = Expression.Assign(paramNameExp, Expression.Constant(prop.Name));
                // temp.Value = p.PropertyValue
                var valueAssignExp = Expression.Assign(valueExp, Expression.Convert(Expression.Property(paramExp, prop), typeof(object)));
                // cmd.Parameters.Add(temp)
                var addToList = Expression.Call(Expression.Property(cmdExp, parameterCollection), listAddMethodInfo, tempExp);

                body.Add(pAssign);
                body.Add(nameAssignExp);
                body.Add(valueAssignExp);
                body.Add(addToList);
            }
            var block = Expression.Block(new[] { tempExp, p1 }, body);
            var lambda = Expression.Lambda<Action<IDbCommand, object>>(block, cmdExp, objExp);
            return lambda.Compile();
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
