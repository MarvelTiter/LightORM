using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    public class Mapper {
        /// <summary>
        /// 从提供的 DataRecord 对象创建新委托实例。
        /// </summary>
        /// <param name="RecordInstance">表示一个 DataRecord 实例</param>
        /// <returns>从提供的 DataRecord 对象创建新委托实例。</returns>
        /// <remarks></remarks>
        private static Func<IDataReader, T> GetInstanceCreator<T>(IDataReader RecordInstance) {
            List<MemberBinding> Bindings = new List<MemberBinding>();
            Type TargetType = typeof(T);
            Type SourceType = typeof(IDataReader);
            ParameterExpression SourceInstance = Expression.Parameter(SourceType, "SourceInstance");
            MethodInfo GetSourcePropertyMethodExpression = SourceType.GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod();

            DataTable SchemaTable = RecordInstance.GetSchemaTable();

            //通过在目标属性和字段在记录中的循环检查哪些是匹配的
            for (int i = 0; i <= RecordInstance.FieldCount - 1; i++) {
                foreach (PropertyInfo TargetProperty in TargetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                    //如果属性名和字段名称是一样的
                    if (TargetProperty.Name.ToLower() == RecordInstance.GetName(i).ToLower() && TargetProperty.CanWrite) {
                        //获取 RecordField 的类型
                        Type RecordFieldType = RecordInstance.GetFieldType(i);
                        //RecordField 可空类型检查
                        if ((bool)(SchemaTable.Rows[i]["AllowDBNull"]) == true && RecordFieldType.IsValueType) {
                            RecordFieldType = typeof(Nullable<>).MakeGenericType(RecordFieldType);
                        }

                        //为 RecordField 创建一个表达式
                        Expression RecordFieldExpression = Expression.Call(SourceInstance, GetSourcePropertyMethodExpression, Expression.Constant(i, typeof(int)));

                        //获取一个表示 SourceValue 的表达式
                        Expression SourceValueExpression = GetSourceValueExpression(RecordFieldType, RecordFieldExpression);

                        Type TargetPropertyType = TargetProperty.PropertyType;
                        //从 RecordField 到 TargetProperty 类型的值转换
                        Expression ConvertedRecordFieldExpression = GetConvertedRecordFieldExpression(RecordFieldType, SourceValueExpression, TargetPropertyType);

                        MethodInfo TargetPropertySetter = TargetProperty.GetSetMethod();
                        //为属性创建绑定
                        var BindExpression = Expression.Bind(TargetPropertySetter, ConvertedRecordFieldExpression);
                        //将绑定添加到绑定列表
                        Bindings.Add(BindExpression);
                    }
                }
            }
            //创建 Target 的新实例并绑定到 DataRecord
            MemberInitExpression Body = Expression.MemberInit(Expression.New(TargetType), Bindings);

            return Expression.Lambda<Func<IDataReader, T>>(Body, SourceInstance).Compile();
        }

        /// <summary>
        /// 获取表示 RecordField 真实值的表达式。
        /// </summary>
        /// <param name="RecordFieldType">表示 RecordField 的类型。</param>
        /// <param name="RecordFieldExpression">表示 RecordField 的表达式。</param>
        /// <returns>表示 SourceValue 的表达式。</returns>
        private static Expression GetSourceValueExpression(Type RecordFieldType, Expression RecordFieldExpression) {
            //首先从 RecordField 取消装箱值，以便我们可以使用它
            UnaryExpression UnboxedRecordFieldExpression = Expression.Convert(RecordFieldExpression, RecordFieldType);

            //获取一个检查 SourceField 为 null 值的表达式
            UnaryExpression NullCheckExpression = Expression.IsFalse(Expression.TypeIs(RecordFieldExpression, typeof(DBNull)));

            ParameterExpression Value = Expression.Variable(RecordFieldType, "Value");
            //获取一个设置 TargetProperty 值的表达式
            Expression SourceValueExpression = Expression.Block(
                new ParameterExpression[] { Value },
                Expression.IfThenElse(
                    NullCheckExpression,
                    Expression.Assign(Value, UnboxedRecordFieldExpression),
                    Expression.Assign(Value, Expression.Constant(GetDefaultValue(RecordFieldType), RecordFieldType))),
                    Expression.Convert(Value, RecordFieldType));
            return SourceValueExpression;
        }

        private static object GetDefaultValue(Type recordFieldType) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets an expression representing the recordField converted to the TargetPropertyType
        /// </summary>
        /// <param name="RecordFieldType">The Type of the RecordField</param>
        /// <param name="UnboxedRecordFieldExpression">An Expression representing the Unboxed RecordField value</param>
        /// <param name="TargetPropertyType">The Type of the TargetProperty</param>
        /// <returns></returns>
        private static Expression GetConvertedRecordFieldExpression(Type RecordFieldType, Expression UnboxedRecordFieldExpression, Type TargetPropertyType) {
            Expression ConvertedRecordFieldExpression = default(Expression);
            if (object.ReferenceEquals(TargetPropertyType, RecordFieldType)) {
                //Just assign the unboxed expression
                ConvertedRecordFieldExpression = UnboxedRecordFieldExpression;

            } else if (object.ReferenceEquals(TargetPropertyType, typeof(string))) {
                //There are no casts from primitive types to String.
                //And Expression.Convert Method (Expression, Type, MethodInfo) only works with static methods.
                ConvertedRecordFieldExpression = Expression.Call(UnboxedRecordFieldExpression, RecordFieldType.GetMethod("ToString", Type.EmptyTypes));
            } else {
                //Using Expression.Convert works wherever you can make an explicit or implicit cast.
                //But it casts OR unboxes an object, therefore the double cast. First unbox to the SourceType and then cast to the TargetType
                //It also doesn't convert a numerical type to a String or date, this will throw an exception.
                ConvertedRecordFieldExpression = Expression.Convert(UnboxedRecordFieldExpression, TargetPropertyType);
            }
            return ConvertedRecordFieldExpression;
        }
    }
}
