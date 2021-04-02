using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.Extension {
    public static class ObjectReadProperty {
        private static Dictionary<Type,Func<object, object>> funcDic;
        public static object ReadProperty(this object self, PropertyInfo property) {
            // self => (Type)self.Property
            if (funcDic == null) {
                funcDic = new Dictionary<Type, Func<object, object>>();
            }
            var type = self.GetType();

            if (!funcDic.ContainsKey(type)) { 
                ParameterExpression param = Expression.Parameter(typeof(object), "self");
                Expression convertedParam = Expression.Convert(param, type);
                MemberExpression member = Expression.Property(convertedParam, property);
                Expression converted = Expression.Convert(member, typeof(object));
                var lambda = Expression.Lambda<Func<object, object>>(converted, param);
                var func = lambda.Compile();
                funcDic.Add(type, func);
            }           
            return funcDic[type].Invoke(self);
        }
    }
}
