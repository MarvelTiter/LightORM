using MDbContext.SqlExecutor.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    public static class SqlExecutor {
        public static int Execute(this IDbConnection self, string sql, object param) {
            return 0;
        }

        public static DataTable ExecuteReader(this IDbConnection self, string sql, object param) {
            CommandDefinition command = new CommandDefinition(sql, param);

            return null;
        }

        public static IEnumerable<T> QueryTest<T>(this IDbConnection self, string sql, object param = null) {
            CommandDefinition command = new CommandDefinition(sql, param);
            return self.internalQuery<T>(command);
        }

        public static T QuerySingle<T>(this IDbConnection self, string sql, object param) {
            return default;
        }

        private static IEnumerable<T> internalQuery<T>(this IDbConnection conn, CommandDefinition command) {
            // 缓存
            var parameter = command.Parameters;
            Certificate certificate = new Certificate(command.CommandText, command.CommandType, conn, typeof(T), parameter?.GetType());
            CacheInfo cacheInfo = CacheInfo.GetCacheInfo(certificate, parameter);
            // 读取
            IDbCommand cmd;
            IDataReader reader;
            try {
                cmd = command.SetupCommand(conn, cacheInfo.ParameterReader);
                if (conn.State == ConnectionState.Closed)
                    conn.Open();
                reader = cmd.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
                if (cacheInfo.Deserializer == null) {
                    cacheInfo.Deserializer = BuildDeserializer(reader, typeof(T));
                }
                while (reader.Read()) {
                    var val = cacheInfo.Deserializer(reader);
                    yield return GetValue<T>(val);
                }
            } finally {
                // dispose

            }

        }

        private static Func<IDataReader, object> BuildDeserializer(IDataReader reader, Type type) {
            IDeserializer des = new ExpressionBuilder();
            return des.BuildDeserializer(reader, type);
        }

        private static T GetValue<T>(object val) {
            if (val is T) {
                return (T)val;
            }
            Type effectiveType = typeof(T);
            if (val == null && (!effectiveType.IsValueType || Nullable.GetUnderlyingType(effectiveType) != null)) {
                return default(T);
            }

            try {
                Type conversionType = Nullable.GetUnderlyingType(effectiveType) ?? effectiveType;
                return (T)Convert.ChangeType(val, conversionType, CultureInfo.InvariantCulture);
            } catch (Exception ex) {
                return default(T);
            }
        }
    }
}
