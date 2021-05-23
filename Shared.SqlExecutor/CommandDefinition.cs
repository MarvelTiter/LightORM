using MDbContext.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    internal struct CommandDefinition {
        private static Dictionary<Type, Action<IDbCommand>> commandInitCache = new Dictionary<Type, Action<IDbCommand>>();

        public string CommandText { get; }

        public object Parameters { get; }

        public CommandType? CommandType { get; }

        public IDbTransaction Transaction { get; set; }

        public CommandDefinition(string commandText, object parameters = null, IDbTransaction trans = null, CommandType? commandType = null) {
            CommandText = commandText;
            Parameters = parameters;
            CommandType = commandType;
            Transaction = trans;
        }

        private CommandDefinition(object parameters) {
            this = default(CommandDefinition);
            Parameters = parameters;
        }

        internal IDbCommand SetupCommand(IDbConnection cnn, Action<IDbCommand, object> paramReader) {
            IDbCommand dbCommand = cnn.CreateCommand();

            GetInit(dbCommand.GetType())?.Invoke(dbCommand);

            if (Transaction != null)
                dbCommand.Transaction = Transaction;

            dbCommand.CommandText = CommandText;
            if (CommandType.HasValue)
                dbCommand.CommandType = CommandType.Value;

            paramReader?.Invoke(dbCommand, Parameters);
            return dbCommand;
        }

        private static Action<IDbCommand> GetInit(Type commandType) {
            if (commandType == null) {
                return null;
            }

            if (commandInitCache.TryGet(commandType, out Action<IDbCommand> value)) {
                return value;
            }

            MethodInfo basicPropertySetter = GetBasicPropertySetter(commandType, "BindByName", typeof(bool));
            MethodInfo basicPropertySetter2 = GetBasicPropertySetter(commandType, "InitialLONGFetchSize", typeof(int));
            value = null;
            if (basicPropertySetter != null || basicPropertySetter2 != null) {
                DynamicMethod dynamicMethod = new DynamicMethod(commandType.Name + "_init", null, new Type[1]
                {
                    typeof(IDbCommand)
                });
                ILGenerator iLGenerator = dynamicMethod.GetILGenerator();

                if (basicPropertySetter != null) {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Castclass, commandType);
                    iLGenerator.Emit(OpCodes.Ldc_I4_1);
                    iLGenerator.EmitCall(OpCodes.Callvirt, basicPropertySetter, null);
                }

                if (basicPropertySetter2 != null) {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Castclass, commandType);
                    iLGenerator.Emit(OpCodes.Ldc_I4_M1);
                    iLGenerator.EmitCall(OpCodes.Callvirt, basicPropertySetter2, null);
                }

                iLGenerator.Emit(OpCodes.Ret);
                value = (Action<IDbCommand>)dynamicMethod.CreateDelegate(typeof(Action<IDbCommand>));
            }

            commandInitCache.TryAdd(commandType, value);
            return value;
        }

        private static MethodInfo GetBasicPropertySetter(Type declaringType, string name, Type expectedType) {
            PropertyInfo property = declaringType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if ((object)property != null && property.CanWrite && property.PropertyType == expectedType && property.GetIndexParameters().Length == 0) {
                return property.GetSetMethod();
            }
            return null;
        }
    }
}
