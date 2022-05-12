﻿using LightORM.Test.Models;
using MDbContext;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LightORM.Test
{
    internal class EnumConditionTest
    {
        [SetUp]
        public void Setup()
        {

        }
        private DbContext VbDbContext()
        {
            DbContext.Init(DbBaseType.Oracle);
            var conn = new OracleConnection("Data Source=192.168.56.11:1521/ORCL;Persist Security Info=True;User ID=CGS;Password=CGS2020");
            return conn.DbContext();
        }

        [Test]
        public void EnumCondition()
        {
            var pExp = Expression.Parameter(typeof(CgsUsers), "p");
            var propExp = Expression.Property(pExp, "UsrEnable");
            var enumValueExp = Expression.Constant((int)YesOrNo.Yes, typeof(int));
            var right = Expression.Convert(enumValueExp, typeof(YesOrNo));
            var body = Expression.Equal(propExp, right);
            var whereExp = Expression.Lambda<Func<CgsUsers, bool>>(body, pExp);
            var db = VbDbContext();
            db.DbSet.Select<CgsUsers>()
                //.Where(u=>u.UsrEnable == YesOrNo.Yes);
                .Where(whereExp);
            Console.WriteLine(db.DbSet);
        }
    }
}
