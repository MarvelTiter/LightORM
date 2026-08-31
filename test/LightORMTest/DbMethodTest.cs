using System.Linq.Expressions;

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
