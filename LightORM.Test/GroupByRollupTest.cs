using DExpSql;
using LightORM.Test.Models;
using MDbContext;
using MDbContext.Context.Extension;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Test
{
    internal class GroupByRollupTest
    {
        private DbContext VbDbContext()
        {
            DbContext.Init(DbBaseType.Oracle);
            var conn = new OracleConnection("Data Source=192.168.56.11:1521/ORCL;Persist Security Info=True;User ID=CGS;Password=CGS2020");
            return conn.DbContext();
        }
        private DbContext Db1834Context()
        {
            DbContext.Init(DbBaseType.Oracle);
            var conn = new OracleConnection("Data Source=172.18.183.4/ORCL;Persist Security Info=True;User ID=trffpn_app;Password=trffpn_app_2014");
            return conn.DbContext();
        }
        class Model
        {
            public string FLT_CATEGORY { get; set; }
            public int? JFL_EXIST { get; set; }
            public int C { get; set; }
        }
        [Test]
        public async Task RollupTest()
        {
            var db = VbDbContext();
            var dt = await db.DbSet.Select<JobFile>(j => new
            {
                Category = SqlFn.Coalesce(() => new { j.FLT_CATEGORY }, "合计"),
                j.JFL_EXIST,
                C = SqlFn.Count()
            })
                .GroupBy(j => new { j.FLT_CATEGORY, j.JFL_EXIST }, rollup: true).ToDataTableAsync();
            foreach (DataRow item in dt.Rows)
            {
                Console.WriteLine($"{item[0]} - {item[1]} - {item[2]}");
            }
        }

        class TestModel
        {
            [TableHeader("姓名")]
            public string Name { get; set; }
            [TableHeader("审核部门")]
            public string Bmmc { get; set; }
            [TableHeader("工作量")]
            public int TotalCount { get; set; }
            public int PassCount { get; set; }
            [TableHeader("驳回数")]
            public int NoPassCount { get; set; }
            [TableHeader("摩托车总数")]
            public int MotoCount { get; set; }
            [TableHeader("摩托车通过数")]
            public int MotoPassCount { get; set; }
            [TableHeader("摩托车驳回数")]
            public int MotoNoPassCount { get; set; }
            [TableHeader("汽车总数")]
            public int SCarCount { get; set; }
            [TableHeader("汽车通过数")]
            public int SCarPassCount { get; set; }
            [TableHeader("汽车驳回数")]
            public int SCarNoPassCount { get; set; }
            [TableHeader("大车通过数")]
            public int BCarPassCount { get; set; }
            [TableHeader("大车驳回数")]
            public int BCarNoPassCount { get; set; }
            [TableHeader("通过率")]
            public string PassRate { get; set; }
            [TableHeader("驳回率")]
            public string NoPassRate { get; set; }
        }

        [Test]
        public async Task RollupTest2()
        {
            var db = Db1834Context();
            var result = await db.QueryAsync<TestModel>(@"
select COALESCE(c.Bmmc,'合计') Bmmc, COALESCE(a.shr,'合计') Name
, count(a.lsh) TotalCount
,count(case when shbj = 1 then 1 else null end) PassCount
,count(case when shbj <> 1 then 1 else null end ) NoPassCount
,count(case when b.hpzl in ('07', '08', '09', '10', '11', '12', '17', '19', '21', '24') then 1 else null end ) MotoCount
,count(case when b.hpzl not in ('07', '08', '09', '10', '11', '12', '17', '19', '21', '24') then 1 else null end ) SCarCount
,count(case when b.hpzl in ('07', '08', '09', '10', '11', '12', '17', '19', '21', '24') and SHBJ = 1 then 1 else null end ) MotoPassCount
,count(case when b.hpzl in ('07', '08', '09', '10', '11', '12', '17', '19', '21', '24') and SHBJ <> 1 then 1 else null end ) MotoNoPassCount
,count(case when b.hpzl not in ('07', '08', '09', '10', '11', '12', '17', '19', '21', '24') and SHBJ = 1 then 1 else null end ) SCarPassCount
,count(case when b.hpzl not in ('07', '08', '09', '10', '11', '12', '17', '19', '21', '24') and SHBJ <> 1 then 1 else null end ) SCarNoPassCount
,count(case when b.hpzl = '01' and SHBJ = 1 then 1 else null end ) BCarPassCount
,count(case when b.hpzl = '01' and SHBJ <> 1 then 1 else null end ) BCarNoPassCount
   from VEH_IS_CHECKCONFIRM_LOG a, VEH_IS_FLOW b, FRM_DEPARTMENT c
where a.lsh = b.lsh and a.shbm = c.glbm and c.jlzt = '1'
and b.jylb = '01'
and shrq>= to_date('2022-05-05','YYYY-MM-DD')
and shrq< to_date('2022-05-06','YYYY-MM-DD')
group by rollup(c.bmmc,a.shr)
order by c.bmmc,a.shr
", null);
            foreach (var item in result)
            {
                Console.WriteLine($"{item.Name} - {item.Bmmc} - {item.TotalCount}");
            }
        }
    }
}
