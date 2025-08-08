using LightORM.Extension;
using LightORM.Providers;

namespace LightORM;

internal static class SelectHandleExtensions
{
    internal static void WhereHandle(this IExpSelect select, Expression? exp)
    {
        select.SqlBuilder.Expressions.Add(new ExpressionInfo
        {
            ResolveOptions = SqlResolveOptions.Where,
            Expression = exp,
        });
    }

    internal static void OrderByHandle(this IExpSelect select, Expression? exp, bool asc)
    {
        select.SqlBuilder.Expressions.Add(new ExpressionInfo()
        {
            Expression = exp,
            ResolveOptions = SqlResolveOptions.Order,
            AdditionalParameter = asc ? "ASC" : "DESC"
        });
    }
    internal static IExpSelectGroup<TGroup, TTables> GroupByHandle<TGroup, TTables>(this IExpSelect select, Expression? exp)
    {
        if (exp is LambdaExpression keySelector)
        {
            select.SqlBuilder.Expressions.Add(new ExpressionInfo()
            {
                ResolveOptions = SqlResolveOptions.Group,
                Expression = exp
            });
            return new GroupSelectProvider<TGroup, TTables>(select.Executor, select.SqlBuilder, keySelector);
        }
        //LightOrmException.Throw("GroupBy请返回NewExpression，否则无法在后续操作中解析属性来源");
        throw new LightOrmException("表达式类型不是LambdaExpression");

    }
    internal static void HandleTempQuery(this IExpSelect select, params IExpTemp[] temps)
    {
        foreach (var item in temps)
        {
            //select.SqlBuilder.TempViews.Add(item.SqlBuilder);
            select.SqlBuilder.HandleTempsRecursion(item.SqlBuilder);
            item.ResultTable.Index = select.SqlBuilder.NextTableIndex;
            select.SqlBuilder.SelectedTables.Add(item.ResultTable);
        }
    }

    internal static void JoinHandle<TJoin>(this IExpSelect select, Expression? exp, TableLinkType joinType, IExpSelect<TJoin>? subQuery = null)
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
            EntityInfo = TableInfo.Create<TJoin>(select.SqlBuilder.NextTableIndex),
        };
        if (subQuery != null)
        {
            joinInfo.IsSubQuery = true;
            joinInfo.SubQuery = subQuery.SqlBuilder;
        }
        select.SqlBuilder.Joins.Add(joinInfo);
    }

    internal static void JoinHandle(this IExpSelect select, Type type, Expression? exp, TableLinkType joinType)
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
            EntityInfo = TableInfo.Create(type, select.SqlBuilder.NextTableIndex),
        };

        select.SqlBuilder.Joins.Add(joinInfo);
    }

    internal static void JoinHandle<TJoin>(this IExpSelect select, Expression? exp, TableLinkType joinType, IExpTemp tempQuery)
    {
        var expression = new ExpressionInfo
        {
            ResolveOptions = SqlResolveOptions.Join,
            Expression = exp,
        };

        select.SqlBuilder.Expressions.Add(expression);
        tempQuery.ResultTable.Index = select.SqlBuilder.NextTableIndex;
        var joinInfo = new JoinInfo()
        {
            ExpressionId = expression.Id,
            JoinType = joinType,
            EntityInfo = tempQuery.ResultTable
        };
        select.SqlBuilder.HandleTempsRecursion(tempQuery.SqlBuilder);
        select.SqlBuilder.Joins.Add(joinInfo);
    }

    [Obsolete("多余的设计")]
    internal static void JoinHandle(this IExpSelect select, Expression? exp, TableLinkType joinType)
    {
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

    internal static SelectProvider1<TTemp> HandleSubQuery<TTemp>(this IExpSelect select, string? alias = null)
    {
        select.SqlBuilder.IsSubQuery = true;
        var builder = new SelectBuilder();
        var table = TableInfo.Create<TTemp>();
        if (alias != null)
        {
            LightOrmException.Throw("暂不支持自定义表别名");
            table.Alias = alias;
        }
        builder.SelectedTables.Add(table);
        builder.HandleTempsRecursion(select.SqlBuilder);
        builder.SubQuery = select.SqlBuilder;
        return new SelectProvider1<TTemp>(select.Executor, builder);
    }
}

