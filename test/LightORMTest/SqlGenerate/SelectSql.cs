namespace LightORMTest.SqlGenerate;

public partial class SelectSql : TestBase
{
    #region select

    [TestMethod]
    public virtual void Select_One_Table()
    {
        var sql = Db.Select<Product>()
            .Where(p => p.ModifyTime > DateTime.Now)
            .ToSql(p => new { p.ProductId, p.ProductName });
        Console.WriteLine(sql);
        AssertSqlResult(nameof(Select_One_Table), sql);
    }

    [TestMethod]
    public void Select_One_Table_And_Join_Two_Table()
    {
        var sql = Db.Select<User>()
            .InnerJoin<UserRole>(w => w.Tb1.UserId == w.Tb2.UserId)
            .LeftJoin<Role>(w => w.Tb2.RoleId == w.Tb3.RoleId)
            .Where(u => u.UserId == "admin")
            .ToSql(w => w.Tb1);
        Console.WriteLine(sql);
        AssertSqlResult(nameof(Select_One_Table_And_Join_Two_Table), sql);
    }

    [TestMethod]
    public void Select_Three_Table()
    {
        var sql = Db.Select<Permission, RolePermission, Role>()
            .Distinct()
            .Where(w => w.Tb1.PermissionId == w.Tb2.PermissionId && w.Tb2.RoleId == w.Tb3.RoleId)
            .ToSql(w => new { w.Tb1 });
        Console.WriteLine(sql);
        AssertSqlResult(nameof(Select_Three_Table), sql);
    }

    class Jobs
    {
        public string? Plate { get; set; }
        public string? StnId { get; set; }
    }


    [TestMethod]
    public void Select_AsTable()
    {
        var sql = Db.Select<User>()
            .SelectColumns(u => new { u.UserId, u.Password })
            .ToSql();
        Console.WriteLine(sql);
        AssertSqlResult(nameof(Select_AsTable), sql);
    }

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
        AssertSqlResult(nameof(Select_Join_GroupBy_Having), sql);

    }

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
        AssertSqlResult(nameof(Select_GroupBy_SubQuery_Join),sql);
    }
    
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
        AssertSqlResult(nameof(Select_JoinGroupSelect), sql);
    }
    
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
    }

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
    }

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
    }


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
    }

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
    }

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

        var sql2 = Db.Select<User>()
            // 两种写法都可以
            .Where(u => u.UserRoles.Where(r => r.RoleId.Contains("admin")).Any())
            .ToSql();
        Assert.IsTrue(sql == sql2);
        //var result = """
        //    SELECT DISTINCT *
        //    FROM `USER` `a`
        //    INNER JOIN `USER_ROLE` `b` ON ( `a`.`USER_ID` = `b`.`USER_ID` )
        //    INNER JOIN `ROLE` `c` ON ( `c`.`ROLE_ID` = `b`.`ROLE_ID` )
        //    WHERE `c`.`ROLE_ID` LIKE '%'||'admin'||'%'
        //    """;
        //Assert.IsTrue(SqlNormalizer.AreSqlEqual(result, sql));
    }

    [TestMethod]
    public void Select_Navigate_Where_Single()
    {
        var sql = Db.Select<User>()
            // 两种写法都可以
            //.Where(u => u.UserRoles.Where(r => r.RoleId.Contains("admin")).Any())
            .Where(u => u.City.Name.StartsWith("D"))
            .ToSql();
        Console.WriteLine(sql);
    }

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
    }

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
            .Having(g => g.Count() > 2 && g.Average(g.Tables.DateDiff) < 1).AsTemp("temp", g =>
                new
                {
                    g.Group.Id,
                    AvgDiff = g.Average(g.Tables.DateDiff)
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
    }

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
            .Having(g => g.Count() > 2 && g.Average(g.Tables.DateDiff) < 1).AsTable(g =>
                new
                {
                    g.Group.Id,
                    AvgDiff = g.Average(g.Tables.DateDiff)
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
    }

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
    }


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
    }


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
    }


    #endregion

    [TestMethod]
    public void Select_Temp_ThenGroup_And_Union()
    {
        // 从Jobs表中，选择Plate的第一个字符作为Fzjg字段，选择Fzjg和StnId，作为temp表，并命名为info
        // With info as (...)
        var info = Db.Select<Jobs>().AsTemp("info", j => new
        {
            Fzjg = j.Plate!.Substring(1, 2),
            j.StnId
        });
        // 从info表中，按StnId和Fzjg分组并且按Count(*)排序后，选择StnId，Fzjg，Count(*)，RowNumer，作为temp表，并命名为stn_fzjg，表数据为每个StnId中，按Fzjg数据量进行排序并标记为Index
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
        // 从info表中，按Fzjg分组并且按Count(*)排序后，选择StnId，Fzjg，Count(*)，RowNumer，作为temp表，并命名为all_fzjg，表数据为所有Fzjg中，按每个Fzjg的数据量进行排序并标记为Index
        var allFzjg = Db.FromTemp(info).GroupBy(a => new { a.Fzjg })
            .OrderByDesc(a => a.Count())
            .AsTemp("all_fzjg", g => new
            {
                StnId = "合计",
                g.Group.Fzjg,
                Count = g.Count(),
                Index = WinFn.RowNumber().OrderByDesc(g.Count()).Value()
            });
        // 从info表中，按StnId进行Group By Rollup ，选择StnId和分组数据量，作为temp表，并命名为all_station
        var allStation = Db.FromTemp(info).GroupBy(t => new { t.StnId })
            .Rollup()
            .AsTemp("all_station", g => new
            {
                StnId = SqlFn.NullThen(g.Group.StnId, "合计"),
                Total = SqlFn.Count()
            });
        /*
         * 1. 从stn_fzjg中，筛选出所有前3的Fzjg数量，然后按StnId分组，选择StnId，组内第一Fzjg作为FirstFzjg，组内第一的Count作为FirstCount
         * 2. 从all_fzjg中，筛选出所有前3的Fzjg数量，选择'合计'作为StnId，第一Fzjg作为FirstFzjg，第一的Count作为FirstCount
         * 3. 将1和2的结果Union ALl
         * 4. 转为子查询，inner join all_station
         * 5. select结果列
         */
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
    }

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

    [TestMethod]
    public void TestGroupBy()
    {
        var sql = Db.Select<User>()
            .InnerJoin<Product>((u, p) => u.UserId == p.ProductCode)
            .GroupBy(u => new { u.Tb1.Age })
            .ToSql(g => new
            {
                g.Group.Age,
                Count = g.Count()
            });
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void TestPaging()
    {
        var skip = 10;
        var take = 10;
        var sql = Db.Select<User>()
            .InnerJoin<Product>((u, p) => u.UserId == p.ProductCode)
            .Skip(skip)
            .Take(take)
            .ToSql();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void TestRowNumberWindowFunc()
    {
        var sql = Db.Select<User>()
            .ToSql(u => new
            {
                RowNo = WinFn.RowNumber().PartitionBy(u.City.Name).OrderBy(u.Id).Value(),
                u.UserId,
                u.UserName,
                u.Password
            });
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void SelectWithOverriddenTableName()
    {
        var sql = Db.Select<User>("OLD_USER")
            .Where(u => u.UserName == "admin")
            .ToSql();
        Console.WriteLine(sql);
    }

    [TestMethod]
    public void SelectWithJoinOverriddenTableName()
    {
        var sql = Db.Select<User>()
            .InnerJoin<Product>("OLD_PRODUCT", (u, p) => u.UserId == p.ProductCode)
            .ToSql();
        Console.WriteLine(sql);
    }
}