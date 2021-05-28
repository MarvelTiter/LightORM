using DExpSql;
using MDbContext;
using MDbContext.Extension;
using MDbContext.SqlExecutor;
using MDbContext.SqlExecutor.Service;
using MDbEntity.Attributes;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
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


                //MD5 md5 = MD5.Create();
                //byte[] buffers = md5.ComputeHash(Encoding.Default.GetBytes("123"));

                //string result1 = "";
                //for (int i = 0; i < buffers.Length; i++) {
                //    result1 += buffers[i].ToString("x2");
                //}
                //Console.WriteLine(result1);
                //Console.ReadKey();
                OracleCommand cmd = new OracleConnection().CreateCommand();
                cmd.BindByName = true;
                cmd.InitialLONGFetchSize = -1;

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
