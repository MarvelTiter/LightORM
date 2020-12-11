using MDbContext;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Test.Models.Entities;
using Test.Models.QueryParam;

namespace Test.Services {
    class UserService : IUserService {
        private readonly string connString;
        DbContext db;
        public UserService() {
            connString = "Data Source=192.168.56.11;Database=APDSDB3IN1;User ID=sa;Password=Ybeluoek3;";
            DbContext.Init(0);
        }
        public int Insert(Users user) {
            using (SqlConnection conn = new SqlConnection(connString)) {
                db = DbContext.Instance(conn);
                db.DbSet.Insert(user);
                return db.Excute();
            }
        }

        public IEnumerable<Users> Select(UserQueryParam p) {
            using (SqlConnection conn = new SqlConnection(connString)) {
                db = DbContext.Instance(conn);
                db.DbSet.Select<Users>();
                return db.Query<Users>();
            }
        }

        public int Update(Users user) {
            using (SqlConnection conn = new SqlConnection(connString)) {
                db = conn.DbContext();
                db.DbSet.Update<Users>(() => new { Age = 12 }).Where(u => u.Account == "001");
                db.AddTrans();
                db.DbSet.Update<Users>(() => new { Sex = "男" }).Where(u => u.Account == "001");
                db.AddTrans();
                return db.ExecuteTrans() ? 1 : 0;

            }
        }

        public int Update(object dyc) {
            throw new NotImplementedException();
        }
    }
}
