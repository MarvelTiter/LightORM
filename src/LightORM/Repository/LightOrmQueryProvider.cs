using LightORM.Extension;

namespace LightORM.Repository;
internal class LightOrmQueryProvider : IQueryProvider
{
    private readonly SelectBuilder select = new();
    private readonly ISqlExecutor ado;
    private LambdaExpression? keySelector;
    public LightOrmQueryProvider(ISqlExecutor ado, Type type)
    {
        this.ado = ado;
        select.SelectedTables.Add(TableInfo.Create(type));
    }
    public IQueryable CreateQuery(Expression expression)
    {
        throw new NotImplementedException();
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        if (expression is not MethodCallExpression methodCallExpression)
        {
            throw new ArgumentException("Expression must be a method call expression.", nameof(expression));
        }

        return methodCallExpression.Method.Name switch
        {
            "Where" => HandleExpression<TElement>(methodCallExpression, SqlResolveOptions.Where),
            "OrderBy" => HandleExpression<TElement>(methodCallExpression, SqlResolveOptions.Order, e => e.AdditionalParameter = "ASC"),
            "OrderByDescending" => HandleExpression<TElement>(methodCallExpression, SqlResolveOptions.Order, e => e.AdditionalParameter = "DESC"),
            "Join" => HandleJoinExpression<TElement>(methodCallExpression),
            "GroupBy" => HandleGroupExpression<TElement>(methodCallExpression),
            "Select" => HandleSelectExpression<TElement>(methodCallExpression),
            "Skip" => HandleConstantExpression<TElement, int>(methodCallExpression, i => select.Skip = i),
            "Take" => HandleConstantExpression<TElement, int>(methodCallExpression, i => select.Take = i),
            _ => throw new NotSupportedException($"Method '{methodCallExpression.Method.Name}' is not supported.")
        };
    }

    private LightOrmQuery<TElement> HandleExpression<TElement>(MethodCallExpression methodCallExpression, SqlResolveOptions option, Action<ExpressionInfo>? action = null)
    {
        var expression = methodCallExpression.Arguments[1];
        if (expression.TryGetLambdaExpression(out var lambda))
        {
            expression = LinqExpressionFlattener.Default.Flatten(lambda!);
        }
        var exp = new ExpressionInfo
        {
            ResolveOptions = option,
            Expression = expression
        };
        action?.Invoke(exp);
        select.Expressions.Add(exp);
        return ReturnOrCreateQuery<TElement>(methodCallExpression, this);
    }

    private LightOrmQuery<TElement> HandleJoinExpression<TElement>(MethodCallExpression methodCallExpression)
    {
        LambdaExpression joinCondition = BuildJoinConditionLambda(methodCallExpression);
        var joinType = joinCondition.Parameters[1].Type;
        var exp = new ExpressionInfo
        {
            ResolveOptions = SqlResolveOptions.Join,
            Expression = joinCondition,
        };
        select.Expressions.Add(exp);
        select.Joins.Add(new JoinInfo()
        {
            ExpressionId = exp.Id,
            JoinType = TableLinkType.InnerJoin,
            EntityInfo = TableInfo.Create(joinType, select.NextTableIndex),
        });
        return ReturnOrCreateQuery<TElement>(methodCallExpression, this);

        static LambdaExpression BuildJoinConditionLambda(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Arguments[2].TryGetLambdaExpression(out var left)
                 && methodCallExpression.Arguments[3].TryGetLambdaExpression(out var right))
            {
                var joinCondition = Expression.Lambda(Expression.Equal(left!.Body, right!.Body), [left.Parameters[0], right.Parameters[0]]);
                return joinCondition;
            }
            throw new ArgumentNullException();
        }
    }

    private LightOrmQuery<TElement> HandleGroupExpression<TElement>(MethodCallExpression methodCallExpression)
    {
        keySelector = GetKeySelectorLambda(methodCallExpression);
        //var elementSelector = GetElementSelectorLambda(methodCallExpression);
        var exp = new ExpressionInfo()
        {
            Expression = keySelector,
            ResolveOptions = SqlResolveOptions.Group,
        };
        select.Expressions.Add(exp);
        return ReturnOrCreateQuery<TElement>(methodCallExpression, this);

        static LambdaExpression GetKeySelectorLambda(MethodCallExpression methodCallExpression)
        {
            methodCallExpression.Arguments[1].TryGetLambdaExpression(out var lambda);
            if (lambda == null)
            {
                throw new ArgumentNullException();
            }
            return LinqExpressionFlattener.Default.Flatten(lambda);
        }

        //static LambdaExpression? GetElementSelectorLambda(MethodCallExpression methodCallExpression)
        //{
        //    if (methodCallExpression.Arguments.Count > 2)
        //    {
        //        methodCallExpression.Arguments[2].TryGetLambdaExpression(out var lambda);
        //        if (lambda is not null)
        //            return LinqExpressionFlattener.Default.Flatten(lambda);
        //    }

        //    return null;
        //}
    }

    private LightOrmQuery<TElement> HandleSelectExpression<TElement>(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression.Arguments[1].TryGetLambdaExpression(out var lambda))
        {
            lambda = LinqExpressionFlattener.Default.Flatten(lambda!);
        }
        if (keySelector is not null)
        {
            lambda = FlatGrouping.Default.Flat(lambda!, keySelector);
        }
        var exp = new ExpressionInfo
        {
            ResolveOptions = SqlResolveOptions.Select,
            Expression = lambda
        };
        select.Expressions.Add(exp);
        return ReturnOrCreateQuery<TElement>(methodCallExpression, this);
    }

    private LightOrmQuery<TElement> HandleConstantExpression<TElement, TValue>(MethodCallExpression methodCallExpression, Action<TValue> action)
    {
        var c = methodCallExpression.Arguments[1] as ConstantExpression;
        if (c?.Value is TValue value)
        {
            action?.Invoke(value);
        }
        return ReturnOrCreateQuery<TElement>(methodCallExpression, this);
    }

    private static LightOrmQuery<TElement> ReturnOrCreateQuery<TElement>(MethodCallExpression methodCallExpression, LightOrmQueryProvider queryProvider)
    {
        var ins = methodCallExpression.Arguments[0] as ConstantExpression;
        return (ins?.Value as LightOrmQuery<TElement>) ?? new LightOrmQuery<TElement>(queryProvider);
    }

    public object Execute(Expression expression)
    {
        var sql = select.ToSqlString(ado.Database.CustomDatabase);
        return ado.ExecuteReader(sql, select.DbParameters);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        var sql = select.ToSqlString(ado.Database.CustomDatabase);
        var def = ado.Query<TResult>(sql, select.DbParameters).FirstOrDefault();
        if (def == null && expression is MethodCallExpression method && method.Arguments.Count > 1)
        {
            var defaultValue = method.Arguments[1] as UnaryExpression;
            if (defaultValue?.Operand is ConstantExpression constValue)
            {
                return (TResult)constValue?.Value!;
            }
        }
        return def!;
    }
}
