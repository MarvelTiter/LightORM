using MDbContext;
using MDbContext.Extension;
using MDbContext.SqlExecutor;
using MDbContext.SqlExecutor.Service;
using MDbEntity.Attributes;
using Notice.Core.Entities;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace Test {
    class Program //: Application
    {
        //[STAThread]
        static void Main(string[] args) {
            try {
                //Console.ReadKey(true);
                //MDbContext.DbContext.Init(2);
                //var limit = new SearchParam { Age = 10 };
                //var db = MDbContext.DbContext.Instance(null);
                //CalcTimeSpan("BuildSql", () => {
                //    db.DbSet.Select<Student>()
                //     .Where(t => t.ClassID2 == 1);
                //});

                //db.DbSet.ToString().Log();
                //var entity = new Teacher { Age = 20, ClassID = 2 };
                //CalcTimeSpan("BuildSql", () => {
                //    db.DbSet.Update<Teacher>(() => new { entity.Age, entity.ClassID })
                //    .Where(tea => tea.ClassID == 1);
                //    db.DbSet.Log();
                //});

                using (IDbConnection conn = new OracleConnection("Password=dbo_gzjwjkjcz;User ID=dbo_gzjwjkjcz;Data Source=172.18.169.230/ORCL;Persist Security Info=True")) {
                    var db = conn.DbContext();
                    db.DbSet.Select<RoadTransTruck>();
                    var com = conn.CreateCommand();
                    com.CommandText = db.Sql;
                    conn.Open();
                    var reader = com.ExecuteReader();
                    //IDeserializer des = new ExpressionBuilder();
                    //des.BuildDeserializer(reader, typeof(RoadTransTruck));
                    var dt = reader.GetSchemaTable();
                    CalcTimeSpan("CustomReflection", () => {
                        //CustomReflection(reader);
                    });

                    CalcTimeSpan("EmitReflection", () => {
                        EmitReflection(reader);
                    });
                }



            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }

        static void CalcTimeSpan(string title, Action action) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            action.Invoke();
            stopwatch.Stop();
            Console.WriteLine($"{title} Cost : {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine("=============================");
        }

        static void CustomReflection(IDataReader reader) {
            List<RoadTransTruck> list = new List<RoadTransTruck>();
            var props = typeof(RoadTransTruck).GetProperties();
            while (reader.Read()) {
                RoadTransTruck road = new RoadTransTruck();
                for (int i = 0; i < reader.FieldCount; i++) {
                    var name = reader.GetName(i);
                    var value = reader.GetValue(i);
                    var prop = props.FirstOrDefault(p => p.CanWrite && p.Name == name);
                    if (prop != null && value != DBNull.Value) {
                        var type = prop.PropertyType;
                        var underlyingType = Nullable.GetUnderlyingType(type);
                        prop.SetValue(road, Convert.ChangeType(value, underlyingType ?? type));
                    }
                }
                list.Add(road);
            }
            Console.WriteLine(list.Count());
        }

        static void EmitReflection(IDataReader reader) {
            var result = reader.Select<RoadTransTruck>().ToList();
            Console.WriteLine(result.Count());
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
    public class Student : Student2 {
        public int ClassID { get; set; }
    }
    public class Student2 {
        public int ClassID2 { get; set; }
    }
}
