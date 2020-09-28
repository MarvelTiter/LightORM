using MDbContext;
using DExpSql.ExpressionMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebTest.Models.Entities;
using WebTest.Models.RequestModel;

namespace WebTest.Services
{
    public class JobService : IJobService
    {
        public JobService()
        {
            DbContext.Init(1, "Data Source=192.168.56.11/orcl;Persist Security Info=True;User ID=CGS;Password=CGS2020");
        }
        public IEnumerable<JOBS> JobsList(JOBSRequest req)
        {
            using (var db = DbContext.Instance)
            {
                var sql = db.DbSet.Select<JOBS>();
                if (!string.IsNullOrEmpty(req.JYLSH))
                    sql.Where(j => j.JYLSH.Like(req.JYLSH));
                sql.Paging((req.PageIndex - 1) * req.PageSize, req.PageIndex * req.PageSize);
                return db.Query<JOBS>();
            }
        }
    }
}
