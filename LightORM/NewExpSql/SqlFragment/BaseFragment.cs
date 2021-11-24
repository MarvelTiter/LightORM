using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.SqlFragment
{
    internal abstract class BaseFragment
    {
        protected static Dictionary<Type, Dictionary<string, BaseFragment>> cache = new Dictionary<Type, Dictionary<string, BaseFragment>>();
        protected bool HasResolved { get; set; }
        protected bool RequiredAlia { get; set; } = true;
        internal StringBuilder Sql { get; set; } = new StringBuilder();
        internal Dictionary<string, object> SqlParameters { get; set; } = new Dictionary<string, object>();
        internal int Length => Sql.Length;
        internal ITableContext Tables { get; set; }
        internal virtual void ResolveSql(Expression body, params Type[] types)
        {
            if (HasResolved) return;
            DoResolve(body, types);
            HasResolved = true;
        }
        internal void SetTables<T>(ref ITableContext tableContext, DbBaseType dbBaseType)
        {
            if (Tables == null)
            {
                if (tableContext == null)
                    tableContext = new TableContext<T>(dbBaseType);
                Tables = tableContext;
            }
            else
            {
                tableContext = Tables;
            }
        }
        internal virtual void ResolveParam() => throw new NotImplementedException();

        protected abstract void DoResolve(Expression body, params Type[] types);
        //protected abstract string ConstructSql();
        internal string AddDbParameter(object value)
        {
            if (SqlParameters == null) SqlParameters = new Dictionary<string, object>();
            var name = $"{Tables.GetPrefix()}Const{SqlParameters.Count}";
            SqlParameters.Add(name, value);
            return name;
        }

        public override string ToString()
        {
            //var sql = ConstructSql();
            return Sql.ToString();
        }

        internal void SqlAppend(string str) => Sql.Append(str);

        internal void Remove(string str)
        {
            var start = Sql.Length - str.Length;
            Sql.Remove(start, str.Length);
        }

        internal bool EndWith(string str) => Sql.ToString().EndsWith(str);

        public static T GetPart<T>(string expKey, params object[] parameters) where T : BaseFragment
        {
            T partCache = FragmentCache<T>.GetPart(expKey);
            if (partCache == default)
            {
                partCache = CreatePart<T>(parameters);
                if (!string.IsNullOrEmpty(expKey))
                    FragmentCache<T>.AddCache(expKey, partCache);
            }
            return partCache;
        }

        private static T CreatePart<T>(params object[] parameters) where T : BaseFragment
        {
            var ctor = typeof(T).GetConstructors().First(c => c.GetParameters().Length == parameters.Length);
            if (ctor == null)
            {
                throw new ArgumentException($"can not found constructor with {parameters.Length} parameter");
            }
            var part = (T)ctor.Invoke(parameters);
            return part;
        }
    }
}
