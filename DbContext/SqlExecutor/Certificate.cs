using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    public class Certificate : IEquatable<Certificate> {

        public Certificate(string commandText, CommandType? commandType, IDbConnection conn, Type targetType, Type parameterType) {
            CommandText = commandText;
            CommandType = commandType;
            Connection = conn;
            TargetType = targetType;
            ParameterType = parameterType;
        }

        public string CommandText { get; }
        public CommandType? CommandType { get; }
        public IDbConnection Connection { get; }
        public Type TargetType { get; }
        public Type ParameterType { get; }

        public bool Equals(Certificate other) {
            if (this == other) {
                return true;
            }

            if (other == null) {
                return false;
            }
            if (CommandText == other.CommandText
                && CommandType == other.CommandType
                && TargetType == other.TargetType
                && Connection?.ConnectionString == other.Connection?.ConnectionString
                && ParameterType == other.ParameterType)
                return true;
            return false;
        }
    }
}
