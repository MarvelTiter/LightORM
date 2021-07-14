using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MDbContext.NewExpSql.SqlFragment {


    internal abstract class BaseFragment {

        protected static Dictionary<Type, Dictionary<string, BaseFragment>> cache = new Dictionary<Type, Dictionary<string, BaseFragment>>();
        protected bool HasResolved { get; set; }
        internal StringBuilder Sql { get; set; } = new StringBuilder();
        internal Dictionary<string, object> SqlParameters { get; set; }
        internal int Length => Sql.Length;
        protected ISqlContext Context { get; set; }
        internal virtual void ResolveSql(Expression body, params Type[] types) {
            if (HasResolved) return;
            DoResolve(body, types);
            HasResolved = true;
        }

        internal virtual void ResolveParam() => throw new NotImplementedException();

        protected abstract void DoResolve(Expression body, params Type[] types);
        public abstract override string ToString();

        public static T GetPart<T>(string expKey, params object[] parameters) where T : BaseFragment {

            bool v = cache.TryGetValue(typeof(T), out var partCache);
            if (!v) {
                partCache = new Dictionary<string, BaseFragment>();
                //cache.Add(typeof(T), partCache);
                return CreatePart();
            } else {
                bool v1 = partCache.TryGetValue(expKey, out var fragment);
                if (!v1) {
                    return CreatePart();
                }
                return (T)fragment;
            }

            T CreatePart() {
                var ctor = typeof(T).GetConstructors().First(c => c.GetParameters().Length == parameters.Length);
                if (ctor == null) {
                    throw new ArgumentException($"can not found constructor with {parameters.Length} parameter");
                }
                var part = (T)ctor.Invoke(parameters);
                partCache.Add(expKey, part);
                return part;
            }
        }

        internal void Add(string str) => Sql.Append(str);
        internal void Remove(string str) {
            var start = Sql.Length - str.Length;
            Sql.Remove(start, str.Length);
        }
        internal bool EndWith(string str) =>
            //char[] vs = str.ToCharArray();
            //var start = Sql.Length - str.Length;
            //for (int i = 0; i < str.Length; i++) {

            //}
            Sql.ToString().EndsWith(str);
    }
}
