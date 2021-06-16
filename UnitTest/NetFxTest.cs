using MDbContext;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Test.Core.Models;

namespace UnitTest {
    [TestClass]
    public class NetFxTest {
        [TestMethod]
        public void TestMethod1() {
            DbContext.Init(0);
            //using (IDbConnection conn = new SqlConnection("Persist Security Info=False;User ID=sa;Password=sa;Initial Catalog=APDSDB2020;Server=192.168.0.104")) {
            using (IDbConnection conn = new OracleConnection("Password=cgs;User ID=cgs;Data Source=192.168.5.10/gzorcl;Persist Security Info=True")) {
                var db = conn.DbContext();
                var sql = db.DbSet.Select<Job>();
                var result = db.Query<Job>();
                foreach (var item in result) {
                    Debug.WriteLine(item.ToString());
                }
            }
        }
        [TestMethod]
        public void TestNumber() {
            var number = 12312312.123;
            var s = number.ToString("#L#E#D#C#K#E#D#C#J#E#D#C#I#E#D#C#H#E#D#C#G#E#D#C#F#E#D#C#.0B0A");
            var d = Regex.Replace(s, @"((?<=-|^)[^1-9]*)|((?'z'0)[0A-E]*((?=[1-9])|(?'-z'(?=[F-L\.]|$))))|((?'b'[F-L])(?'z'0)[0A-L]*((?=[1-9])|(?'-z'(?=[\.]|$))))", "${b}${z}");
            var r = Regex.Replace(d, ".", m => "负元空零壹贰叁肆伍陆柒捌玖空空空空空空空分角拾佰仟万亿兆京垓秭穰"[m.Value[0] - '-'].ToString());
            Debug.WriteLine(r);
        }

        [TestMethod]
        public void TestEmunerable() {
            var list = GetIEnumerable();
            foreach (var item in list) {
                Debug.WriteLine($"{item.ID},{item.Name}");
            }
        }

        public class Mission:INotifyPropertyChanged {
            public int ID { get; set; }
            public int ConnectionID { get; set; }
            public int GroupID { get; set; }
            public string Name { get; set; }
            public string Content { get; set; }
            public string Description { get; set; }
            /// <summary>
            /// 0-日统计全天，1-日统计时间段 2-月统计
            /// </summary>
            private int _MissionType;

            public event PropertyChangedEventHandler PropertyChanged;

            public int MissionType {
                get { return _MissionType; }
                set { SetValue(ref _MissionType, value); }
            }

            private void SetValue(ref int missionType, int value) {
                missionType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MissionType"));
            }

            public string MissionParameter { get; set; }
        }
        private IEnumerable<Mission> GetIEnumerable() {
            DbContext.Init(3);
            using (var conn = new SqliteConnection(@"DataSource=E:\GitRepositories\CGS\CGS\bin\Debug\CGS.db")) {
                var db = conn.DbContext();
                db.DbSet.Select<Mission>();
                return db.Query<Mission>();
            }
        }

    }
}
