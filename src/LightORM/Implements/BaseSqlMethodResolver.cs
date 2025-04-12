using LightORM.Extension;

namespace LightORM.Implements
{
    /// <summary>
    /// Sql函数解析
    /// </summary>
    public abstract partial class BaseSqlMethodResolver : ISqlMethodResolver
    {
        /// <summary>
        /// 在生成器中初始化集合内容
        /// </summary>
        private readonly Dictionary<string, Action<IExpressionResolver, MethodCallExpression>> methods = [];

        public void AddOrUpdateMethod(string name, Action<IExpressionResolver, MethodCallExpression> methodResolver)
        {
            methods[name] = methodResolver;
        }

        public void Resolve(IExpressionResolver resolver, MethodCallExpression expression)
        {
            var methodName = expression.Method.Name;
            if (!methods.TryGetValue(methodName, out var action))
            {
                throw new NotSupportedException($"{this.GetType().Name}: {expression.Method.Name}, {expression.Method.GetParameters().Select(p => $"[{p.ParameterType}:{p.Name}]")}");
            }

            action.Invoke(resolver, expression);
        }

        public virtual void Format(IExpressionResolver resolver, MethodCallExpression expression)
        {
            if (expression.Arguments.Count == 0)
                return;
            var formatString = expression.Arguments[0] as ConstantExpression;
            var args = expression.Arguments.Skip(1).Select(a => Expression.Lambda(a).Compile().DynamicInvoke());
            var formatedValue = string.Format(formatString.Value.ToString(), [..args]);
            resolver.Sql.Append("'");
            resolver.Sql.Append(formatedValue);            
            resolver.Sql.Append("'");
        }

        #region 子查询专用方法解析

        public virtual void Result(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            //var subQuery = methodCall.Arguments[0];
            //var obj = Expression.Lambda(subQuery).Compile().DynamicInvoke();
            var sel = methodCall.GetExpSelectObject();
            if (sel is not null)
            {
                sel.SqlBuilder.Level = resolver.Level + 1;
                sel.SqlBuilder.IsSubQuery = true;
                resolver.Sql.AppendLine("(");
                var sql = sel.SqlBuilder.ToSqlString();
                resolver.Sql.Append(sql);
                resolver.Sql.Append(')');
            }
        }

        public virtual void Exits(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            //var subQuery = methodCall.Arguments[0];
            //var obj = Expression.Lambda(subQuery).Compile().DynamicInvoke();
            var sel = methodCall.GetExpSelectObject();
            if (sel is not null)
            {
                sel.SqlBuilder.Level = resolver.Level + 1;
                sel.SqlBuilder.IsSubQuery = true;
                resolver.Sql.AppendLine(resolver.IsNot ? "NOT EXISTS (" : "EXISTS (");
                var sql = sel.SqlBuilder.ToSqlString();
                resolver.Sql.Append(sql);
                resolver.Sql.Append(')');
            }
        }

        #endregion

        public virtual void In(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(resolver.IsNot ? " NOT IN " : " IN ");
            resolver.Sql.Append('(');
            resolver.Visit(methodCall.Arguments[1]);
            resolver.Sql.Append(')');
        }

        public virtual void Count(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (methodCall.IsExpSelect() && !methodCall.IsExpSelectGrouping())
            {
                var sel = methodCall.GetExpSelectObject()!;
                if (methodCall.Arguments.Count == 0)
                {
                    sel.HandleResult(null, "COUNT(*)");
                }
                else
                {
                    sel.HandleResult(methodCall.Arguments[0], "COUNT({0})");
                }

                sel.SqlBuilder.Level = resolver.Level + 1;
                sel.SqlBuilder.IsSubQuery = true;
                resolver.Sql.AppendLine("(");
                var sql = sel.SqlBuilder.ToSqlString();
                resolver.Sql.Append(sql);
                resolver.Sql.AppendLine(")");
                return;
            }

            if (methodCall.Arguments.Count > 0)
            {
                var useCaseWhen = methodCall.Method.GetParameters()[0].ParameterType == typeof(bool)
                                  && methodCall.Arguments[0] is BinaryExpression;
                if (useCaseWhen)
                {
                    resolver.Sql.Append("COUNT(CASE WHEN ");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(" THEN 1 ElSE NULL END)");
                }
                else
                {
                    resolver.Sql.Append("COUNT(");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(')');
                }
            }
            else
            {
                resolver.Sql.Append("COUNT(*)");
            }
        }

