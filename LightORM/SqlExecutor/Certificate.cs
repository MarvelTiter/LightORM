using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDbContext.SqlExecutor {
    public class Certificate : IEquatable<Certificate> {

        public Certificate(string commandText, CommandType? commandType, IDbConnection conn, Type targetType, Type parameterType) {
            Sql = commandText;
            CommandType = commandType;
            Connection = conn;
            TargetType = targetType;
            ParameterType = parameterType;
        }

        public string Sql { get; }
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
            if (Sql == other.Sql
                && CommandType == other.CommandType
                && TargetType == other.TargetType
                && Connection?.ConnectionString == other.Connection?.ConnectionString
                && ParameterType == other.ParameterType)
                return true;
            return false;
        }

        public override int GetHashCode() {
            var h1 = Sql.GetHashCode();
            var h2 = CommandType.GetHashCode();
            var h3 = Connection.GetHashCode();
            var h4 = TargetType.GetHashCode();
            var h5 = ParameterType is null ? h1 : ParameterType.GetHashCode();
            var code =
                L(h1, 1) |
                R(h2, 27) |
                R(h3, 27) |
                L(h4, 2) |
                L(h5, 3);
            return (int)code;
        }

        private uint L(int num, int step) {
            return ((uint)num) << step;
        }
        private uint R(int num, int step) {
            return ((uint)num) >> step;
        }

    }
}
