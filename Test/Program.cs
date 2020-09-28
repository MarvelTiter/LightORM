using DExpSql.ExpressionMethod;
using MDbAction;
using MDbContext;
using MDbEntity.Attributes;
using System;
using System.Linq;
using Test.Models.Entities;

namespace Test
{
    class Program //: Application
    {
        //[STAThread]
        static void Main(string[] args)
        {
            try
            {
                //MainWindow mw = new MainWindow();
                //Application app = new Application();
                //app.Run(mw);

                //IUserService userService = new UserService();
                //UserQueryParam p = new UserQueryParam();
                //p.Account = "00";
                //var result = userService.Select(p);

                //Console.ReadKey();


                var p = new Teacher();
                p.Age = 33;
                p.Name = "测试1111111111";
                DbContext.Init((int)DBType.Oracle, "Password=dbo_gzjwjkjcz;User ID=dbo_gzjwjkjcz;Data Source=172.18.169.230/ORCL;Persist Security Info=True");
                var db = DbContext.Instance;

               
                ////db.DbSet.Update<Users>(user);

                //foreach (var item in result)
                //{
                //    Console.WriteLine($"{item.USERNAME}:{item.LOGINDATE}");
                //}
                Console.WriteLine(db.DbSet);
                Console.WriteLine();
                //
                //begin = DateTime.Now;
                //IValidate<Teacher> validate = new ValidateImp<Teacher>();
                //validate.Validate(p);
                //var duration = DateTime.Now - begin;
                //foreach (string item in validate.ValidateMsg)
                //{
                //    Console.WriteLine(item);
                //}
                //Console.WriteLine("=============================");
                //Console.WriteLine($"Cost {duration.TotalMilliseconds} ms");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }

        static void CalcTimeSpan(string title, Action action)
        {
            var begin = DateTime.Now;
            action.Invoke();
            var duration = DateTime.Now - begin;
            Console.WriteLine($"{title} Cost : {duration.TotalMilliseconds}");
            Console.WriteLine("=============================");
        }
    }

    public class SearchParam
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    public class Teacher
    {
        public static Student stu { get; set; }
        public int ClassID { get; set; }

        [Display("年龄")]
        [Range(10, 20)]
        public int Age { get; set; }

        [Display("姓名")]
        [MaxLength(4)]
        public string Name { get; set; }

    }

    public class Student
    {
        public int ClassID { get; set; }
    }
    public class Student2
    {
        public int ClassID { get; set; }
    }
}
