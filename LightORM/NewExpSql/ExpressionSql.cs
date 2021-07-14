using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql {
    public class ExpressionSql {
        private readonly DbBaseType dbType;

        public ExpressionSql(DbBaseType dbType) {
            this.dbType = dbType;
        }

        internal ISqlContext Context { get; private set; }

        public ExpressionSqlCore<T> Select<T>(Expression<Func<T, object>> exp = null, bool distanct = false) {
            var core = new ExpressionSqlCore<T>(dbType).Select(exp, distanct);
            Context = core.Context;
            return core;
        }

        public ExpressionSqlCore<T> Select<T, T1>(Expression<Func<T, T1, object>> exp = null, bool distanct = false) {
            var core = new ExpressionSqlCore<T>(dbType).Select(exp, distanct);
            Context = core.Context;
            return core;
        }
#if DEBUG
        public override string ToString() {
            var sql = Context.BuildSql(out var dic);
            return sql + "\n" + paramString();

            string paramString() {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, object> item in dic) {
                    sb.AppendLine($"[{item.Key},{item.Value}]");
                }
                return sb.ToString();
            }
        }
#endif
    }
}
