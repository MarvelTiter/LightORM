using LightORM.Interfaces;
using LightORM.Interfaces.ExpSql;
using LightORM.Providers.Sqlite.Extensions;
using LightORM.Utils.Vistors;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LightORMTest.ExpressionTest;

[TestClass]
public class TypeSetFlatTest : TestBase
{
    public override DbBaseType DbType => DbBaseType.Sqlite;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite("1");
    }

    [TestMethod]
    public void Flat()
    {
        Expression<Func<TypeSet<User, Permission>, bool>> exp = w => w.Tb1.Age > 10;

        var n = FlatTypeSet.Default.Flat(exp);
        var nn = n.Resolve(SqlResolveOptions.Select, ResolveCtx);
        Console.WriteLine(n);
        Console.WriteLine(nn.SqlString);
    }

    [TestMethod]
    public void FlatNew()
    {
        Expression<Func<TypeSet<User, Permission>, object>> exp = w => new { w.Tb1.UserName, w.Tb2.Path };
        var n = FlatTypeSet.Default.Flat(exp);
        var nn = n.Resolve(SqlResolveOptions.Select, ResolveCtx);
        Console.WriteLine(n);
        Console.WriteLine(nn.SqlString);
    }
}

[TestClass]
public class FlatGroupTest:TestBase
{
    public override DbBaseType DbType => DbBaseType.Sqlite;

    protected override void Configura(IExpressionContextSetup option)
    {
        option.UseSqlite("1");
    }

    class IGr
    {
        public int? Age { get; set; }
        public string? RoleId { get; set; }
    }

    [TestMethod]
    public void FlatGroup()
    {
        Expression<Func<IExpSelectGrouping<IGr, TypeSet<User, UserRole>>, object>>? exp = w => new
        {
            w.Group.Age,
            w.Group.RoleId,
            R = w.Tables.Tb1.UserName,
            T = w.Tables.Tb2.RoleId
        };
        // (user, userrole, w) => new { w.Group, Total = w.Count(), Pass = w.Count(user.Age)  }
        var n = FlatTypeSet.Default.Flat(exp);
        var nn = n.Resolve(SqlResolveOptions.Select, ResolveCtx);
        Console.WriteLine(exp);
        Console.WriteLine(nn.SqlString);
    }

    [TestMethod]
    public void FlatGroup2()
    {
        Expression<Func<IExpSelectGrouping<IGr, User>, object>>? exp = w => new
        {
            w.Group.Age,
            w.Group.RoleId,
            R = w.Tables.UserName,
        };
        // (user, userrole, w) => new { w.Group, Total = w.Count(), Pass = w.Count(user.Age)  }
        var n = FlatTypeSet.Default.Flat(exp);
        var nn = n.Resolve(SqlResolveOptions.Select, ResolveCtx);
        Console.WriteLine(n);
        Console.WriteLine(nn.SqlString);
    }
}