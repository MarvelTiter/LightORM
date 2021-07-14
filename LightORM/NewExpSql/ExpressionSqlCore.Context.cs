using MDbContext.Extension;
using MDbContext.NewExpSql.SqlFragment;
using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDbContext.NewExpSql {
    internal class ExpressionSqlCoreContext<T> : ISqlContext {
        private Dictionary<Type, char> tableAlia;
        private Queue<char> charAlia;
        List<BaseFragment> fragments;
        Dictionary<string, object> parameters;
        public Position Position { get; set; }
        public DbBaseType DbType { get; private set; }
        public int WhereIndex { get; set; } = -1;
        public LikeMode LikeMode { get; set; } = LikeMode.None;

        public ExpressionSqlCoreContext(DbBaseType dbType) {
            Init();
            DbType = dbType;
        }

        private void Init() {
            charAlia = new Queue<char>();
            fragments = new List<BaseFragment>();
            parameters = new Dictionary<string, object>();
            for (int i = 97; i < 123; i++) {
                // a - z
                charAlia.Enqueue((char)i);
            }
            if (tableAlia == null) tableAlia = new Dictionary<Type, char>();
            else tableAlia.Clear();
        }
        private bool CheckAssign(Type newType, out Type registedType) {
            foreach (var item in tableAlia.Keys) {
                if (item.IsAssignableFrom(newType) || newType.IsAssignableFrom(item)) {
                    registedType = item;
                    return true;
                }
            }
            registedType = null;
            return false;
        }
        public bool SetTableAlias(Type tableName) {
            if (!CheckAssign(tableName, out _) && !tableAlia.Keys.Contains(tableName)) {
                tableAlia.Add(tableName, charAlia.Dequeue());
                return true;
            }
            return false;
        }

        public string GetTableAlias(Type tableName) {
            if (CheckAssign(tableName, out Type registed)) {
                return tableAlia[registed].ToString();
            } else if (tableAlia.Keys.Contains(tableName)) {
                return tableAlia[tableName].ToString();
            }
            return "";
        }

        public string GetTableName(bool withAlias, Type t = null) {
            if (t == null) t = typeof(T);
            var attrs = t.GetAttribute<TableNameAttribute>();
            var tbName = attrs is null ? t.Name : attrs.TableName;
            if (withAlias) {
                return tbName + " " + GetTableAlias(t);
            } else {
                return tbName;
            }
        }

        public string GetPrefix() {
            switch (DbType) {
                case DbBaseType.SqlServer:
                case DbBaseType.Sqlite:
                    return "@";
                case DbBaseType.Oracle:
                    return ":";
                case DbBaseType.MySql:
                    return "?";
                default:
                    return "@";
            }
        }

        public string AddDbParameter(object parameterValue) {
            if (parameterValue == null || parameterValue == DBNull.Value) {
                return " null";
            } else {
                //var type = parameterValue.GetType();
                string name = GetPrefix() + "param" + parameters.Count;
                object val = parameterValue;
                switch (LikeMode) {
                    case LikeMode.Like:
                        val = $"%{parameterValue}%";
                        break;
                    case LikeMode.LeftLike:
                        val = $"%{parameterValue}";
                        break;
                    case LikeMode.RightLike:
                        val = $"{parameterValue}%";
                        break;
                }
                parameters.Add(name, val);
                LikeMode = LikeMode.None;
                return name;
            }
        }

        public string BuildSql(out Dictionary<string, object> keyValues) {
            keyValues = parameters;
            StringBuilder sql = new StringBuilder();
            foreach (var item in fragments) {
                sql.AppendLine(item.ToString());
            }
            return sql.ToString();
        }

        public void AddFragment<F>(F fragment) where F : BaseFragment {
            fragments.Add(fragment);
        }
    }
}
