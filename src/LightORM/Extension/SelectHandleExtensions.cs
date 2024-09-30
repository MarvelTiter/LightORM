using LightORM.Providers;

namespace LightORM;

internal static class SelectHandleExtensions
{
    internal static void WhereHandle(this IExpSelect select, Expression exp)
    {
        select.SqlBuilder.Expressions.Add(new ExpressionInfo
        {
            ResolveOptions = SqlResolveOptions.Where,
            Expression = exp,
        });
    }

    internal static void OrderByHandle(this IExpSelect select, Expression exp, bool asc)
    {
        select.SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            Expression = exp,
            ResolveOptions = SqlResolveOptions.Order,
            AdditionalParameter = asc ? "ASC" : "DESC"
        });
    }
    internal static IExpSelectGroup<TGroup, TTables> GroupByHandle<TGroup, TTables>(this IExpSelect select, Expression exp)
    {
        if (exp is LambdaExpression lambda && lambda.Body is not NewExpression)
        {
            LightOrmException.Throw("GroupBy请返回匿名类型，否则无法再后续操作中解析属性来源");
        }
        select.SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            ResolveOptions = SqlResolveOptions.Group,
            Expression = exp
        });
        return new GroupSelectProvider<TGroup, TTables>(select.Executor, select.SqlBuilder);
    }
    internal static void HandleTempQuery(this IExpSelect select, params IExpTemp[] temps)
    {
        foreach (var item in temps)
        {
            select.SqlBuilder.TempViews.Add(item.SqlBuilder);
            select.SqlBuilder.SelectedTables.Add(item.ResultTable);
        }
    }

    internal static void JoinHandle<TJoin>(this IExpSelect select, Expression exp, TableLinkType joinType, IExpSelect<TJoin>? subQuery = null)
    {
        var expression = new ExpressionInfo
        {
            ResolveOptions = SqlResolveOptions.Join,
            Expression = exp,
        };

        select.SqlBuilder.Expressions.Add(expression);
        var joinInfo = new JoinInfo()
        {
            ExpressionId = expression.Id,
            JoinType = joinType,
            EntityInfo = Cache.TableContext.GetTableInfo<TJoin>(),
        };
        if (subQuery != null)
        {
            subQuery.SqlBuilder.Level += 1;
            joinInfo.IsSubQuery = true;
            joinInfo.QuerySql = subQuery.ToSql();
        }
        select.SqlBuilder.Joins.Add(joinInfo);
        //SqlBuilder.OtherTables.Add()
        //return (this as TSelect)!;
    }

    internal static void JoinHandle(this IExpSelect select, Expression exp, TableLinkType joinType)
    {
        /// <summary>
        /// 当Select了多个表的时候，使用非泛型的Join扩展方法时，按顺序从SelectedTables中Join
        /// </summary>
        var expression = new ExpressionInfo
        {
            ResolveOptions = SqlResolveOptions.Join,
            Expression = exp,
            AdditionalParameter = select.SqlBuilder.Joins.Count + 1,
        };

        select.SqlBuilder.Expressions.Add(expression);
        var joinInfo = new JoinInfo()
        {
            ExpressionId = expression.Id,
            JoinType = joinType,
        };
        select.SqlBuilder.Joins.Add(joinInfo);
        //SqlBuilder.OtherTables.Add()
        //return (this as TSelect)!;
    }

    internal static void HandleResult(this IExpSelect select, Expression? exp, string? template)
    {
        select.SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            Expression = exp,
            ResolveOptions = SqlResolveOptions.Select,
            Template = template
        });
    }

    internal static SelectProvider1<TTemp> HandleSubQuery<TTemp>(this IExpSelect select)
    {
        select.SqlBuilder.Level += 1;
        select.SqlBuilder.IsSubQuery = true;
        var builder = new SelectBuilder(select.SqlBuilder.DbType)
        {
            SubQuery = select.SqlBuilder,
        };
        builder.SelectedTables.Add(TableContext.GetTableInfo<TTemp>());
        return new SelectProvider1<TTemp>(select.Executor, builder);
    }
}

