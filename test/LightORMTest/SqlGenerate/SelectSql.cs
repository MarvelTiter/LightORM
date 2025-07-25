namespace LightORMTest.SqlGenerate;

public class SelectSql : TestBase
{
    #region select

    [TestMethod]
    public void Select_One_Table()
    {
        var sql = Db.Select<Product>()
            .Where(p => p.ModifyTime > DateTime.Now)
            .ToSql(p => new { p.ProductId, p.ProductName });
        Console.WriteLine(sql);
        //var result = $"""
        //        SELECT `a`.`ProductId`, `a`.`ProductName`
        //        FROM `Product` `a`
        //        WHERE (`a`.`ModifyTime` > @Now_0)
        //        """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_One_Table_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }

        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }

    public virtual string? Select_One_Table_Result() => null;

    [TestMethod]
    public void Select_One_Table_And_Join_Two_Table()
    {
        var sql = Db.Select<User>()
            .InnerJoin<UserRole>(w => w.Tb1.UserId == w.Tb2.UserId)
            .InnerJoin<Role>(w => w.Tb2.RoleId == w.Tb3.RoleId)
            .Where(u => u.UserId == "admin")
            .ToSql(w => w.Tb1);
        Console.WriteLine(sql);
        //var result = """
        //        SELECT `a`.*
        //        FROM `USER` `a`
        //        INNER JOIN `USER_ROLE` `b` ON (`a`.`USER_ID` = `b`.`USER_ID`)
        //        INNER JOIN `ROLE` `c` ON (`b`.`ROLE_ID` = `c`.`ROLE_ID`)
        //        WHERE (`a`.`USER_ID` = 'admin')
        //        """;
        var result = Select_One_Table_And_Join_Two_Table_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }

        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }

    public virtual string? Select_One_Table_And_Join_Two_Table_Result() => null;

    [TestMethod]
    public void Select_Three_Table()
    {
        var sql = Db.Select<Permission, RolePermission, Role>()
            .Distinct()
            .Where(w => w.Tb1.PermissionId == w.Tb2.PermissionId && w.Tb2.RoleId == w.Tb3.RoleId)
            .ToSql(w => new { w.Tb1 });
        Console.WriteLine(sql);
        //var result = """
        //        SELECT DISTINCT `a`.*
        //        FROM `POWERS` `a`, `ROLE_POWER` `b`, `ROLE` `c`
        //        WHERE ((`a`.`POWER_ID` = `b`.`POWER_ID`) AND (`b`.`ROLE_ID` = `c`.`ROLE_ID`))
        //        """;
        var result = Select_Three_Table_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }

    public virtual string? Select_Three_Table_Result() => null;

    class Jobs
    {
        public string? Plate { get; set; }
        public string? StnId { get; set; }
    }

    [TestMethod]
    public void Select_Temp_ThenGroup_And_Union()
    {
        var info = Db.Select<Jobs>().AsTemp("info", j => new
        {
            Fzjg = j.Plate!.Substring(1, 2),
            j.StnId
        });
        var stnFzjg = Db.FromTemp(info)
            .GroupBy(a => new { a.StnId, a.Fzjg })
            .OrderByDesc(a => new { a.Group.StnId, i = a.Count() })
            .AsTemp("stn_fzjg", g => new
            {
                g.Group.StnId,
                g.Group.Fzjg,
                Count = g.Count(),
                Index = WinFn.RowNumber().PartitionBy(g.Tables.StnId).OrderByDesc(g.Count()).Value()
            });
        var allFzjg = Db.FromTemp(info).GroupBy(a => new { a.Fzjg })
            .OrderByDesc(a => a.Count())
            .AsTemp("all_fzjg", g => new
            {
                StnId = "合计",
                g.Group.Fzjg,
                Count = g.Count(),
                Index = WinFn.RowNumber().OrderByDesc(g.Count()).Value()
            });
        var allStation = Db.FromTemp(info).GroupBy(t => new { t.StnId })
            .Rollup()
            .AsTemp("all_station", g => new
            {
                StnId = SqlFn.NullThen(g.Group.StnId, "合计"),
                Total = SqlFn.Count()
            });
        var sql = Db.FromTemp(stnFzjg).Where(t => t.Index < 4)
            .GroupBy(t => new { t.StnId })
            .AsTable(g => new
            {
                StnId = g.Group.StnId!,
                FirstFzjg = g.Join(g.Tables.Index == 1 ? g.Tables.Fzjg.ToString() : "").Separator("").OrderBy(g.Tables.StnId).Value(),
                FirstCount = g.Join(g.Tables.Index == 1 ? g.Tables.Count.ToString() : "").Separator("").OrderBy(g.Tables.StnId).Value()
            }).UnionAll(Db.FromTemp(allFzjg).Where(t => t.Index < 4).AsTable(g => new
            {
                StnId = "合计",
                FirstFzjg = SqlFn.Join(g.Index == 1 ? g.Fzjg.ToString() : "").Separator("").OrderBy(g.StnId).Value(),
                FirstCount = SqlFn.Join(g.Index == 1 ? g.Count.ToString() : "").Separator("").OrderBy(g.StnId).Value()
            })).AsSubQuery()
            .InnerJoin(allStation, (t, a) => t.StnId == a.StnId)
            .ToSql((t, a) => new
            {
                Jczmc = SqlFn.NullThen(t.StnId, "TT"),
                a.Total,
                t
            });
        Console.WriteLine(sql);
        //var result = """
        //    WITH info AS (
        //        SELECT SUBSTR(`a`.`Plate`,1,2) AS `Fzjg`, `a`.`StnId`
        //        FROM `Jobs` `a`
        //    )
        //    , stn_fzjg AS (
        //        SELECT `a`.`StnId`, `a`.`Fzjg`, COUNT(*) AS `Count`, ROW_NUMBER() OVER( PARTITION BY `a`.`StnId` ORDER BY COUNT(*) DESC ) AS `Index`
        //        FROM info `a`
        //        GROUP BY `a`.`StnId`, `a`.`Fzjg`
        //        ORDER BY `a`.`StnId`, COUNT(*) DESC
        //    )
        //    , all_fzjg AS (
        //        SELECT '合计' AS `StnId`, `a`.`Fzjg`, COUNT(*) AS `Count`, ROW_NUMBER() OVER( ORDER BY COUNT(*) DESC ) AS `Index`
        //        FROM info `a`
        //        GROUP BY `a`.`Fzjg`
        //        ORDER BY COUNT(*) DESC
        //    )
        //    , all_station AS (
        //        SELECT IFNULL(`a`.`StnId`,'合计') AS `StnId`, COUNT(*) AS `Total`
        //        FROM info `a`
        //        GROUP BY ROLLUP(`StnId`)
        //    )
        //    SELECT IFNULL(`a`.`StnId`,'TT') AS `Jczmc`, `b`.`Total`, `a`.*
        //    FROM (
        //        SELECT `a`.`StnId`, GROUP_CONCAT( CASE WHEN (`a`.`Index` = 1) THEN CAST(`a`.`Fzjg` AS TEXT) ELSE '' END,'') AS `FirstFzjg`, GROUP_CONCAT( CASE WHEN (`a`.`Index` = 1) THEN CAST(`a`.`Count` AS TEXT) ELSE '' END,'') AS `FirstCount`
        //        FROM stn_fzjg `a`
        //        WHERE (`a`.`Index` < 4)
        //        GROUP BY `a`.`StnId`
        //        UNION ALL
        //        SELECT '合计' AS `StnId`, GROUP_CONCAT( CASE WHEN (`a`.`Index` = 1) THEN CAST(`a`.`Fzjg` AS TEXT) ELSE '' END,'') AS `FirstFzjg`, GROUP_CONCAT( CASE WHEN (`a`.`Index` = 1) THEN CAST(`a`.`Count` AS TEXT) ELSE '' END,'') AS `FirstCount`
        //        FROM all_fzjg `a`
        //        WHERE (`a`.`Index` < 4)
        //    ) `a`
        //    INNER JOIN all_station `b` ON (`a`.`StnId` = `b`.`StnId`)
        //    """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_Temp_ThenGroup_And_Union_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_Temp_ThenGroup_And_Union_Result() => null;

    [TestMethod]
    public void Select_AsTable()
    {
        var sql = Db.Select<User>()
            .SelectColumns(u => new { u.UserId, u.Password })
            .ToSql();
        var result = Select_AsTable_Result();
        Console.WriteLine(sql);
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_AsTable_Result() => null;
    #endregion

    #region group

    [TestMethod]
    public void Select_Join_GroupBy_Having()
    {
        var sql = Db.Select<User>()
            .InnerJoin<UserRole>(w => w.Tb1.UserId == w.Tb2.UserId)
            .GroupBy(w => new { w.Tb1.UserId, w.Tb2.RoleId })
            .Having(w => w.Count() > 10 && w.Max(w.Tables.Tb1.Age) > 18)
            .OrderBy(w => w.Group.UserId)
            .ToSql(w => new
            {
                w.Group.UserId,
                w.Group.RoleId,
                Total = w.Count(),
                Pass = w.Count<int?>(w.Tables.Tb1.Age > 10 ? 1 : null),
                NoPass = w.Max(w.Tables.Tb1.Age > 10, w.Tables.Tb1.UserName)
            });
        Console.WriteLine(sql);
        //var result = """
        //    SELECT `a`.`USER_ID` AS `UserId`, `b`.`ROLE_ID` AS `RoleId`, COUNT(*) AS `Total`, COUNT(CASE WHEN (`a`.`AGE` > 10) THEN 1 ELSE NULL END) AS `Pass`, MAX(CASE WHEN (`a`.`AGE` > 10) THEN `a`.`USER_NAME` ELSE 0 END) AS `NoPass`
        //    FROM `USER` `a`
        //    INNER JOIN `USER_ROLE` `b` ON (`a`.`USER_ID` = `b`.`USER_ID`)
        //    GROUP BY `a`.`USER_ID`, `b`.`ROLE_ID`
        //    HAVING COUNT(*) > 10 AND MAX(`a`.`AGE`) > 18
        //    ORDER BY `a`.`USER_ID` ASC
        //    """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_Join_GroupBy_Having_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }

    public virtual string? Select_Join_GroupBy_Having_Result() => null;

    [TestMethod]
    public void Select_GroupBy_SubQuery_Join()
    {
        var sql = Db.Select<User>()
            .GroupBy(a => new { a.UserId })
            .AsTable(g => new
            {
                g.Group.UserId,
                Total = g.Count(),
                Tb = g.Count<int?>(g.Tables.Age > 18 ? 1 : null)
            }).AsSubQuery()
            .InnerJoin<Permission>((a, s) => a.UserId == s.PermissionId)
            .ToSql((a, s) => new
            {
                Jyjgbh = a.UserId,
                a.Total,
                a.Tb,
                Jczmc = s.PermissionName
            });
        Console.WriteLine(sql);
        //var result = """
        //    SELECT `a`.`UserId`, `a`.`Total`, `a`.`Tb`, `b`.`POWER_NAME` AS `Jczmc`
        //    FROM (
        //        SELECT `a`.`USER_ID` AS `UserId`, COUNT(*) AS `Total`, COUNT(CASE WHEN (`a`.`AGE` > 18) THEN 1 ELSE NULL END) AS `Tb`
        //        FROM `USER` `a`
        //        GROUP BY `a`.`USER_ID`
        //    ) `a`
        //    INNER JOIN `POWERS` `b` ON (`a`.`UserId` = `b`.`POWER_ID`)
        //    """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_GroupBy_SubQuery_Join_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_GroupBy_SubQuery_Join_Result() => null;
    #endregion

    #region sub select

    [TestMethod]
    public void Select_JoinGroupSelect()
    {
        var sql = Db.Select<User>()
            .LeftJoin(Db.Select<Product>().GroupBy(p => new { p.ProductId }).AsTable(g => new
            {
                g.Group.ProductId,
                Total = g.Count()
            }), (u, j) => u.Age == j.ProductId)
            .Where(w => w.Tb2.Total > 10)
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //        SELECT *
        //        FROM `USER` `a`
        //        LEFT JOIN (
        //            SELECT `a`.`ProductId`, COUNT(*) AS `Total`
        //            FROM `Product` `a`
        //            GROUP BY `a`.`ProductId`
        //        ) `b` ON (`a`.`AGE` = `b`.`ProductId`)
        //        WHERE (`b`.`Total` > 10)
        //        """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_JoinGroupSelect_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_JoinGroupSelect_Result() => null;

    [TestMethod]
    public void Select_GroupBy_AsTable_SubQuery()
    {
        var sql = Db.Select<User>()
            .Where(u => u.Age > 10)
            .GroupBy(u => new { u.UserId })
            .AsTable(u => new
            {
                u.Group.UserId,
                Total = u.Count()
            }).AsSubQuery()
            .Where(t => t.UserId.Contains("admin"))
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //        SELECT *
        //        FROM (
        //            SELECT `a`.`USER_ID` AS `UserId`, COUNT(*) AS `Total`
        //            FROM `USER` `a`
        //            WHERE (`a`.`AGE` > 10)
        //            GROUP BY `a`.`USER_ID`
        //        ) `a`
        //        WHERE `a`.`UserId` LIKE '%'||'admin'||'%'
        //        """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_GroupBy_AsTable_SubQuery_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_GroupBy_AsTable_SubQuery_Result() => null;

    [TestMethod]
    public void Select_Join_SelectJoin()
    {
        var temp = Db.Select<User>()
            .InnerJoin<UserRole>((u, ur) => u.UserId == ur.UserId)
            .InnerJoin<Role>(w => w.Tb2.RoleId == w.Tb3.RoleId)
            .AsTable(w => new
            {
                w.Tb1.UserId,
                w.Tb1.UserName,
                w.Tb3.RoleName,
            });
        var sql = Db.Select<User>()
            .LeftJoin<Permission>(w => w.Tb1.UserId == w.Tb2.PermissionId)
            .LeftJoin(temp, (u, _, t) => u.UserId == t.UserId)
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //        SELECT *
        //        FROM `USER` `a`
        //        LEFT JOIN `POWERS` `b` ON (`a`.`USER_ID` = `b`.`POWER_ID`)
        //        LEFT JOIN (
        //            SELECT `a`.`USER_ID` AS `UserId`, `a`.`USER_NAME` AS `UserName`, `c`.`ROLE_NAME` AS `RoleName`
        //            FROM `USER` `a`
        //            INNER JOIN `USER_ROLE` `b` ON (`a`.`USER_ID` = `b`.`USER_ID`)
        //            INNER JOIN `ROLE` `c` ON (`b`.`ROLE_ID` = `c`.`ROLE_ID`)
        //        ) `c` ON (`a`.`USER_ID` = `c`.`UserId`)
        //        """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_Join_SelectJoin_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_Join_SelectJoin_Result() => null;

    [TestMethod]
    public void Select_SubSelectAsWhere()
    {
        var sql = Db.Select<User>()
            .Where(u => u.Age > Db.Select<UserRole>().Where(ur => ur.RoleId.Contains("admin")).SelectColumns(ur => ur.RoleId).Result<int>())
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //        SELECT *
        //        FROM `USER` `a`
        //        WHERE (`a`.`AGE` > (
        //            SELECT `a`.`ROLE_ID`
        //            FROM `USER_ROLE` `a`
        //            WHERE `a`.`ROLE_ID` LIKE '%'||'admin'||'%'
        //        ))
        //        """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));

        sql = Db.Select<User>()
            .Where(u => u.Age > Db.Select<User>().Max(u => u.Age))
            .ToSql();
        Console.WriteLine(sql);
        var result = Select_SubSelectAsWhere_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_SubSelectAsWhere_Result() => null;

    [TestMethod]
    public void Select_WhereExits()
    {
        var sql = Db.Select<User>().Where(u => Db.Select<UserRole>().Where(ur => ur.RoleId.Contains("admin")).Exits()).ToSql();
        Console.WriteLine(sql);
        //var result = """
        //        SELECT *
        //        FROM `USER` `a`
        //        WHERE EXISTS (
        //            SELECT 1
        //            FROM `USER_ROLE` `b`
        //            WHERE `b`.`ROLE_ID` LIKE '%'||'admin'||'%'
        //        )
        //        """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_WhereExits_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_WhereExits_Result() => null;

    [TestMethod]
    public void Select_SubSelectAsField()
    {
        var sql = Db.Select<User>().ToSql(u => new
        {
            u.UserName,
            Age = Db.Select<User>().Count(u => u.UserId)
        });
        Console.WriteLine(sql);
        //var result = """
        //    SELECT `a`.`USER_NAME` AS `UserName`, (
        //        SELECT COUNT(`a`.`USER_ID`)
        //        FROM `USER` `a`
        //    ) AS `Age`
        //    FROM `USER` `a`
        //    """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_SubSelectAsField_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_SubSelectAsField_Result() => null;

    #endregion

    #region select include

    [TestMethod]
    public void Select_Navigate_Where_Multi()
    {
        var sql = Db.Select<User>()
            // 两种写法都可以
            //.Where(u => u.UserRoles.Where(r => r.RoleId.Contains("admin")).Any())
            .Where(u => u.UserRoles.WhereIf(r => r.RoleId.Contains("admin")))
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //    SELECT DISTINCT *
        //    FROM `USER` `a`
        //    INNER JOIN `USER_ROLE` `b` ON ( `a`.`USER_ID` = `b`.`USER_ID` )
        //    INNER JOIN `ROLE` `c` ON ( `c`.`ROLE_ID` = `b`.`ROLE_ID` )
        //    WHERE `c`.`ROLE_ID` LIKE '%'||'admin'||'%'
        //    """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));

        var result = Select_Navigate_Where_Multi_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_Navigate_Where_Multi_Result() => null;

    [TestMethod]
    public void Select_Navigate_Where_Single()
    {
        var sql = Db.Select<User>()
             // 两种写法都可以
             //.Where(u => u.UserRoles.Where(r => r.RoleId.Contains("admin")).Any())
             .Where(u => u.City.Name.StartsWith("D"))
             .ToSql();
        Console.WriteLine(sql);
        var result = Select_Navigate_Where_Single_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_Navigate_Where_Single_Result() => null;

    #endregion

    #region temp sql

    [TestMethod]
    public void Select_Temp()
    {
        var tempU = Db.Select<User>().GroupBy(u => new { u.UserId }).AsTemp("us", g => new
        {
            g.Group.UserId,
            Total = g.Count()
        });
        var tempR = Db.Select<Role>().WithTempQuery(tempU)
            .Where((r, u) => r.RoleId == u.UserId)
            .Where(w => w.Tb2.UserId.StartsWith("ad"))
            .AsTemp("temp", w => new
            {
                w.Tb1.RoleId,
                w.Tb2.UserId,
            });

        var tempT = Db.FromTemp(tempR).Where(a => a.RoleId.StartsWith("h")).AsTemp("hh");

        var sql = Db.Select<Permission>().WithTempQuery(tempT)
            .Where(w => w.Tb2.UserId.Contains("admin"))
            .ToSql();

        Console.WriteLine(sql);
        //var result = """
        //        WITH us AS (
        //            SELECT `a`.`USER_ID` AS `UserId`, COUNT(*) AS `Total`
        //            FROM `USER` `a`
        //            GROUP BY `a`.`USER_ID`
        //        )
        //        , temp AS (
        //            SELECT `a`.`ROLE_ID` AS `RoleId`, `b`.`UserId`
        //            FROM `ROLE` `a`, us `b`
        //            WHERE (`a`.`ROLE_ID` = `b`.`UserId`) AND `b`.`UserId` LIKE 'ad'||'%'
        //        )
        //        , hh AS (
        //            SELECT *
        //            FROM temp `a`
        //            WHERE `a`.`RoleId` LIKE 'h'||'%'
        //        )
        //        SELECT *
        //        FROM `POWERS` `a`, hh `b`
        //        WHERE `b`.`UserId` LIKE '%'||'admin'||'%'
        //        """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_Temp_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_Temp_Result() => null;

    [TestMethod]
    public void Select_GroupBy_AsSub_GroupBy_AsTemp_SelectJoinTemp()
    {
        var dt = DateTime.Now;
        var temp = Db.Select<User>().Where(u => u.LastLogin > dt).AsTable(u => new
        {
            Id = u.UserId,
            DateDiff = (u.LastLogin - WinFn.Lag(u.LastLogin).PartitionBy(u.UserId).OrderBy(u.LastLogin).Value()) * 24
        }).AsSubQuery()
            .Where(a => a.DateDiff != null)
            .GroupBy(a => new { a.Id })
            .Having(g => g.Count() > 2 && g.Avg(g.Tables.DateDiff) < 1).AsTemp("temp", g =>
                new
                {
                    g.Group.Id,
                    AvgDiff = g.Avg(g.Tables.DateDiff)
                });
        var sql = Db.Select<User>().InnerJoin(temp, (u, t) => u.UserId == t.Id).ToSqlWithParameters();
        Console.WriteLine(sql);
        //var result = """
        //        WITH temp AS (
        //            SELECT `a`.`Id`, AVG(`a`.`DateDiff`) AS `AvgDiff`
        //            FROM (
        //                SELECT `a`.`USER_ID` AS `Id`, ((`a`.`LAST_LOGIN` - LAG(`a`.`LAST_LOGIN`) OVER( PARTITION BY `a`.`USER_ID` ORDER BY `a`.`LAST_LOGIN` ASC )) * 24) AS `DateDiff`
        //                FROM `USER` `a`
        //                WHERE (`a`.`LAST_LOGIN` > @s_dt_0)
        //            ) `a`
        //            WHERE (`a`.`DateDiff` IS NOT NULL)
        //            GROUP BY `a`.`Id`
        //            HAVING COUNT(*) > 2 AND AVG(`a`.`DateDiff`) < 1
        //        )
        //        SELECT *
        //        FROM `USER` `a`
        //        INNER JOIN temp `b` ON (`a`.`USER_ID` = `b`.`Id`)
        //        """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_GroupBy_AsSub_GroupBy_AsTemp_SelectJoinTemp_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_GroupBy_AsSub_GroupBy_AsTemp_SelectJoinTemp_Result() => null;

    [TestMethod]
    public void Select_GroupBy_AsSub_GroupBy_SelectJoinSub()
    {
        var dt = DateTime.Now;
        var temp = Db.Select<User>().Where(u => u.LastLogin > dt).AsTable(u => new
        {
            Id = u.UserId,
            DateDiff = (u.LastLogin - WinFn.Lag(u.LastLogin).PartitionBy(u.UserId).OrderBy(u.LastLogin).Value()) * 24
        }).AsSubQuery()
            .Where(a => a.DateDiff != null)
            .GroupBy(a => new { a.Id })
            .Having(g => g.Count() > 2 && g.Avg(g.Tables.DateDiff) < 1).AsTable(g =>
                new
                {
                    g.Group.Id,
                    AvgDiff = g.Avg(g.Tables.DateDiff)
                });
        var sql = Db.Select<User>().InnerJoin(temp, (u, t) => u.UserId == t.Id).ToSqlWithParameters();
        Console.WriteLine(sql);
        //var result = """
        //        SELECT *
        //        FROM `USER` `a`
        //        INNER JOIN (
        //            SELECT `a`.`Id`, AVG(`a`.`DateDiff`) AS `AvgDiff`
        //            FROM (
        //                SELECT `a`.`USER_ID` AS `Id`, ((`a`.`LAST_LOGIN` - LAG(`a`.`LAST_LOGIN`) OVER( PARTITION BY `a`.`USER_ID` ORDER BY `a`.`LAST_LOGIN` ASC )) * 24) AS `DateDiff`
        //                FROM `USER` `a`
        //                WHERE (`a`.`LAST_LOGIN` > @s_dt_0)
        //            ) `a`
        //            WHERE (`a`.`DateDiff` IS NOT NULL)
        //            GROUP BY `a`.`Id`
        //            HAVING COUNT(*) > 2 AND AVG(`a`.`DateDiff`) < 1
        //        ) `b` ON (`a`.`USER_ID` = `b`.`Id`)
        //        """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_GroupBy_AsSub_GroupBy_SelectJoinSub_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_GroupBy_AsSub_GroupBy_SelectJoinSub_Result() => null;

    #endregion

    #region union select

    [TestMethod]
    public void Select_Union()
    {
        var sql = Db.Select<User>()
            .Where(u => u.Age > 10)
            .Union(Db.Select<User>().Where(u => u.Age > 15))
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //        SELECT *
        //        FROM `USER` `a`
        //        WHERE (`a`.`AGE` > 10)
        //        UNION
        //        SELECT *
        //        FROM `USER` `a`
        //        WHERE (`a`.`AGE` > 15)
        //        """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_Union_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_Union_Result() => null;

    [TestMethod]
    public void Select_Context_Union()
    {
        var sql = Db.Union(Db.Select<User>(), Db.Select<User>())
            .Where(u => u.Age > 10)
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //        SELECT *
        //        FROM (
        //            SELECT *
        //            FROM `USER` `a`
        //            UNION
        //            SELECT *
        //            FROM `USER` `a`
        //        ) `a`
        //        WHERE (`a`.`AGE` > 10)
        //        """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_Context_Union_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_Context_Union_Result() => null;

    #endregion

    #region flat prop

    [TestMethod]
    public void Select_Flat_Property()
    {
        var sql = Db.Select<UserFlat>()
            .Where(l => l.PriInfo.IsLock == true)
            .ToSql();
        Console.WriteLine(sql);
        //var result = """
        //    SELECT *
        //    FROM `SMS_LOG` `a`
        //    WHERE (`a`.`CODE` = 100)
        //    """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
        var result = Select_Flat_Property_Result();
        if (string.IsNullOrEmpty(result))
        {
            return;
        }
        Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }
    public virtual string? Select_Flat_Property_Result() => null;

    #endregion


    [TestMethod]
    public void TestNullValue()
    {
        int? age = null;
        var sql = Db.Select<User>()
            .Where(u => u.Age != age)
            .ToSql();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void TestBooleanValue()
    {
        bool islock = false;
        var sql = Db.Select<User>()
            .Where(u => u.IsLock == islock)
            .ToSql();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void TestArrayAccess()
    {
        int[] arr = [10];
        var sql = Db.Select<User>()
            .Where(u => u.Age == arr[0])
            .ToSql();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void TestArrayContain()
    {
        int?[] arr = [10, 11];
        var select = Db.Select<User>()
            .Where(u => arr.Contains(u.Age));
        var sql = select.ToSql();
        var ps = select.SqlBuilder.DbParameters;
        Console.WriteLine(sql);
    }
}