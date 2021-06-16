using DExpSql;
using MDbContext;
using MDbContext.Extension;
using MDbContext.SqlExecutor;
using MDbContext.SqlExecutor.Service;
using MDbEntity.Attributes;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Test {
    class Program //: Application
    {
        //[STAThread]
        static void Main(string[] args) {
            try {
                var fs = File.Open(@"E:\Documents\Desktop\247Struct.json", FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(fs);
                var jsonString = reader.ReadToEnd();
                var S247 = JsonConvert.DeserializeObject<List<TableStruct>>(jsonString);

                var fs2 = File.Open(@"E:\Documents\Desktop\21Struct.json", FileMode.Open, FileAccess.Read);
                StreamReader reader2 = new StreamReader(fs2);
                var jsonString2 = reader2.ReadToEnd();
                var S21 = JsonConvert.DeserializeObject<List<TableStruct>>(jsonString2);
                int success = 0;
                foreach (TableStruct item in S247) {
                    TableStruct diff = S21.FirstOrDefault(ts => ts.TABLENAME == item.TABLENAME && ts.COLUMNNAME == item.COLUMNNAME);
                    if (diff.NULLABLE == item.NULLABLE) {
                        success += 1;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{item.TABLENAME}:{item.COLUMNNAME}:{item.NULLABLE} == {diff.TABLENAME}:{diff.COLUMNNAME}:{diff.NULLABLE}");
                    } else {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{item.TABLENAME}:{item.COLUMNNAME}:{item.NULLABLE} <> {diff.TABLENAME}:{diff.COLUMNNAME}:{diff.NULLABLE}");
                    }
                }
                Console.WriteLine($"总数：555，一致数:{success}");

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
    }

    class TableStruct {
        public string TABLENAME { get; set; }
        public string COLUMNNAME { get; set; }
        public string NULLABLE { get; set; }
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
}
