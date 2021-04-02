using MDbContext.SqlExecutor.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    public class ExpressionBuilder : IDeserializer {

        private static readonly MethodInfo DataRecord_ItemGetter_Int = typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetOrdinal = typeof(IDataRecord).GetMethod("GetOrdinal");
        private static readonly MethodInfo DataReader_Read = typeof(IDataReader).GetMethod("Read");
        private static readonly MethodInfo Convert_IsDBNull = typeof(Convert).GetMethod("IsDBNull");
        private static readonly MethodInfo DataRecord_GetDateTime = typeof(IDataRecord).GetMethod("GetDateTime");
        private static readonly MethodInfo DataRecord_GetDecimal = typeof(IDataRecord).GetMethod("GetDecimal");
        private static readonly MethodInfo DataRecord_GetDouble = typeof(IDataRecord).GetMethod("GetDouble");
        private static readonly MethodInfo DataRecord_GetInt32 = typeof(IDataRecord).GetMethod("GetInt32");
        private static readonly MethodInfo DataRecord_GetInt64 = typeof(IDataRecord).GetMethod("GetInt64");
        private static readonly MethodInfo DataRecord_GetString = typeof(IDataRecord).GetMethod("GetString");
        private static readonly MethodInfo DataRecord_IsDBNull = typeof(IDataRecord).GetMethod("IsDBNull");
        private static readonly MethodInfo DataRecord_GetValue = typeof(IDataRecord).GetMethod("GetValue");

        public Func<IDataReader, object> BuildDeserializer(IDataReader reader, Type targetType) {
            IDataReader dataReader = reader;
            Type type = targetType;

            //List<MemberBinding> Bindings = new List<MemberBinding>();
            List<Expression> body = new List<Expression>();
            Type SourceType = typeof(IDataReader);
            ParameterExpression SourceInstance = Expression.Parameter(SourceType, "reader");

            //DataTable SchemaTable = dataReader.GetSchemaTable();

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            // var entity = new Entity()
            var exEntity = Expression.Variable(targetType, "entity");
            Expression exEntityInstance = Expression.New(targetType);
            body.Add(Expression.Assign(exEntity, exEntityInstance));
            // object temp = null
            var temp = Expression.Variable(typeof(object), "temp");
            body.Add(Expression.Assign(temp, Expression.Constant(null)));
            for (int i = 0; i < dataReader.FieldCount; i++) {
                // 
                var prop = props.FirstOrDefault(p => p.CanWrite && p.Name.ToLower() == dataReader.GetName(i).ToLower());
                if (prop == null) continue;
                var propType = prop.PropertyType;

                // reader.get_Item(i)
                Expression exFieldValue = Expression.Call(SourceInstance, DataRecord_ItemGetter_Int, Expression.Constant(i, typeof(int)));

                // temp = reader.get_Item(i)
                Expression tempAssign = Expression.Assign(temp, exFieldValue);
                body.Add(tempAssign);
                //
                Expression propExpression = Expression.Property(exEntity, prop.Name);
                Expression converted = Expression.Convert(temp, propType);
                // entity.PropertyName = (PropertyType)temp;
                Expression propAdd = Expression.Assign(propExpression, converted);
                /*
                 * if (!(temp is DBNull || temp is null)){
                 *  
                 * }
                 */
                var exCheckDBNull = Expression.TypeIs(temp, typeof(DBNull));
                var exCheckNull = Expression.NotEqual(temp, Expression.Constant(null));
                var exIfThenElse = Expression.IfThen(
                    Expression.Not(Expression.Or(exCheckNull, exCheckDBNull)),
                    propAdd);
                body.Add(exIfThenElse);
            }
            // return entity;
            body.Add(exEntity);

            Expression block = Expression.Block(new[] { exEntity, temp }, body);
            var lambda = Expression.Lambda<Func<IDataReader, object>>(block, SourceInstance);
            return lambda.Compile();
        }       
    }
}
