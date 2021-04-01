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

            List<MemberBinding> Bindings = new List<MemberBinding>();

            Type SourceType = typeof(IDataReader);
            ParameterExpression SourceInstance = Expression.Parameter(SourceType, "SourceInstance");

            DataTable SchemaTable = dataReader.GetSchemaTable();

            //通过在目标属性和字段在记录中的循环检查哪些是匹配的
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < dataReader.FieldCount; i++) {
                foreach (PropertyInfo prop in props) {
                    //如果属性名和字段名称是一样的
                    if (prop.Name.ToLower() == dataReader.GetName(i).ToLower() && prop.CanWrite) {
                        //获取 RecordField 的类型
                        Type fieldType = dataReader.GetFieldType(i);
                        //RecordField 可空类型检查
                        if ((bool)(SchemaTable.Rows[i]["AllowDBNull"]) == true && fieldType.IsValueType) {
                            fieldType = typeof(Nullable<>).MakeGenericType(fieldType);
                        }

                        //为 RecordField 创建一个表达式
                        Expression RecordFieldExpression = Expression.Call(SourceInstance, DataRecord_ItemGetter_Int, Expression.Constant(i, typeof(int)));

                        //获取一个表示 SourceValue 的表达式
                        Expression SourceValueExpression = GetSourceValueExpression(fieldType, RecordFieldExpression);

                        //从 RecordField 到 TargetProperty 类型的值转换
                        Expression ConvertedRecordFieldExpression = GetConvertedRecordFieldExpression(fieldType, SourceValueExpression, prop.PropertyType);

                        MethodInfo TargetPropertySetter = prop.GetSetMethod();
                        //为属性创建绑定
                        var BindExpression = Expression.Bind(TargetPropertySetter, ConvertedRecordFieldExpression);
                        //将绑定添加到绑定列表
                        Bindings.Add(BindExpression);
                    }
                }
            }
            //创建 Target 的新实例并绑定到 DataRecord
            MemberInitExpression Body = Expression.MemberInit(Expression.New(type), Bindings);

            return Expression.Lambda<Func<IDataReader, object>>(Body, SourceInstance).Compile();
        }

        private Expression GetSourceValueExpression(Type recordFieldType, Expression recordFieldExpression) {
            throw new NotImplementedException();
        }

        private Expression GetConvertedRecordFieldExpression(Type recordFieldType, Expression sourceValueExpression, Type targetPropertyType) {
            throw new NotImplementedException();
        }
    }
}
