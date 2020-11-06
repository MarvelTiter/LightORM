using DExpSql.ExpressionMethod;
using MDbAction;
using MDbContext;
using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
                var task = Task.Run(() =>
                {
                    var m = 0;
                    var r = 1 / m;
                });
                task.Wait();
                throw task.Exception;
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
