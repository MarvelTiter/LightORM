using MDbContext.SqlExecutor.Service;
using MDbEntity.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        private static readonly MethodInfo Helper_GetBytes = typeof(ExpressionBuilder).GetMethod("RecordFieldToBytes", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo DataRecord_GetFieldType = typeof(IDataRecord).GetMethod("GetFieldType", new Type[] { typeof(int) });

        private static readonly MethodInfo DataRecord_GetByte = typeof(IDataRecord).GetMethod("GetByte", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetInt16 = typeof(IDataRecord).GetMethod("GetInt16", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetInt32 = typeof(IDataRecord).GetMethod("GetInt32", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetInt64 = typeof(IDataRecord).GetMethod("GetInt64", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetFloat = typeof(IDataRecord).GetMethod("GetFloat", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetDouble = typeof(IDataRecord).GetMethod("GetDouble", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetDecimal = typeof(IDataRecord).GetMethod("GetDecimal", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetBoolean = typeof(IDataRecord).GetMethod("GetBoolean", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetString = typeof(IDataRecord).GetMethod("GetString", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetChar = typeof(IDataRecord).GetMethod("GetChar", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetGuid = typeof(IDataRecord).GetMethod("GetGuid", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetDateTime = typeof(IDataRecord).GetMethod("GetDateTime", new Type[] { typeof(int) });

        private static readonly MethodInfo DataRecord_IsDBNull = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetValue = typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) });

        readonly static Dictionary<Type, MethodInfo> typeMapMethod = new Dictionary<Type, MethodInfo>(37) {
            [typeof(byte)] = DataRecord_GetByte,
            [typeof(sbyte)] = DataRecord_GetByte,
            [typeof(short)] = DataRecord_GetInt16,
            [typeof(ushort)] = DataRecord_GetInt16,
            [typeof(int)] = DataRecord_GetInt32,
            [typeof(uint)] = DataRecord_GetInt32,
            [typeof(long)] = DataRecord_GetInt64,
            [typeof(ulong)] = DataRecord_GetInt64,
            [typeof(float)] = DataRecord_GetFloat,
            [typeof(double)] = DataRecord_GetDouble,
            [typeof(decimal)] = DataRecord_GetDecimal,
            [typeof(bool)] = DataRecord_GetBoolean,
            [typeof(string)] = DataRecord_GetString,
            [typeof(char)] = DataRecord_GetChar,
            [typeof(Guid)] = DataRecord_GetGuid,
            [typeof(DateTime)] = DataRecord_GetDateTime,
            [typeof(byte[])] = Helper_GetBytes
        };
        private static byte[] RecordFieldToBytes(IDataRecord Reader, int Column) {
            long BlobSize = Reader.GetBytes(Column, 0, null, 0, 0);
            if (BlobSize > int.MaxValue)
                throw new ArgumentOutOfRangeException("MemoryStream cannot be larger than " + int.MaxValue);

            byte[] Buffer = new byte[Convert.ToInt32(BlobSize - 1) + 1];
            Reader.GetBytes(Column, 0, Buffer, 0, Buffer.Length);
            return Buffer;
        }
        public Func<IDataReader, object> BuildDeserializer<T>(IDataReader reader) {
            return GetInstanceCreator<T>(reader, CultureInfo.CurrentCulture, false);
        }

        /// <summary>
        /// record => {
        ///     return new T {
        ///         Member0 = (memberType)record.Get_XXX[0],
        ///         Member1 = (memberType)record.Get_XXX[1],
        ///         Member2 = (memberType)record.Get_XXX[2],
        ///         Member3 = (memberType)record.Get_XXX[3],
        ///         Member4 = record.IsDBNull(4) ? default(memberType) : (memberType)record.Get_XXX[4],
        ///     }
        /// }
        /// </summary>
        /// <typeparam name="Target"></typeparam>
        /// <param name="RecordInstance"></param>
        /// <param name="Culture"></param>
        /// <param name="MustMapAllProperties"></param>
        /// <returns></returns>
        private Func<IDataRecord, object> GetInstanceCreator<Target>(IDataRecord RecordInstance, CultureInfo Culture, bool MustMapAllProperties) {
            ParameterExpression recordInstanceExp = Expression.Parameter(typeof(IDataRecord), "Record");
            Type TargetType = typeof(Target);
            DataTable SchemaTable = ((IDataReader)RecordInstance).GetSchemaTable();
            Expression Body = default(Expression);

            // 元组处理
            if (TargetType.FullName.StartsWith("System.Tuple`")) {
                ConstructorInfo[] Constructors = TargetType.GetConstructors();
                if (Constructors.Count() != 1)
                    throw new ArgumentException("Tuple must have one Constructor");
                var Constructor = Constructors[0];

                var Parameters = Constructor.GetParameters();
                if (Parameters.Length > 7)
                    throw new NotSupportedException("Nested Tuples are not supported");

                Expression[] TargetValueExpressions = new Expression[Parameters.Length];
                for (int Ordinal = 0; Ordinal < Parameters.Length; Ordinal++) {
                    var ParameterType = Parameters[Ordinal].ParameterType;
                    if (Ordinal >= RecordInstance.FieldCount) {
                        if (MustMapAllProperties) { throw new ArgumentException("Tuple has more fields than the DataReader"); }
                        TargetValueExpressions[Ordinal] = Expression.Default(ParameterType);
                    } else {
                        TargetValueExpressions[Ordinal] = GetTargetValueExpression(
                                                        RecordInstance,
                                                        Culture,
                                                        recordInstanceExp,
                                                        SchemaTable,
                                                        Ordinal,
                                                        ParameterType);
                    }
                }
                Body = Expression.New(Constructor, TargetValueExpressions);
            }
            // 基础类型处理 eg: IEnumable<int>  IEnumable<string>
            else if (TargetType.IsElementaryType()) {
                const int Ordinal = 0;
                Expression TargetValueExpression = GetTargetValueExpression(
                                                        RecordInstance,
                                                        Culture,
                                                        recordInstanceExp,
                                                        SchemaTable,
                                                        Ordinal,
                                                        TargetType);

                ParameterExpression TargetExpression = Expression.Variable(TargetType, "Target");
                Expression AssignExpression = Expression.Assign(TargetExpression, TargetValueExpression);
                Body = Expression.Block(new ParameterExpression[] { TargetExpression }, AssignExpression);
            }
            // 其他
            else {                
                SortedDictionary<int, MemberBinding> Bindings = new SortedDictionary<int, MemberBinding>();
                // 字段处理 Field
                foreach (FieldInfo TargetMember in TargetType.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                    Action work = delegate {
                        for (int Ordinal = 0; Ordinal < RecordInstance.FieldCount; Ordinal++) {
                            //Check if the RecordFieldName matches the TargetMember
                            if (MemberMatchesName(TargetMember, RecordInstance.GetName(Ordinal))) {
                                Expression TargetValueExpression = GetTargetValueExpression(
                                                                        RecordInstance,
                                                                        Culture,
                                                                        recordInstanceExp,
                                                                        SchemaTable,
                                                                        Ordinal,
                                                                        TargetMember.FieldType);

                                //Create a binding to the target member
                                MemberAssignment BindExpression = Expression.Bind(TargetMember, TargetValueExpression);
                                Bindings.Add(Ordinal, BindExpression);
                                return;
                            }
                        }
                        //If we reach this code the targetmember did not get mapped
                        if (MustMapAllProperties) {
                            throw new ArgumentException(String.Format("TargetField {0} is not matched by any field in the DataReader", TargetMember.Name));
                        }
                    };
                    work();
                }
                // 属性处理 Property
                foreach (PropertyInfo TargetMember in TargetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                    if (TargetMember.CanWrite) {
                        Action work = delegate {
                            for (int Ordinal = 0; Ordinal < RecordInstance.FieldCount; Ordinal++) {
                                //Check if the RecordFieldName matches the TargetMember
                                if (MemberMatchesName(TargetMember, RecordInstance.GetName(Ordinal))) {
                                    Expression TargetValueExpression = GetTargetValueExpression(
                                                                            RecordInstance,
                                                                            Culture,
                                                                            recordInstanceExp,
                                                                            SchemaTable,
                                                                            Ordinal,
                                                                            TargetMember.PropertyType);

                                    //Create a binding to the target member
                                    MemberAssignment BindExpression = Expression.Bind(TargetMember, TargetValueExpression);
                                    Bindings.Add(Ordinal, BindExpression);
                                    return;
                                }
                            }
                            //If we reach this code the targetmember did not get mapped
                            if (MustMapAllProperties) {
                                throw new ArgumentException(String.Format("TargetProperty {0} is not matched by any Field in the DataReader", TargetMember.Name));
                            }
                        };
                        work();
                    }
                }

                Body = Expression.MemberInit(Expression.New(TargetType), Bindings.Values);
            }
            //Compile as Delegate
            return Expression.Lambda<Func<IDataRecord, object>>(Body, recordInstanceExp).Compile();
        }

        private static bool MemberMatchesName(MemberInfo Member, string Name) {
            string FieldnameAttribute = GetColumnNameAttribute();
            return FieldnameAttribute.ToLower() == Name.ToLower() || Member.Name.ToLower() == Name.ToLower();

            string GetColumnNameAttribute() {
                if (Member.GetCustomAttributes(typeof(ColumnNameAttribute), true).Count() > 0) {
                    return ((ColumnNameAttribute)Member.GetCustomAttributes(typeof(ColumnNameAttribute), true)[0]).Name;
                } else {
                    return string.Empty;
                }
            }
        }

        private static Expression GetTargetValueExpression(
            IDataRecord RecordInstance,
            CultureInfo Culture,
            ParameterExpression recordInstanceExp,
            DataTable SchemaTable,
            int Ordinal,
            Type TargetMemberType) {
            Type RecordFieldType = RecordInstance.GetFieldType(Ordinal);
            bool AllowDBNull = Convert.ToBoolean(SchemaTable.Rows[Ordinal]["AllowDBNull"]);
            Expression RecordFieldExpression = GetRecordFieldExpression(recordInstanceExp, Ordinal, RecordFieldType);
            Expression ConvertedRecordFieldExpression = GetConversionExpression(RecordFieldType, RecordFieldExpression, TargetMemberType, Culture);
            MethodCallExpression NullCheckExpression = GetNullCheckExpression(recordInstanceExp, Ordinal);

            //Create an expression that assigns the converted value to the target
            Expression TargetValueExpression = default(Expression);
            if (AllowDBNull) {
                TargetValueExpression = Expression.Condition(
                    NullCheckExpression,
                    Expression.Default(TargetMemberType),
                    ConvertedRecordFieldExpression,
                    TargetMemberType
                    );
            } else {
                TargetValueExpression = ConvertedRecordFieldExpression;
            }
            return TargetValueExpression;
        }

        private static Expression GetRecordFieldExpression(ParameterExpression recordInstanceExp, int Ordinal, Type RecordFieldType) {
            //MethodInfo GetValueMethod = default(MethodInfo);
            typeMapMethod.TryGetValue(RecordFieldType, out var GetValueMethod);
            if (GetValueMethod == null)
                GetValueMethod = DataRecord_GetValue;

            Expression RecordFieldExpression;
            if (object.ReferenceEquals(RecordFieldType, typeof(byte[]))) {
                RecordFieldExpression = Expression.Call(GetValueMethod, new Expression[] { recordInstanceExp, Expression.Constant(Ordinal, typeof(int)) });
            } else {
                RecordFieldExpression = Expression.Call(recordInstanceExp, GetValueMethod, Expression.Constant(Ordinal, typeof(int)));
            }
            return RecordFieldExpression;
        }

        private static MethodCallExpression GetNullCheckExpression(ParameterExpression RecordInstance, int Ordinal) {
            MethodCallExpression NullCheckExpression = Expression.Call(RecordInstance, DataRecord_IsDBNull, Expression.Constant(Ordinal, typeof(int)));
            return NullCheckExpression;
        }

        private static Expression GetConversionExpression(Type SourceType, Expression SourceExpression, Type TargetType, CultureInfo Culture) {
            Expression TargetExpression;
            if (ReferenceEquals(TargetType, SourceType)) {
                TargetExpression = SourceExpression;
            } else if (ReferenceEquals(SourceType, typeof(string))) {
                TargetExpression = GetParseExpression(SourceExpression, TargetType, Culture);
            } else if (ReferenceEquals(TargetType, typeof(string))) {
                TargetExpression = Expression.Call(SourceExpression, SourceType.GetMethod("ToString", Type.EmptyTypes));
            } else if (ReferenceEquals(TargetType, typeof(bool))) {
                MethodInfo ToBooleanMethod = typeof(Convert).GetMethod("ToBoolean", new[] { SourceType });
                TargetExpression = Expression.Call(ToBooleanMethod, SourceExpression);
            } else if (ReferenceEquals(SourceType, typeof(Byte[]))) {
                TargetExpression = GetArrayHandlerExpression(SourceExpression, TargetType);
            } else {
                TargetExpression = Expression.Convert(SourceExpression, TargetType);
            }
            return TargetExpression;
        }

        private static Expression GetArrayHandlerExpression(Expression sourceExpression, Type targetType) {
            Expression TargetExpression = default(Expression);
            if (object.ReferenceEquals(targetType, typeof(byte[]))) {
                TargetExpression = sourceExpression;
            } else if (object.ReferenceEquals(targetType, typeof(MemoryStream))) {
                ConstructorInfo ConstructorInfo = targetType.GetConstructor(new[] { typeof(byte[]) });
                TargetExpression = Expression.New(ConstructorInfo, sourceExpression);
            } else {
                throw new ArgumentException("Cannot convert a byte array to " + targetType.Name);
            }
            return TargetExpression;
        }

        private static Expression GetParseExpression(Expression SourceExpression, Type TargetType, CultureInfo Culture) {
            Type UnderlyingType = GetUnderlyingType(TargetType);
            if (UnderlyingType.IsEnum) {
                MethodCallExpression ParsedEnumExpression = GetEnumParseExpression(SourceExpression, UnderlyingType);
                //Enum.Parse returns an object that needs to be unboxed
                return Expression.Unbox(ParsedEnumExpression, TargetType);
            } else {
                Expression ParseExpression = default(Expression);
                switch (UnderlyingType.FullName) {
                    case "System.Byte":
                    case "System.UInt16":
                    case "System.UInt32":
                    case "System.UInt64":
                    case "System.SByte":
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                    case "System.Double":
                    case "System.Decimal":
                        ParseExpression = GetNumberParseExpression(SourceExpression, UnderlyingType, Culture);
                        break;
                    case "System.DateTime":
                        ParseExpression = GetDateTimeParseExpression(SourceExpression, UnderlyingType, Culture);
                        break;
                    case "System.Boolean":
                    case "System.Char":
                        ParseExpression = GetGenericParseExpression(SourceExpression, UnderlyingType);
                        break;
                    default:
                        throw new ArgumentException(string.Format("Conversion from {0} to {1} is not supported", "String", TargetType));
                }
                if (Nullable.GetUnderlyingType(TargetType) == null) {
                    return ParseExpression;
                } else {
                    //Convert to nullable if necessary
                    return Expression.Convert(ParseExpression, TargetType);
                }
            }
            Expression GetGenericParseExpression(Expression sourceExpression, Type type) {
                MethodInfo ParseMetod = type.GetMethod("Parse", new[] { typeof(string) });
                MethodCallExpression CallExpression = Expression.Call(ParseMetod, new[] { sourceExpression });
                return CallExpression;
            }
            Expression GetDateTimeParseExpression(Expression sourceExpression, Type type, CultureInfo culture) {
                MethodInfo ParseMetod = type.GetMethod("Parse", new[] { typeof(string), typeof(DateTimeFormatInfo) });
                ConstantExpression ProviderExpression = Expression.Constant(culture.DateTimeFormat, typeof(DateTimeFormatInfo));
                MethodCallExpression CallExpression = Expression.Call(ParseMetod, new[] { sourceExpression, ProviderExpression });
                return CallExpression;
            }

            MethodCallExpression GetEnumParseExpression(Expression sourceExpression, Type type) {
                //Get the MethodInfo for parsing an Enum
                MethodInfo EnumParseMethod = typeof(Enum).GetMethod("Parse", new[] { typeof(Type), typeof(string), typeof(bool) });
                ConstantExpression TargetMemberTypeExpression = Expression.Constant(type);
                ConstantExpression IgnoreCase = Expression.Constant(true, typeof(bool));
                //Create an expression the calls the Parse method
                MethodCallExpression CallExpression = Expression.Call(EnumParseMethod, new[] { TargetMemberTypeExpression, sourceExpression, IgnoreCase });
                return CallExpression;
            }

            MethodCallExpression GetNumberParseExpression(Expression sourceExpression, Type type, CultureInfo culture) {
                MethodInfo ParseMetod = type.GetMethod("Parse", new[] { typeof(string), typeof(NumberFormatInfo) });
                ConstantExpression ProviderExpression = Expression.Constant(culture.NumberFormat, typeof(NumberFormatInfo));
                MethodCallExpression CallExpression = Expression.Call(ParseMetod, new[] { sourceExpression, ProviderExpression });
                return CallExpression;
            }
        }

        private static Type GetUnderlyingType(Type targetType) {
            return Nullable.GetUnderlyingType(targetType) ?? targetType;
        }
    }
    public static class Ex {
        /// <summary>
        /// 检查是否为基础类型
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsElementaryType(this Type t) {
            return ElementaryTypes.Contains(t);
        }
        readonly static HashSet<Type> ElementaryTypes = LoadElementaryTypes();
        private static HashSet<Type> LoadElementaryTypes() {
            HashSet<Type> TypeSet = new HashSet<Type> {
                    typeof(string),
                    typeof(byte),
                    typeof(sbyte),
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(ushort),
                    typeof(uint),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(Guid),
                    typeof(bool),
                    typeof(TimeSpan),
                    typeof(byte?),
                    typeof(sbyte?),
                    typeof(short?),
                    typeof(int?),
                    typeof(long?),
                    typeof(ushort?),
                    typeof(uint?),
                    typeof(ulong?),
                    typeof(float?),
                    typeof(double?),
                    typeof(decimal?),
                    typeof(DateTime?),
                    typeof(Guid?),
                    typeof(bool?),
                    typeof(TimeSpan?)
                };
            return TypeSet;
        }
    }
}
