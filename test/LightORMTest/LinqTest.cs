using LightORM.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightORMTest;

public class LinqTest : TestBase
{
    [TestMethod]
    public void LinqWhere()
    {
        var repository = new DefaultRepository<User>(Db);
        var list = repository.Table.Where(u => u.Age > 10).ToList();
        Console.WriteLine(list.Count);
    }
    [TestMethod]
    public void LinqWhereAndOrderBy()
    {
        var repository = new DefaultRepository<User>(Db);
        var list = from u in repository.Table
                   where u.Age > 10
                   orderby u.UserId
                   select u;
        foreach (var item in list)
        {
            Console.WriteLine($"{item.UserId} {item.Age}");
        }
    }
    [TestMethod]
    public void LinqJoin()
    {
        var usrRepo = new DefaultRepository<User>(Db);
        var proRepo = new DefaultRepository<Product>(Db);

        var list = from u in usrRepo.Table
                   join p in proRepo.Table on u.UserId equals p.ProductCode
                   orderby u.Id
                   select u;
        foreach (var item in list)
        {
            Console.WriteLine($"{item.UserId} {item.Age}");
        }
    }
    [TestMethod]
    public void LinqSelect()
    {
        var usrRepo = new DefaultRepository<User>(Db);
        var proRepo = new DefaultRepository<Product>(Db);

        var list = from u in usrRepo.Table
                   join p in proRepo.Table on u.UserId equals p.ProductCode
                   orderby u.Id
                   select new { p.ProductName, u.UserId, u.Age };
        foreach (var item in list)
        {
            Console.WriteLine($"{item.UserId} {item.Age}");
        }
    }
    [TestMethod]
    public void LinqGroupBy()
    {
        var usrRepo = new DefaultRepository<User>(Db);
        var proRepo = new DefaultRepository<Product>(Db);


        var list = from u in usrRepo.Table
                   join p in proRepo.Table on u.UserId equals p.ProductCode
                   group u by u.Age into g
                   select new { g.Key, Count = g.Average(u => u.Age) };
        foreach (var item in list)
        {
            Console.WriteLine($"{item.Key} - {item.Count}");
        }
    }

    [TestMethod]
    public void LinqSkipAndTake()
    {
        var skip = 10;
        var take = 10;
        var usrRepo = new DefaultRepository<User>(Db);
        var list = usrRepo.Table.Skip(skip).Take(take);
        foreach (var item in list)
        {
            Console.WriteLine($"{item.UserId} {item.Age}");
        }
    }
}
