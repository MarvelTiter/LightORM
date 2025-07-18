﻿using LightORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightORM.Providers.Sqlite.Extensions;
using LightORM.Providers.Oracle.Extensions;
namespace SqlGenerateTest
{
    internal class TestBase
    {
        public IExpressionContext Db { get; }
        internal ResolveContext ResolveCtx { get; }
        protected ITableContext TableContext { get; } = new TestTableContext();
        public TestBase()
        {
            var path = Path.GetFullPath("../../../../../test.db");

            ExpSqlFactory.Configuration(option =>
            {
                //option.SetDatabase(DbBaseType.Sqlite, "DataSource=" + path, SQLiteFactory.Instance);
                option.UseSqlite("DataSource=" + path);
                option.UseOracle(option =>
                {
                    option.DbKey = "Oracle";
                    option.MasterConnectionString = "User ID=IFSAPP;Password=IFSAPP;Data Source=RACE;";
                });
                option.SetTableContext(TableContext);
                option.SetWatcher(aop =>
                {
                    aop.DbLog = (sql, p) =>
                    {
                        Console.WriteLine(sql);
                        Console.WriteLine();
                    };
                });//.InitializedContext<TestInitContext>();
            });
            Db = ExpSqlFactory.GetContext();
            ResolveCtx = ResolveContext.Create(DbBaseType.Oracle);
        }
    }
}
