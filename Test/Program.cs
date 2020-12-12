using DExpSql;
using MDbContext;
using MDbEntity.Attributes;
using System;
using System.Linq;

namespace Test {
    class Program //: Application
    {
        //[STAThread]
        static void Main(string[] args) {
            try {
                Console.ReadKey(true);
                var limit = new SearchParam { Age = 10 };
                var sql = DbContext.Instance(null);
                CalcTimeSpan("BuildSql", () => {
                    sql.DbSet.Select<Teacher, Student>((t, s) => new {
                        AgeCount = Db.Sum(() => t.Age > limit.Age && t.Age < 15), //  =>  SUM(CASE WHEN a.Age > 10 THEN 1 ELSE null END) AgeCount
                        ClassCount = Db.Sum(() => t.ClassID > 10), //  =>  SUM(CASE WHEN a.ClassID > 10 THEN 1 ELSE null END) ClassCount
                        ClassCount2 = Db.Count(() => t.ClassID > 10) //  =>  COUNT(CASE WHEN a.ClassID > 10 THEN 1 ELSE null END) ClassCount2
                        //t.Age
                    })
                    .InnerJoin<Student>((t, s) => t.ClassID == s.ClassID)
                    .Where((t) => t.ClassID == 1)
                    .GroupBy((t) => t.Age);
                    sql.DbSet.Log();
                });

                var entity = new Teacher { Age = 20, ClassID = 2 };
                CalcTimeSpan("BuildSql", () => {
                    sql.DbSet.Update<Teacher>(() => new { entity.Age, entity.ClassID })
                    .Where(tea => tea.ClassID == 1);
                    sql.DbSet.Log();
                });


                //var userService = new UserService();
                //userService.Update(new Users());
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }

        static void CalcTimeSpan(string title, Action action) {
            var begin = DateTime.Now;
            action.Invoke();
            var duration = DateTime.Now - begin;
            Console.WriteLine($"{title} Cost : {duration.TotalMilliseconds} ms");
            Console.WriteLine("=============================");
        }
    }

    public static class Ex {
        public static string NotNullNoSpace(this string self) {
            return "11111";
        }

        public static void Log(this object self) {
            Console.WriteLine(self.ToString());
        }

        public static T Parse<T>(this object self) {
            var type = typeof(T);
            var underLyingType = Nullable.GetUnderlyingType(type);
            return (T)ChangeType(self, underLyingType ?? type);
        }

        private static object ChangeType(object value, Type type) {
            if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
            if (value == null) return default;
            if (type == value.GetType()) return value;
            if (type.IsEnum) {
                if (value is string)
                    return Enum.Parse(type, value as string);
                else
                    return Enum.ToObject(type, value);
            }
            if (!type.IsInterface && type.IsGenericType) {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = ChangeType(value, innerType);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }
            if (value is string && type == typeof(Guid)) return new Guid(value as string);
            if (value is string && type == typeof(Version)) return new Version(value as string);
            if (!(value is IConvertible)) return value;
            return Convert.ChangeType(value, type);
        }
    }


    public class SearchParam {
        public int Age { get; set; }
        public string Name { get; set; }
    }

    [TableName("t_teacher")]
    public class Teacher {
        public static Student stu { get; set; }
        public int ClassID { get; set; }

        [Display("年龄")]
        [Range(10, 20)]
        public int Age { get; set; }

        [Display("姓名")]
        [MaxLength(4)]
        public string Name { get; set; }

    }

    [TableName("t_student")]
    public class Student {
        public int ClassID { get; set; }
    }
    public class Student2 {
        public int ClassID { get; set; }
    }
}
