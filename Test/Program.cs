using DExpSql;
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
                DbContext.Init(1);
                using (IDbConnection conn = new OracleConnection("Password=cgs;User ID=cgs;Data Source=192.168.5.10/gzorcl;Persist Security Info=True")) {
                    var db = conn.DbContext();
                    var sql = db.DbSet.Select<Job>().Paging(0, 10);
                    var result = db.Query<Job>();
                    foreach (Job item in result) {
                        Console.WriteLine($"{item.JOB_ID}-{item.JOB_SN}-{item.JOB_COMMENT}");
                    }
                }

                //DbContext.Init(0);
                //using (IDbConnection conn = new SqlConnection("Data Source=192.168.56.11;Initial Catalog=APDSDBNEW; User Id=sa;Password=Ybeluoek3")) {
                //    var db = conn.DbContext();
                //    db.DbSet.Select<Users>();
                //    var result = db.Query<Users>();
                //    foreach (var item in result) {
                //        Console.WriteLine($"{item.Account}-{item.Age}-{item.Tel}");
                //    }
                //}

            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            Console.ReadKey();
        }
        static void CalcTimeSpan(string title, Action action) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            action.Invoke();
            stopwatch.Stop();
            Console.WriteLine($"{title} Cost : {stopwatch.Elapsed}");
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

        static void ExpressionTreeReflection(IDataReader reader, Func<IDataReader, object> func) {
            List<RoadTransTruck> result = new List<RoadTransTruck>();
            while (reader.Read()) {
                result.Add((RoadTransTruck)func(reader));
            }
            Console.WriteLine(result.Count());
        }
        static void DapperQuery(DbContext db) {
            var result = db.Query<RoadTransTruck>().ToList();
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

    public class Users {
        public string Account { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? Tel { get; set; }
        public string Sex { get; set; }
        public int? Age { get; set; }
        public string Duty { get; set; }
        public bool? IsUse { get; set; }
        public string ValidDate { get; set; }
    }

    [TableName("DQJY_JOBS")]
    public class Job {
        public double JOB_ID { get; set; }
        public string JOB_SN { get; set; }
        public double JOB_SEQ { get; set; }
        public DateTime? JOB_DATE { get; set; }
        public string JOB_PLATE { get; set; }
        public string JOB_PCLASS { get; set; }
        public double BNS_ID { get; set; }
        public string STN_ID { get; set; }
        public double JOB_STATE { get; set; }
        public string USR_ID { get; set; }
        public DateTime? JOB_TIMESTAMP { get; set; }
        public string JOB_COMMENT { get; set; }
        public string JOB_WHMD { get; set; }
        public string JOB_CANCELRESULT { get; set; }
        public double JOB_APP1_RLT { get; set; }
        public string JOB_APP1_USR_ID { get; set; }
        public double JOB_APP2_RLT { get; set; }
        public string JOB_APP2_USR_ID { get; set; }
        public string BOOKING_SOURCE { get; set; }
        public string CYYXM { get; set; }
        public string JGXTJYLSH { get; set; }
        public string CLSBDH { get; set; }
    }
}
