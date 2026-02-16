using LightORM.Extension;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
namespace LightORM.SqlExecutor;

internal partial class ExpressionBuilder
{
    private static readonly ConcurrentDictionary<string, Delegate> dynamicDelegates = [];

    public static Func<IDataReader, T> BuildDeserializer<T>(DbDataReader reader)
    {
        var type = typeof(T);

        var cacheKey = GenerateCacheKey(type, reader);

        if (!dynamicDelegates.TryGetValue(cacheKey, out var @delegate))
        {
            @delegate = BuildFunc<T>(reader, CultureInfo.CurrentCulture);
            dynamicDelegates.TryAdd(cacheKey, @delegate);
        }

        return (Func<IDataReader, T>)@delegate;

        static string GenerateCacheKey(Type type, DbDataReader reader)
        {
            var schemaTable = reader.GetSchemaTable();
            if (schemaTable == null)
                return $"{type.FullName}_null_schema";

            var builder = new StringBuilder(256);
            builder.Append(type.FullName); // 使用 FullName 而不是 GUID

            foreach (DataRow row in schemaTable.Rows)
            {
                builder.Append('|')
                       .Append(row["ColumnName"])       // 列名
                       .Append(':')
                       .Append(row["DataType"]?.ToString() ?? "null") // 数据类型
                       .Append(':')
                       .Append(row["AllowDBNull"]); // 是否可空
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// reader => {
    ///     return new T {
    ///         Member0 = (memberType)reader.Get_XXX[0],
    ///         Member1 = (memberType)reader.Get_XXX[1],
    ///         Member2 = (memberType)reader.Get_XXX[2],
    ///         Member3 = (memberType)reader.Get_XXX[3],
    ///         Member4 = reader.IsDBNull(4) ? default(memberType) : (memberType)reader.Get_XXX[4],
    ///     }
    /// }
    /// </summary>
    /// <typeparam name="Target"></typeparam>
    /// <param name="reader"></param>
    /// <param name="Culture"></param>
    /// <returns></returns>
    private static Func<IDataReader, Target> BuildFunc<Target>(DbDataReader reader, CultureInfo Culture)
    {
        ParameterExpression recordInstanceExp = Expression.Parameter(typeof(IDataReader), "reader");
        Type TargetType = typeof(Target);
        DataTable SchemaTable = reader.GetSchemaTable() ?? throw new LightOrmException("GetSchemaTable 结果为 null");
        Expression? Body = default;

        // 元组处理
        if (TargetType.FullName?.StartsWith("System.Tuple`") ?? false)
        {
            ConstructorInfo[] Constructors = TargetType.GetConstructors();
            if (Constructors.Length != 1)
            {
                throw new LightOrmException("Tuple must have one Constructor");
            }
            var Constructor = Constructors[0];

            var Parameters = Constructor.GetParameters();
            if (Parameters.Length > 7)
                throw new NotSupportedException("Nested Tuples are not supported");

            Expression[] TargetValueExpressions = new Expression[Parameters.Length];
            for (int Ordinal = 0; Ordinal < Parameters.Length; Ordinal++)
            {
                var ParameterType = Parameters[Ordinal].ParameterType;
                if (Ordinal >= reader.FieldCount)
                {
                    TargetValueExpressions[Ordinal] = Expression.Default(ParameterType);
                }
                else
                {
                    TargetValueExpressions[Ordinal] = GetTargetValueExpression(
                                                    reader,
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
        else if (TargetType.IsElementaryType())
        {
            const int Ordinal = 0;
            Expression TargetValueExpression = GetTargetValueExpression(
                                                    reader,
                                                    Culture,
                                                    recordInstanceExp,
                                                    SchemaTable,
                                                    Ordinal,
                                                    TargetType);

            //UnaryExpression converted = Expression.Convert(TargetValueExpression, TargetType);
            Body = Expression.Block(TargetValueExpression);
        }
        // 其他
        else
        {
            var columns = TableContext.GetTableInfo(TargetType).Columns;
            if (TargetType.IsAnonymous())
            {
                Body = CreateAnonymous();
            }
            else
            {
                Body = CreateCustomEntiry();
            }

            Expression CreateAnonymous()
            {
                var props = TargetType.GetProperties();
                List<Expression> Expressions = [];
                void work()
                {
                    for (int Ordinal = 0; Ordinal < reader.FieldCount; Ordinal++)
                    {
                        var targetMember = props[Ordinal];
                        Expression TargetValueExpression = GetTargetValueExpression(
                                                                reader,
                                                                Culture,
                                                                recordInstanceExp,
                                                                SchemaTable,
                                                                Ordinal,
                                                                targetMember.PropertyType);

                        //Create a binding to the target member
                        Expressions.Add(TargetValueExpression);
                    }
                }
                work();
                var ctor = TargetType.GetConstructors().First();
                return Expression.New(ctor, Expressions);
            }

            Expression CreateCustomEntiry()
            {
                // 属性处理 Property
                List<MemberBinding> Bindings = [];
                // 处理普通属性
                foreach (var col in columns.Where(c => !c.IsNotMapped && !c.IsNavigate && !c.IsAggregated && !c.IsAggregatedProperty))
                {
                    if (!col.CanWrite && !col.CanInit)
                    {
                        continue;
                    }
                    var TargetMember = TargetType.GetProperty(col.PropertyName)!;
                    void work()
                    {
                        for (int Ordinal = 0; Ordinal < reader.FieldCount; Ordinal++)
                        {
                            //Check if the RecordFieldName matches the TargetMember
                            if (MemberMatchesName(col, reader.GetName(Ordinal)))
                            {
                                Expression TargetValueExpression = GetTargetValueExpression(
                                                                        reader,
                                                                        Culture,
                                                                        recordInstanceExp,
                                                                        SchemaTable,
                                                                        Ordinal,
                                                                        TargetMember.PropertyType);

                                //Create a binding to the target member
                                MemberAssignment BindExpression = Expression.Bind(TargetMember, TargetValueExpression);
                                Bindings.Add(BindExpression);
                                return;
                            }
                        }
                    }
                    work();
                }
                // 处理聚合的属性
                ITableColumnInfo[] agTypes = [.. columns.Where(c => c.AggregateType != null && c.IsAggregated)];
                foreach (var ag in agTypes)
                {
                    var flatType = ag.AggregateType!;
                    List<MemberBinding> bindings = [];
                    PropertyInfo targetMember = TargetType.GetProperty(ag.PropertyName)!;
                    foreach (var flatCol in columns.Where(c => c.AggregateType == flatType && c.IsAggregatedProperty))
                    {
                        if (!flatCol.CanWrite && !flatCol.CanInit)
                        {
                            continue;
                        }
                        var TargetMember = flatType.GetProperty(flatCol.PropertyName)!;
                        void work()
                        {
                            for (int Ordinal = 0; Ordinal < reader.FieldCount; Ordinal++)
                            {
                                //Check if the RecordFieldName matches the TargetMember
                                if (MemberMatchesName(flatCol, reader.GetName(Ordinal)))
                                {
                                    Expression TargetValueExpression = GetTargetValueExpression(
                                                                            reader,
                                                                            Culture,
                                                                            recordInstanceExp,
                                                                            SchemaTable,
                                                                            Ordinal,
                                                                            TargetMember.PropertyType);

                                    //Create a binding to the target member
                                    MemberAssignment BindExpression = Expression.Bind(TargetMember, TargetValueExpression);
                                    bindings.Add(BindExpression);
                                    return;
                                }
                            }
                        }
                        work();
                    }
                    var memberInit = Expression.MemberInit(Expression.New(flatType), bindings);
                    Bindings.Add(Expression.Bind(targetMember, memberInit));
                }
                return Expression.MemberInit(Expression.New(TargetType), Bindings);
            }

        }
        //Compile as Delegate
        var lambdaExp = Expression.Lambda<Func<IDataReader, Target>>(Body, recordInstanceExp);
        System.Diagnostics.Debug.WriteLine(lambdaExp);
        return lambdaExp.Compile();
    }

    private static bool MemberMatchesName(ITableColumnInfo col, string Name)
    {
        return string.Equals(col.PropertyName, Name, StringComparison.CurrentCultureIgnoreCase)
            || string.Equals(col.ColumnName, Name, StringComparison.CurrentCultureIgnoreCase);
    }

    private static Expression GetTargetValueExpression(
        DbDataReader reader,
        CultureInfo Culture,
        ParameterExpression recordInstanceExp,
        DataTable SchemaTable,
        int Ordinal,
        Type TargetMemberType)
    {
        Type RecordFieldType = reader.GetFieldType(Ordinal);
        bool AllowDBNull = IsColumnNullable(SchemaTable, Ordinal);
        Expression RecordFieldExpression = GetRecordFieldExpression(recordInstanceExp, Ordinal, RecordFieldType);
        Expression ConvertedRecordFieldExpression = GetConversionExpression(RecordFieldType, RecordFieldExpression, TargetMemberType, Culture);
        MethodCallExpression NullCheckExpression = GetNullCheckExpression(recordInstanceExp, Ordinal);

        Expression TargetValueExpression;
        if (AllowDBNull)
        {
            TargetValueExpression = Expression.Condition(
            NullCheckExpression,
            Expression.Default(TargetMemberType),
            //Nullable.GetUnderlyingType(TargetMemberType) is null ? ConvertedRecordFieldExpression : Expression.Convert(ConvertedRecordFieldExpression, TargetMemberType),
            ConvertedRecordFieldExpression,
            TargetMemberType
            );
        }
        else
        {
            TargetValueExpression = ConvertedRecordFieldExpression;
        }
        return TargetValueExpression;

        static bool IsColumnNullable(DataTable schemaTable, int ordinal)
        {
            var allowDbNull = schemaTable.Rows[ordinal]["AllowDBNull"];
            return allowDbNull == DBNull.Value ||
                   allowDbNull == null ||
                   Convert.ToBoolean(allowDbNull);
        }
    }

    private static Expression GetRecordFieldExpression(ParameterExpression recordInstanceExp, int Ordinal, Type RecordFieldType)
    {
        //MethodInfo GetValueMethod = default(MethodInfo);
        typeMapMethod.TryGetValue(RecordFieldType, out var GetValueMethod);
        if (GetValueMethod == null)
            GetValueMethod = DataRecord_GetValue;

        Expression RecordFieldExpression;
        if (ReferenceEquals(RecordFieldType, typeof(byte[])))
        {
            RecordFieldExpression = Expression.Call(GetValueMethod, [recordInstanceExp, Expression.Constant(Ordinal, typeof(int))]);
        }
        else if (IsUnsignType(RecordFieldType))
        {
            RecordFieldExpression = Expression.Call(
                GetValueMethod,
                [recordInstanceExp, Expression.Constant(Ordinal, typeof(int))]
            );
        }
        else
        {
            RecordFieldExpression = Expression.Call(recordInstanceExp, GetValueMethod, Expression.Constant(Ordinal, typeof(int)));
        }
        return RecordFieldExpression;
        static bool IsUnsignType(Type type)
        {
            return type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);
        }
    }


    private static MethodCallExpression GetNullCheckExpression(ParameterExpression RecordInstance, int Ordinal)
    {
        MethodCallExpression NullCheckExpression = Expression.Call(RecordInstance, DataRecord_IsDBNull, Expression.Constant(Ordinal, typeof(int)));
        return NullCheckExpression;
    }

    private static Expression GetConversionExpression(Type SourceType, Expression SourceExpression, Type TargetType, CultureInfo Culture)
    {
        Expression TargetExpression;
        var (underlying, isnullable) = GetUnderlyingType(TargetType);
        var converted = false;
        if (TargetType == SourceType || underlying == SourceType)
        {
            TargetExpression = SourceExpression;
        }
        else if (SourceType == typeof(string))
        {
            TargetExpression = GetParseExpression(SourceExpression, TargetType, Culture);
        }
        else if (TargetType == typeof(string))
        {
            TargetExpression = Expression.Call(SourceExpression, SourceType.GetMethod("ToString", Type.EmptyTypes)!);
        }
        else if (TargetType == typeof(bool) || underlying == typeof(bool))
        {
            MethodInfo ToBooleanMethod = typeof(Convert).GetMethod("ToBoolean", [SourceType])!;
            TargetExpression = Expression.Call(ToBooleanMethod, SourceExpression);
        }
        else if (SourceType == typeof(byte[]))
        {
            TargetExpression = GetArrayHandlerExpression(SourceExpression, TargetType);
        }
        else
        {
            TargetExpression = Expression.Convert(SourceExpression, TargetType);
            converted = true;
        }
        if (isnullable && !converted)
        {
            TargetExpression = Expression.Convert(TargetExpression, TargetType);
        }
        return TargetExpression;
    }

    private static Expression GetArrayHandlerExpression(Expression sourceExpression, Type targetType)
    {
        Expression TargetExpression;
        if (targetType == typeof(byte[]))
        {
            TargetExpression = sourceExpression;
        }
        else if (targetType == typeof(MemoryStream))
        {
            ConstructorInfo ConstructorInfo = targetType.GetConstructor([typeof(byte[])])!;
            TargetExpression = Expression.New(ConstructorInfo, sourceExpression);
        }
        else
        {
            throw new LightOrmException("Cannot convert a byte array to " + targetType.Name);
        }
        return TargetExpression;
    }
    private static Expression GetParseExpression(Expression SourceExpression, Type TargetType, CultureInfo Culture)
    {
        var (UnderlyingType, _) = GetUnderlyingType(TargetType);
        if (UnderlyingType.IsEnum)
        {
            MethodCallExpression ParsedEnumExpression = GetEnumParseExpression(SourceExpression, UnderlyingType);
            //Enum.Parse returns an object that needs to be unboxed
            return Expression.Convert(ParsedEnumExpression, TargetType);
        }
        else
        {
            Expression ParseExpression;
            switch (UnderlyingType.FullName)
            {
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
                    ParseExpression = TryParseStringToBoolean(SourceExpression, UnderlyingType);
                    break;
                case "System.Char":
                    ParseExpression = GetGenericParseExpression(SourceExpression, UnderlyingType);
                    break;
                default:
                    throw new LightOrmException(string.Format("Conversion from {0} to {1} is not supported", "String", TargetType));
            }
            if (Nullable.GetUnderlyingType(TargetType) == null)
            {
                return ParseExpression;
            }
            else
            {
                //Convert to nullable if necessary
                return Expression.Convert(ParseExpression, TargetType);
            }
        }
        Expression GetGenericParseExpression(Expression sourceExpression, Type type)
        {
            MethodInfo ParseMetod = type.GetMethod("Parse", [typeof(string)])!;
            MethodCallExpression CallExpression = Expression.Call(ParseMetod, [sourceExpression]);
            return CallExpression;
        }
        Expression GetDateTimeParseExpression(Expression sourceExpression, Type type, CultureInfo culture)
        {
            MethodInfo ParseMetod = type.GetMethod("Parse", [typeof(string), typeof(DateTimeFormatInfo)])!;
            ConstantExpression ProviderExpression = Expression.Constant(culture.DateTimeFormat, typeof(DateTimeFormatInfo));
            MethodCallExpression CallExpression = Expression.Call(ParseMetod, [sourceExpression, ProviderExpression]);
            return CallExpression;
        }

        MethodCallExpression GetEnumParseExpression(Expression sourceExpression, Type type)
        {
            //Get the MethodInfo for parsing an Enum
            //MethodInfo EnumParseMethod = typeof(Enum).GetMethod("Parse", [typeof(Type), typeof(string), typeof(bool)])!;
            ConstantExpression TargetMemberTypeExpression = Expression.Constant(type);
            ConstantExpression IgnoreCase = Expression.Constant(true, typeof(bool));
            //Create an expression the calls the Parse method
            MethodCallExpression CallExpression = Expression.Call(enumParseMethod, [TargetMemberTypeExpression, sourceExpression, IgnoreCase]);
            return CallExpression;
        }

        MethodCallExpression GetNumberParseExpression(Expression sourceExpression, Type type, CultureInfo culture)
        {
            MethodInfo ParseMetod = type.GetMethod("Parse", [typeof(string), typeof(NumberFormatInfo)])!;
            ConstantExpression ProviderExpression = Expression.Constant(culture.NumberFormat, typeof(NumberFormatInfo));
            MethodCallExpression CallExpression = Expression.Call(ParseMetod, [sourceExpression, ProviderExpression]);
            return CallExpression;
        }
        Expression TryParseStringToBoolean(Expression sourceExpression, Type type)
        {
            var valueExpression = Expression.Call(CustomStringParseToBoolean, [sourceExpression]);
            return GetGenericParseExpression(valueExpression, type);
        }
    }

    private static (Type Type, bool IsNullable) GetUnderlyingType(Type targetType)
    {
        var t = Nullable.GetUnderlyingType(targetType);
        return (t ?? targetType, t is not null);
    }
}

file static class Ex
{
    readonly static HashSet<Type> ElementaryTypes = LoadElementaryTypes();
    /// <summary>
    /// 检查是否为基础类型
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static bool IsElementaryType(this Type t)
    {
        return ElementaryTypes.Contains(t);
    }
    private static HashSet<Type> LoadElementaryTypes()
    {
        HashSet<Type> TypeSet =
        [
                typeof(string),
                typeof(byte),
                typeof(byte[]),
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
            ];
        return TypeSet;
    }
}
