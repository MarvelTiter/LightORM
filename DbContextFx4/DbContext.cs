using DExpSql;
using MDbAction;
using MDbAction.IServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MDbContext {
    public class DbContext : IDisposable {
        public ExpressionSql DbSet { get; private set; }
        internal IDbAction DbExec { get; private set; }

        public string Sql => DbSet.SqlCaluse.Sql.ToString();
        public object SqlParameter {
            get {
                var temp = new Dictionary<string, object>();
                foreach (var item in DbSet.SqlCaluse.SqlParam) {
                    temp.Add(item.Key, item.Value);
                }
                return temp;
            }
        }

        public List<string> TransSqls { get; set; }
        public List<object> TransSqlParameter { get; set; }

        private static int DBType;
        public static DbContext Instance(IDbConnection conn) {
            return new DbContext(conn);
        }
        public static DbContext Instance(int type, IDbConnection conn) {
            return new DbContext(type, conn);
        }
        /// <summary>
        /// 初始化数据库类型 0 - SqlServer; 1 - Oracle; 2 - MySql
        /// </summary>
        /// <param name="dBType">0 - SqlServer; 1 - Oracle; 2 - MySql</param>
        public static void Init(int dBType) {
            DBType = dBType;
        }

        public DbContext(int type, IDbConnection connection) {
            DbExec = new DbAction(connection);
            DbSet = new ExpressionSql(type);
        }

        public DbContext(IDbConnection connection) {
            DbExec = new DbAction(connection);
            DbSet = new ExpressionSql(DBType);
        }


        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool m_disposed;

        protected virtual void Dispose(bool disposing) {
            if (!m_disposed) {
                if (disposing) {
                    // Release managed resources                   
                    DbSet = null;
                    DbExec = null;
                }

                // Release unmanaged resources

                m_disposed = true;
            }
        }

        ~DbContext() {
            Dispose(false);
        }
    }
}
