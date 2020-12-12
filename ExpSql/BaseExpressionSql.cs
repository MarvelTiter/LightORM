using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DExpSql {
    internal class BaseExpressionSql<T> : IExpressionSql where T : Expression {
        private string _expressionType => typeof(T).Name;
        public SqlCaluse Update(Expression exp, SqlCaluse sqlCaluse) => Update((T)exp, sqlCaluse);
        protected virtual SqlCaluse Update(T exp, SqlCaluse sqlCaluse) =>
            throw new NotImplementedException($"[{_expressionType}] 未实现 Update");

        public SqlCaluse PrimaryKey(Expression exp, SqlCaluse sqlCaluse) => PrimaryKey((T)exp, sqlCaluse);
        protected virtual SqlCaluse PrimaryKey(T exp, SqlCaluse sqlCaluse) =>
            throw new NotImplementedException($"[{_expressionType}] 未实现 PrimaryKey");

        public SqlCaluse Select(Expression exp, SqlCaluse sqlCaluse) => Select((T)exp, sqlCaluse);
        protected virtual SqlCaluse Select(T exp, SqlCaluse sqlCaluse) =>
            throw new NotImplementedException($"[{_expressionType}] 未实现 Select");

        public SqlCaluse SelectMethod(Expression exp, SqlCaluse sqlCaluse) => SelectMethod((T)exp, sqlCaluse);
        protected virtual SqlCaluse SelectMethod(T exp, SqlCaluse sqlCaluse) =>
            throw new NotImplementedException($"[{_expressionType}] 未实现 SelectMethod");

        public SqlCaluse Join(Expression exp, SqlCaluse sqlCaluse) => Join((T)exp, sqlCaluse);
        protected virtual SqlCaluse Join(T exp, SqlCaluse sqlCaluse) =>
            throw new NotImplementedException($"[{_expressionType}] 未实现 Join");

        public SqlCaluse Where(Expression exp, SqlCaluse sqlCaluse) => Where((T)exp, sqlCaluse);
        protected virtual SqlCaluse Where(T exp, SqlCaluse sqlCaluse) =>
            throw new NotImplementedException($"[{_expressionType}] 未实现 Where");

        public SqlCaluse Insert(Expression exp, SqlCaluse sqlCaluse) => Insert((T)exp, sqlCaluse);
        protected virtual SqlCaluse Insert(T exp, SqlCaluse sqlCaluse) =>
            throw new NotImplementedException($"[{_expressionType}] 未实现 Insert");

        public SqlCaluse In(Expression exp, SqlCaluse sqlCaluse) => In((T)exp, sqlCaluse);
        protected virtual SqlCaluse In(T exp, SqlCaluse sqlCaluse) =>
            throw new NotImplementedException($"[{_expressionType}] 未实现 In");

        public SqlCaluse GroupBy(Expression exp, SqlCaluse sqlCaluse) => GroupBy((T)exp, sqlCaluse);
        protected virtual SqlCaluse GroupBy(T exp, SqlCaluse sqlCaluse) =>
            throw new NotImplementedException($"[{_expressionType}] 未实现 GroupBy");

        public SqlCaluse OrderBy(Expression exp, SqlCaluse sqlCaluse) => OrderBy((T)exp, sqlCaluse);
        protected virtual SqlCaluse OrderBy(T exp, SqlCaluse sqlCaluse) =>
            throw new NotImplementedException($"[{_expressionType}] 未实现 OrderBy");

        public SqlCaluse Max(Expression exp, SqlCaluse sqlCaluse) => Max((T)exp, sqlCaluse);
        protected virtual SqlCaluse Max(T exp, SqlCaluse sqlCaluse) =>
            throw new NotImplementedException($"[{_expressionType}] 未实现 Max");

    }
}