        public virtual void CountDistinct(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (methodCall.IsExpSelect() && !methodCall.IsExpSelectGrouping())
            {
                var sel = methodCall.GetExpSelectObject()!;
                if (methodCall.Arguments.Count == 0)
                {
                    sel.HandleResult(null, "COUNT(*)");
                }
                else
                {
                    sel.HandleResult(methodCall.Arguments[0], "COUNT(DISTINCT {0})");
                }

                sel.SqlBuilder.Level = resolver.Level + 1;
                sel.SqlBuilder.IsSubQuery = true;
                resolver.Sql.AppendLine("(");
                var sql = sel.SqlBuilder.ToSqlString();
                resolver.Sql.Append(sql);
                resolver.Sql.AppendLine(")");
                return;
            }

            if (methodCall.Arguments.Count > 0)
            {
                var useCaseWhen = methodCall.Method.GetParameters()[0].ParameterType == typeof(bool)
                                  && methodCall.Arguments[0] is BinaryExpression;
                if (useCaseWhen)
                {
                    resolver.Sql.Append("COUNT(DISTINCT CASE WHEN ");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(" THEN 1 ElSE NULL END)");
                }
                else
                {
                    resolver.Sql.Append("COUNT(DISTINCT ");
                    resolver.Visit(methodCall.Arguments[0]);
                    resolver.Sql.Append(')');
                }
            }
            else
            {
                resolver.Sql.Append("COUNT(*)");
            }
        }

