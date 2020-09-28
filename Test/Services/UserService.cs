using DExpSql.ExpressionMethod;
using MDbContext;
using System;
using System.Collections.Generic;
using Test.Models.Entities;
using Test.Models.QueryParam;

namespace Test.Services
{
    class UserService : IUserService
    {
        public UserService()
        {
            DbContext.Init(0, "Data Source=192.168.0.104;Initial Catalog=APDSDB2020; User Id=sa;Password=sa");
        }
        public int Insert(Users user)
        {
            using (var db = DbContext.Instance)
            {
                db.DbSet.Insert(user);
                return db.Excute();
            }
        }

        public IEnumerable<Users> Select(UserQueryParam p)
        {
            using (var db = DbContext.Instance)
            {
                db.DbSet.Select<Users>();
                    //.Where(u => u.Account.RightLike(p.Account));
                var sql = db.DbSet.SqlCaluse.Sql.ToString();

                return db.Query<Users>();
            }
        }

        public int Update(Users user)
        {
            using (var db = DbContext.Instance)
            {
                db.DbSet.Insert(user);
                return db.Excute();
            }
        }

        public int Update(object dyc)
        {
            throw new NotImplementedException();
        }
    }
}
