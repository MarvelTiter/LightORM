﻿
/* 项目“LightORM (netcoreapp3.1)”的未合并的更改
在此之前:
using MDbContext.Extension;
在此之后:
using LightORM.SqlExecutor;
using MDbContext;
using MDbContext.Extension;
*/

/* 项目“LightORM (net7.0)”的未合并的更改
在此之前:
using MDbContext.Extension;
在此之后:
using LightORM.SqlExecutor;
using MDbContext;
using MDbContext.Extension;
*/

/* 项目“LightORM (net4.0)”的未合并的更改
在此之前:
using MDbContext.Extension;
在此之后:
using LightORM.SqlExecutor;
using MDbContext;
using MDbContext.Extension;
*/

/* 项目“LightORM (net6.0)”的未合并的更改
在此之前:
using MDbContext.Extension;
在此之后:
using LightORM.SqlExecutor;
using MDbContext;
using MDbContext.Extension;
*/

/* 项目“LightORM (net45)”的未合并的更改
在此之前:
using MDbContext.Extension;
在此之后:
using LightORM.SqlExecutor;
using MDbContext;
using MDbContext.Extension;
*/

/* 项目“LightORM (net8.0)”的未合并的更改
在此之前:
using MDbContext.Extension;
在此之后:
using LightORM.SqlExecutor;
using MDbContext;
using MDbContext.Extension;
*/

/* 项目“LightORM (net4.8)”的未合并的更改
在此之前:
using MDbContext.Extension;
在此之后:
using LightORM.SqlExecutor;
using MDbContext;
using MDbContext.Extension;
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace LightORM.SqlExecutor;

internal struct CommandDefinition
{
    private static Dictionary<Type, Action<IDbCommand>> commandInitCache = new Dictionary<Type, Action<IDbCommand>>();

    public string CommandText { get; }

    public object? Parameters { get; }

    public CommandType? CommandType { get; }

    public DbTransaction? Transaction { get; }

    public DbConnection Connection { get; }

    public CommandDefinition(DbConnection connection, string commandText, object? parameters = null, DbTransaction? trans = null, CommandType? commandType = null)
    {
        Connection = connection;
        CommandText = commandText;
        Parameters = parameters;
        CommandType = commandType;
        Transaction = trans;
    }

    internal DbCommand PrepareCommand(Action<IDbCommand, object?>? paramReader)
    {
        DbCommand dbCommand = Connection.CreateCommand();

        GetInit(dbCommand.GetType())?.Invoke(dbCommand);
        if (Transaction != null)
            dbCommand.Transaction = Transaction;

        dbCommand.CommandText = CommandText;
        if (CommandType.HasValue)
            dbCommand.CommandType = CommandType.Value;

        paramReader?.Invoke(dbCommand, Parameters);
        return dbCommand;
    }

    private static Action<IDbCommand>? GetInit(Type commandType)
    {
        if (commandType == null)
        {
            return null;
        }

        if (commandInitCache.TryGetValue(commandType, out Action<IDbCommand> value))
        {
            return value;
        }

        MethodInfo? basicPropertySetter = GetBasicPropertySetter(commandType, "BindByName", typeof(bool));
        MethodInfo? basicPropertySetter2 = GetBasicPropertySetter(commandType, "InitialLONGFetchSize", typeof(int));
        if (basicPropertySetter != null || basicPropertySetter2 != null)
        {
#if NETSTANDARD2_0_OR_GREATER
            /*
             * (IDbCommand cmd) => {
             *     (OracleCommand)cmd.set_BindByName(true);
             *     (OracleCommand)cmd.set_InitialLONGFetchSize(-1);
             * }
             */
            ParameterExpression cmdExp = Expression.Parameter(typeof(IDbCommand), "cmd");
            List<Expression> body = new List<Expression>();
            if (basicPropertySetter != null)
            {
                UnaryExpression convertedCmdExp = Expression.Convert(cmdExp, commandType);
                MethodCallExpression setter1Exp = Expression.Call(convertedCmdExp, basicPropertySetter, Expression.Constant(true, typeof(bool)));
                body.Add(setter1Exp);
            }
            if (basicPropertySetter2 != null)
            {
                UnaryExpression convertedCmdExp = Expression.Convert(cmdExp, commandType);
                MethodCallExpression setter2Exp = Expression.Call(convertedCmdExp, basicPropertySetter2, Expression.Constant(-1, typeof(int)));
                body.Add(setter2Exp);
            }
            var lambda = Expression.Lambda<Action<IDbCommand>>(Expression.Block(body), cmdExp);
            value = lambda.Compile();
#else
            DynamicMethod dynamicMethod = new DynamicMethod(commandType.Name + "_init", null, new Type[1]
            {
                typeof(IDbCommand)
            });
            ILGenerator iLGenerator = dynamicMethod.GetILGenerator();

            if (basicPropertySetter != null)
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Castclass, commandType);
                iLGenerator.Emit(OpCodes.Ldc_I4_1);
                iLGenerator.EmitCall(OpCodes.Callvirt, basicPropertySetter, null);
            }

            if (basicPropertySetter2 != null)
            {
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Castclass, commandType);
                iLGenerator.Emit(OpCodes.Ldc_I4_M1);
                iLGenerator.EmitCall(OpCodes.Callvirt, basicPropertySetter2, null);
            }

            iLGenerator.Emit(OpCodes.Ret);
            value = (Action<IDbCommand>)dynamicMethod.CreateDelegate(typeof(Action<IDbCommand>));
#endif
            commandInitCache.Add(commandType, value);
        }

        return value;
    }

    private static MethodInfo? GetBasicPropertySetter(Type declaringType, string name, Type expectedType)
    {
        PropertyInfo property = declaringType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if ((object)property != null && property.CanWrite && property.PropertyType == expectedType && property.GetIndexParameters().Length == 0)
        {
            return property.GetSetMethod();
        }
        return null;
    }
}
