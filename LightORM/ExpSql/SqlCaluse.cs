using MDbContext;
using MDbEntity.Attributes;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql
{

    public struct DbHelper
    {
        public string Prefix { get; set; }
        public Func<string, string> ToDbString { get; set; }

        public DbHelper(string prefix, Func<string, string> func)
        {
            Prefix = prefix;
            ToDbString = func;
        }
    }
    public partial class SqlCaluse
    {
        public Func<string> GetMemberName { get; set; }
        public StringBuilder Sql { get; set; }

        private Dictionary<Type, char> tableAlia;

        private Queue<char> charAlia;

        #region 
        public List<string> IgnoreFields { get; private set; }
        public bool EnableTableAlia { get; set; } = true;

        public Dictionary<string, object> SqlParam { get; private set; }

        public List<string> SelectFields { get; private set; }

        public List<string> GroupByFields { get; private set; }

        /// <summary>
        /// 0 - 返回布尔值 形如(sum(case when expression then 1 else o end))， 1 - 按列统计，形如 sum(colname)
        /// </summary>
        public int SelectMethodType { get; set; } = 1;
        public StringBuilder SelectMethod { get; set; }
        /// <summary>
        /// 模糊查询Like  0:非模糊查询 1：like 2：leftlike 3:rightlike
        /// </summary>
        public int LikeMode { get; set; }

        public bool SelectAll { get; set; }

        public bool HasOrderBy { get; set; }

        /// <summary>
        /// 0 - SqlServer; 1 - Oracle; 2 - MySql 3 - Sqlite
        /// </summary>
        public DbBaseType DbType { get; set; }
        #endregion

        #region readonly value
        public int Length { get => Sql.Length; }

        public string SelectedFieldString { get => string.Join(",", SelectFields); }

        public string ParamString { get => string.Join(",", SqlParam.Keys); }

        public string GroupByFieldString { get => string.Join(",", GroupByFields); }
        #endregion

        Dictionary<DbBaseType, DbHelper> DbHelperDic = new();

        public DbHelper DbHelper
        {
            get
            {
                if (DbHelperDic.ContainsKey(DbType))
                {
                    return (DbHelperDic[DbType]);
                }
                throw new InvalidOperationException($"Unsuported DataBase {DbType}");
            }
        }

        public SqlCaluse()
        {
            Init();
            var sqlserver = new DbHelper("@", value => $"CASE({value} as VARCHAR)");
            var oracle = new DbHelper(":", value => $"TO_CHAR({value})");
            var mysql = new DbHelper("?", value => $"CONVERT(VARCHAR, {value})");
            var sqlite = new DbHelper("@", value => throw new NotImplementedException("Sqlite ToDbString NotImplemented"));
            DbHelperDic.Add(DbBaseType.SqlServer, sqlserver);
            DbHelperDic.Add(DbBaseType.Oracle, oracle);
            DbHelperDic.Add(DbBaseType.MySql, mysql);
            DbHelperDic.Add(DbBaseType.Sqlite, sqlite);
        }

        private void Init()
        {
            charAlia = new Queue<char>();
            for (int i = 97; i < 123; i++)
            {
                // a - z
                charAlia.Enqueue((char)i);
            }
            if (tableAlia == null) tableAlia = new Dictionary<Type, char>();
            else tableAlia.Clear();

            if (SqlParam == null) SqlParam = new Dictionary<string, object>();
            else SqlParam.Clear();

            if (SelectFields == null) SelectFields = new List<string>();
            else SelectFields.Clear();

            if (IgnoreFields == null) IgnoreFields = new List<string>();
            else IgnoreFields.Clear();

            if (GroupByFields == null) GroupByFields = new List<string>();
            else GroupByFields.Clear();

            if (Sql == null) Sql = new StringBuilder();
            else Sql.Clear();

            if (SelectMethod == null) SelectMethod = new StringBuilder();
            else SelectMethod.Clear();
        }

        public string AddDbParameter(object parameterValue, StringBuilder sql = null)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
            {
                if (sql == null) sql = Sql;
                sql.Append(" null");
                return "";
            }
            else
            {
                var type = parameterValue.GetType();
                string name = DbHelper.Prefix + "p" + SqlParam.Count;
                switch (LikeMode)
                {
                    case 1:
                        this.SqlParam.Add(name, $"%{parameterValue}%");
                        break;
                    case 2:
                        this.SqlParam.Add(name, $"%{parameterValue}");
                        break;
                    case 3:
                        this.SqlParam.Add(name, $"{parameterValue}%");
                        break;
                    default:
                        this.SqlParam.Add(name, parameterValue);
                        break;
                }
                //this.Sql.Append(" "+name);
                LikeMode = 0;
                return name;
            }
        }
        private bool CheckAssign(Type newType, out Type registedType)
        {
            foreach (var item in tableAlia.Keys)
            {
                if (item.IsAssignableFrom(newType) || newType.IsAssignableFrom(item))
                {
                    registedType = item;
                    return true;
                }
            }
            registedType = null;
            return false;
        }
        public bool SetTableAlias(Type tableName)
        {
            if (!CheckAssign(tableName, out _) && !tableAlia.Keys.Contains(tableName))
            {
                tableAlia.Add(tableName, charAlia.Dequeue());
                return true;
            }
            return false;
        }

        public string GetTableAlias(Type tableName)
        {
            if (!EnableTableAlia)
            {
                return "";
            }
            if (CheckAssign(tableName, out Type registed))
            {
                return tableAlia[registed].ToString() + ".";
            }
            else if (tableAlia.Keys.Contains(tableName))
            {
                return tableAlia[tableName].ToString() + ".";
            }
            return "";
        }

        public string GetTableName(Type t)
        {
            var attrs = t.GetCustomAttributes(false);
            if (attrs.Length > 0)
            {
                foreach (var item in attrs)
                {
                    if (item is TableNameAttribute table)
                    {
                        return table.TableName;
                    }
                }
            }
            return t.Name;
        }

        public void Clear()
        {
            Init();
        }

        public bool EndWith(string str)
        {
            return Sql.ToString().EndsWith(str);
        }

        public override string ToString()
        {
            return this.Sql.ToString();
        }
    }

    /// <summary>
    /// 分页
    /// </summary>
    public partial class SqlCaluse
    {

        public static SqlCaluse operator +(SqlCaluse sqlCaluse, string sql)
        {
            sqlCaluse.Sql.Append(sql);
            return sqlCaluse;
        }
        public static SqlCaluse operator -(SqlCaluse sqlCaluse, string sql)
        {
            var start = sqlCaluse.Sql.Length - sql.Length;
            sqlCaluse.Sql.Remove(start, sql.Length);
            return sqlCaluse;
        }

        public void Paging(int from, int to)
        {
            var max = Math.Max(from, to);
            var min = Math.Min(from, to);
            var minParam = AddDbParameter(min);
            if (DbType == DbBaseType.SqlServer)
            {
                var maxParam = AddDbParameter(max);
                SqlServerPaging(maxParam, minParam);
            }
            else if (DbType == DbBaseType.Oracle)
            {
                var maxParam = AddDbParameter(max);
                OraclePaging(maxParam, minParam);
            }
            else if (DbType == DbBaseType.MySql)
            {
                var diff = AddDbParameter(max - min);
                MySqlPaging(minParam, diff);

            }else if (DbType == DbBaseType.Sqlite)
            {
                var diff = AddDbParameter(max - min);
                SqlitePaging(minParam, diff);
            }
            else
                throw new NotImplementedException("其余数据库分页查询未实现");
        }

        private void MySqlPaging(string min, string diff)
        {
            Sql.AppendLine($" LIMIT {min},{diff}");
        }

        private void SqlitePaging(string min, string diff)
        {
            Sql.AppendLine($" LIMIT {min},{diff}");
        }

        private void OraclePaging(string max, string min)
        {
            var sql = " SELECT ROWNUM as ROWNO, SubMax.* FROM (\n {0} \n) SubMax";
            Sql = new StringBuilder(string.Format(sql, Sql.ToString()));
            Sql.AppendLine($" WHERE ROWNUM <= {max}");

            sql = " SELECT * FROM (\n {0} \n) SubMin";
            Sql = new StringBuilder(string.Format(sql, Sql.ToString()));
            Sql.AppendLine($" WHERE SubMin.ROWNO > {min}");
        }

        private void SqlServerPaging(string max, string min)
        {
            if (HasOrderBy)
                throw new Exception("SqlServer分页查询，子查询中无法使用OrderBy！");
            var orderByField = SelectFields[0].Remove(0, 2);
            // 子查询，获得ROWNO
            var sql = $"SELECT ROW_NUMBER() OVER(ORDER BY Sub.{orderByField}) ROWNO," + " Sub.* FROM (\n {0} \n ) Sub";
            Sql = new StringBuilder(string.Format(sql, Sql.ToString()));

            // 子查询筛选 ROWNO
            sql = " SELECT * FROM (\n {0} \n ) Paging";
            Sql = new StringBuilder(string.Format(sql, Sql.ToString()));
            Sql.Append($"\n WHERE Paging.ROWNO > {min}");
            Sql.Append($" AND Paging.ROWNO <= {max}");
        }
    }
}
