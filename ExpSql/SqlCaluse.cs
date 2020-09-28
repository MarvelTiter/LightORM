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
    public class SqlCaluse
    {
        public StringBuilder Sql { get; set; }

        private Dictionary<string, char> tableAlia;

        private Queue<char> charAlia;

        #region 
        public List<string> IgnoreFields { get; private set; }

        public Dictionary<string, object> SqlParam { get; private set; }

        public List<string> SelectFields { get; private set; }

        /// <summary>
        /// 模糊查询Like  0:非模糊查询 1：like 2：leftlike 3:rightlike
        /// </summary>
        public int LikeMode { get; set; }

        public bool SelectAll { get; set; }
        public bool HasOrderBy { get; set; }
        #endregion

        #region readonly value
        public int Length { get => Sql.Length; }

        public string SelectedFieldString { get => string.Join(",", SelectFields); }

        public string ParamString { get => string.Join(",", SqlParam.Keys); }
        #endregion
        /// <summary>
        /// 0 - SqlServer; 1 - Oracle; 2 - MySql
        /// </summary>
        public int DbType { get; set; }

        private string DbParamPrefix
        {
            get
            {
                switch (DbType)
                {
                    case 0:
                        return "@";
                    case 1:
                        return ":";
                    case 2:
                        return "?";
                    default:
                        return "@";
                }
            }
        }

        public static SqlCaluse operator +(SqlCaluse sqlCaluse, string sql)
        {
            sqlCaluse.Sql.Append(sql);
            return sqlCaluse;
        }

        public SqlCaluse()
        {
            Init();
        }

        private void Init()
        {
            charAlia = new Queue<char>();
            for (int i = 97; i < 123; i++)
            {
                // a - z
                charAlia.Enqueue((char)i);
            }
            if (tableAlia == null) tableAlia = new Dictionary<string, char>();
            else tableAlia.Clear();

            if (SqlParam == null) SqlParam = new Dictionary<string, object>();
            else SqlParam.Clear();

            if (SelectFields == null) SelectFields = new List<string>();
            else SelectFields.Clear();

            if (IgnoreFields == null) IgnoreFields = new List<string>();
            else IgnoreFields.Clear();

            if (Sql == null) Sql = new StringBuilder();
            else Sql.Clear();
        }

        public void Paging(int from, int to)
        {
            var max = Math.Max(from, to);
            var min = Math.Min(from, to);
            if (DbType == 0)
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
            else if (DbType == 1)
            {
                var sql = " SELECT ROWNUM as ROWNO, SubMax.* FROM (\n {0} \n) SubMax";
                Sql = new StringBuilder(string.Format(sql, Sql.ToString()));
                Sql.AppendLine($" WHERE ROWNUM <= {max}");

                sql = " SELECT * FROM (\n {0} \n) SubMin";
                Sql = new StringBuilder(string.Format(sql, Sql.ToString()));
                Sql.AppendLine($" WHERE SubMin.ROWNO > {min}");
            }
            else
                throw new NotImplementedException("其余数据库分页查询未实现");
        }

        public string AddDbParameter(object parameterValue)
        {
            if (parameterValue == null || parameterValue == DBNull.Value)
            {
                this.Sql.Append(" null");
                return "";
            }
            else
            {
                var type = parameterValue.GetType();
                string name = this.DbParamPrefix + "param" + this.SqlParam.Count;
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
                return " " + name;
            }
        }

        public bool SetTableAlias(string tableName)
        {
            if (!tableAlia.Keys.Contains(tableName))
            {
                tableAlia.Add(tableName, charAlia.Dequeue());
                return true;
            }
            return false;
        }

        public string GetTableAlias(string tableName)
        {
            if (tableAlia.Keys.Contains(tableName))
            {
                return tableAlia[tableName].ToString();
            }
            return "";
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
}
