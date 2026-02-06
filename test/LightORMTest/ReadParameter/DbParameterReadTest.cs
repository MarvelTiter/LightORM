using LightORM.Cache;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace LightORMTest.ReadParameter;

[TestClass]
public class DbParameterReadTest
{
    [TestMethod]
    public void ReadAnonymousTypeToDic()
    {
        var p1 = new
        {
            Date = DateTime.Now,
            Age = 18,
            Name = "Marvel"
        };
        var dic = DbParameterReader.ObjectToDictionary("@", "@Date, @Age, @Name", p1);
        var dic2 = DbParameterReader.ObjectToDictionary("@", "@Date, @Age, @Name", p1);
        Assert.IsTrue((DateTime)dic["Date"] == p1.Date);
        Assert.IsTrue((int)dic["Age"] == p1.Age);
        Assert.IsTrue((string)dic["Name"] == p1.Name);
    }
    enum TestE
    {
        T1 = 10,
        T2 = 20,
    }
    [TestMethod]
    public void ReadAnonymousTypeParameter()
    {
        TestE? test1 = null;
        TestE? test2 = TestE.T2;
        int? ia1 = null;
        int? ia2 = 18;
        var p1 = new
        {
            Date = DateTime.Now,
            Age = ia2,
            Name = "Marvel",
            Level1 = ia1,
            Level2 = 2,
            Type1 = test1,
            Type2 = test2,
            Type_3 = TestE.T1
        };
        using var conn = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");
        var cmd = conn.CreateCommand();
        Action<IDbCommand, object>? dic = DbParameterReader.GetDbParameterReader("@Date, @Age, @Name, @Level1, @Level2, @Type1, @Type2, @Type_3", "@", p1.GetType());
        dic(cmd, p1);
        foreach (SQLiteParameter item in cmd.Parameters)
        {
            Console.WriteLine($"{item.ParameterName} => {item.DbType} => {item.Value}");
        }
    }
}