        public virtual void Sum(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (methodCall.Arguments.Count > 1)
            {
                resolver.Sql.Append("SUM(CASE WHEN ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(" THEN ");
                resolver.Visit(methodCall.Arguments[1]);
                resolver.Sql.Append(" ElSE 0 END)");
            }
            else
            {
                resolver.Sql.Append("SUM(");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(')');
            }
        }

        public virtual void Avg(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (methodCall.Arguments.Count > 1)
            {
                resolver.Sql.Append("AVG(CASE WHEN ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(" THEN ");
                resolver.Visit(methodCall.Arguments[1]);
                resolver.Sql.Append(" ElSE 0 END)");
            }
            else
            {
                resolver.Sql.Append("AVG(");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(')');
            }
        }

        public virtual void Max(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (methodCall.Arguments.Count > 1)
            {
                resolver.Sql.Append("MAX(CASE WHEN ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(" THEN ");
                resolver.Visit(methodCall.Arguments[1]);
                resolver.Sql.Append(" ELSE 0 END)");
            }
            else
            {
                resolver.Sql.Append("MAX(");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(')');
            }
        }

        public virtual void Min(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (methodCall.Arguments.Count > 1)
            {
                resolver.Sql.Append("MIN(CASE WHEN ");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(" THEN ");
                resolver.Visit(methodCall.Arguments[1]);
                resolver.Sql.Append(" ElSE 0 END)");
            }
            else
            {
                resolver.Sql.Append("MIN(");
                resolver.Visit(methodCall.Arguments[0]);
                resolver.Sql.Append(')');
            }
        }

        public virtual void Abs(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            var exp = methodCall.Arguments[0];
            resolver.Sql.Append("ABS");
            if (exp is BinaryExpression)
            {
                resolver.Visit(exp);
            }
            else
            {
                resolver.Sql.Append('(');
                resolver.Visit(exp);
                resolver.Sql.Append(')');
            }
        }

        public virtual void Round(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Sql.Append("ROUND(");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(',');
            resolver.Visit(methodCall.Arguments[1]);
            resolver.Sql.Append(')');
        }

        public virtual void Nvl(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Sql.Append("NVL(");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(',');
            resolver.Visit(methodCall.Arguments[1]);
            resolver.Sql.Append(')');
        }

        public virtual void IsNull(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Sql.Append("ISNULL(");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(',');
            resolver.Visit(methodCall.Arguments[1]);
            resolver.Sql.Append(')');
        }

        public virtual void NullThen(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            //resolver.Sql.Append("IFNULL(");
            //resolver.Visit(methodCall.Arguments[0]);
            //resolver.Sql.Append(',');
            //resolver.Visit(methodCall.Arguments[1]);
            //resolver.Sql.Append(')');
            throw new NotSupportedException();
        }

        #region 类型转换

        public virtual void ToString(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region 字符串相关

        public virtual void StartsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        public virtual void Contains(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        public virtual void EndsWith(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        public virtual void Substring(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            throw new NotSupportedException();
        }

        public virtual void Trim(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            // TRIM(columnName);
            resolver.Sql.Append("TRIM");
            resolver.Sql.Append('(');
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(')');
        }

        public virtual void TrimStart(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            // LTRIM(columnName);
            resolver.Sql.Append("LTRIM");
            resolver.Sql.Append('(');
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(')');
        }

        public virtual void TrimEnd(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            // RTRIM(columnName);
            resolver.Sql.Append("RTRIM");
            resolver.Sql.Append('(');
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(')');
        }

        #region Group Join

        public virtual void Join(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.ExpStores ??= [];
            resolver.ExpStores.Add("Join", methodCall.Arguments[0]);
        }

        public virtual void Distinct(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.ExpStores!.Add("Distinct", null);
        }

        public virtual void Separator(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.ExpStores!.Add("Separator", methodCall.Arguments[0]);
        }

        #endregion

        #endregion

        #region include用到的方法

        public virtual void Where(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (resolver.NavigateDeep > 0)
            {
                resolver.Sql.Clear();
            }

            resolver.NavigateDeep++;
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Clear();
            resolver.Visit(methodCall.Arguments[1]);
        }

        public virtual void WhereIf(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            if (resolver.NavigateDeep > 0)
            {
                resolver.Sql.Clear();
            }

            resolver.NavigateDeep++;
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Visit(methodCall.Arguments[1]);
        }

        #endregion

        #region 窗口函数

        public virtual void RowNumber(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Sql.Append("ROW_NUMBER() OVER(");
        }

        public virtual void Lag(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Sql.Append("LAG(");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(") OVER(");
        }

        public virtual void Rank(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Sql.Append("RANK() OVER(");
        }

        public virtual void PartitionBy(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" PARTITION BY ");
            resolver.Visit(methodCall.Arguments[0]);
        }

        public virtual void OrderBy(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" ORDER BY ");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(" ASC");
        }

        public virtual void OrderByDesc(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" ORDER BY ");
            resolver.Visit(methodCall.Arguments[0]);
            resolver.Sql.Append(" DESC");
        }

        public virtual void Value(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" )");
        }

        #endregion

        #region Case When

        public virtual void Case(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Sql.Append("CASE");
            if (methodCall.Arguments.Count > 0)
            {
                resolver.Sql.Append(' ');
                resolver.Visit(methodCall.Arguments[0]);
            }
        }

        public virtual void When(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" WHEN ");
            resolver.Visit(methodCall.Arguments[0]);
        }

        public virtual void Then(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" THEN ");
            resolver.Visit(methodCall.Arguments[0]);
        }

        public virtual void Else(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" ELSE ");
            resolver.Visit(methodCall.Arguments[0]);
        }

        public virtual void End(IExpressionResolver resolver, MethodCallExpression methodCall)
        {
            resolver.Visit(methodCall.Object);
            resolver.Sql.Append(" END");
        }

        #endregion
    }
}