using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest;

public class DbMethodTest : TestBase
{
    [TestMethod]
    public void TestToString()
    {

        var p = Expression.Parameter(typeof(JobFile), "p");
        var propExp = Expression.Property(p, nameof(JobFile.JFL_DATE));
        var valueExp = Expression.Constant(DateTime.Now, typeof(DateTime?));
        var body = Expression.MakeBinary(ExpressionType.Equal, propExp, valueExp);
        var lambda = Expression.Lambda<Func<JobFile, bool>>(body, p);

        var rr = lambda.Resolve(SqlResolveOptions.Where, ResolveCtx);

        var today = DateTime.Now;
        Expression<Func<User, bool>> exp = u => u.LastLogin!.Value.ToString("yyyy-MM-dd") == today.ToString("yyyy-MM-dd");
        var r1 = exp.Resolve(SqlResolveOptions.Where, ResolveCtx);
        var r2 = exp.Resolve(SqlResolveOptions.Where, ResolveCtx);
        Console.WriteLine(r1.SqlString);
    }

    [TestMethod]
    public void T()
    {
        using var jdcScoped = Db.CreateMainDbScoped();
        var job = new { JOB_ID = 123 };
        string[] where = ["0102"];
        var sql = jdcScoped.Update<JobFile>()
              .Set(f => f.JFL_OK, 0)
              .Set(f => f.JFL_EXIST, 0)
              .Where(f => f.JOB_ID == job.JOB_ID)
              .Where(f => where.Contains(f.FLT_ID!.Trim())).ToSqlWithParameters();
        Console.WriteLine(sql);

        var sql2 = jdcScoped.Update<JobFile>()
              .Set(f => f.JFL_OK, 0)
              .Set(f => f.JFL_EXIST, 0)
              .Where(f => f.JOB_ID == job.JOB_ID)
              .Where(f => where.Contains(f.FLT_ID!.Trim())).ToSqlWithParameters();
        Console.WriteLine(sql2);
    }

    [LightTable(Name = "JOBFILES")]
    public class JobFile
    {
        public int JOB_ID { get; set; }

        public string? FLT_ID { get; set; }

        public string? FLT_CATEGORY { get; set; }

        public int JFL_EXIST { get; set; }

        public int? JFL_OK { get; set; }

        public int? JFL_NO { get; set; }

        public DateTime? JFL_DATE { get; set; }

        public int? JFL_NEED { get; set; }

        public string? JFL_REMARK { get; set; }

        public string? JFL_NAME { get; set; }

        [LightColumn(Ignore = true)]
        public int Rotate { get; set; }
        [LightColumn(Ignore = true)]
        public bool Uploading { get; set; }
        [LightColumn(Ignore = true)]
        public long Timestamp { get; set; } = DateTime.Now.Ticks;
    }
}
