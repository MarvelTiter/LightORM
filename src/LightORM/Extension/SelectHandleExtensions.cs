using LightORM.Extension;
using LightORM.Providers;
using System.Diagnostics.CodeAnalysis;

namespace LightORM;

internal static class SelectHandleExtensions
{
    extension(IExpSelect select)
    {
        internal void WhereHandle(Expression? exp)
        {
            select.SqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Where, exp));
        }

        internal void OrderByHandle(Expression? exp, bool asc)
        {
            select.SqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Order, exp, additionalParameter: asc ? "ASC" : "DESC"));
        }

        internal IExpSelectGroup<TGroup, TTables> GroupByHandle<TGroup, TTables>(Expression? exp)
        {
            if (exp is LambdaExpression keySelector)
            {
                select.SqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Group, exp));
                return new GroupSelectProvider<TGroup, TTables>(select.DbContext, select.SqlBuilder, keySelector);
            }
            //LightOrmException.Throw("GroupBy请返回NewExpression，否则无法在后续操作中解析属性来源");
            throw new LightOrmException("表达式类型不是LambdaExpression");

        }

        internal void HandleTempQuery(params IExpTemp[] temps)
        {
            foreach (var item in temps)
            {
                //select.SqlBuilder.TempViews.Add(item.SqlBuilder);
                select.SqlBuilder.HandleTempsRecursion(item.SqlBuilder);
                item.ResultTable.Index = select.SqlBuilder.NextTableIndex;
                select.SqlBuilder.SelectedTables.Add(item.ResultTable);
            }
        }

        internal void JoinHandle<TJoin>(Expression? exp
            , TableLinkType joinType
            , IExpSelect<TJoin>? subQuery = null
            , string? overriddenTableName = null)
        {
            var expression = new ExpressionInfo(SqlResolveOptions.Join, exp);

            select.SqlBuilder.Expressions.Add(expression);
            var joinInfo = new JoinInfo()
            {
                ExpressionId = expression.Id,
                JoinType = joinType,
                EntityInfo = TableInfo.Create<TJoin>(overriddenTableName, select.SqlBuilder.NextTableIndex),
            };
            if (subQuery is not null)
            {
                joinInfo.IsSubQuery = true;
                joinInfo.SubQuery = subQuery.SqlBuilder;
            }
            select.SqlBuilder.Joins.Add(joinInfo);
        }

        internal void JoinHandle(Type type, Expression? exp, TableLinkType joinType)
        {
            var expression = new ExpressionInfo(SqlResolveOptions.Join, exp);

            select.SqlBuilder.Expressions.Add(expression);
            var joinInfo = new JoinInfo()
            {
                ExpressionId = expression.Id,
                JoinType = joinType,
                EntityInfo = TableInfo.Create(type, select.SqlBuilder.NextTableIndex),
            };
            //joinInfo.EntityInfo.Deep = select.SqlBuilder.Level;
            select.SqlBuilder.Joins.Add(joinInfo);
        }

        internal void JoinHandle<TJoin>(Expression? exp, TableLinkType joinType, IExpTemp tempQuery)
        {
            var expression = new ExpressionInfo(SqlResolveOptions.Join, exp);

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

        internal void HandleResult(Expression? exp, string? template)
        {
            select.SqlBuilder.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Select, exp, template));
        }

        internal SelectProvider1<TTemp> HandleSubQuery<
#if NET8_0_OR_GREATER
       [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
#endif
        TTemp>(string? alias = null)
        {
            select.SqlBuilder.IsSubQuery = true;
            var builder = SelectBuilder.GetSelectBuilder();
            var table = TableInfo.Create<TTemp>();
            if (alias != null)
            {
                table.Alias = alias;
                throw new LightOrmException("暂不支持自定义表别名");
            }
            builder.AddTableInfo(table);
            builder.HandleTempsRecursion(select.SqlBuilder);
            builder.SubQuery = select.SqlBuilder;
            return new SelectProvider1<TTemp>(select.DbContext, builder);
        }
    }

    extension(SelectBuilder builder)
    {
        internal void HandleResult(Expression? exp, string? template)
        {
            builder.Expressions.Add(new(SqlResolveOptions.Select, exp, template));
        }
        internal void JoinHandle(Type type, Expression? exp
            , TableLinkType joinType
            , IExpSelect? subQuery = null
            , string? overriddenTableName = null)
        {
            var expression = new ExpressionInfo(SqlResolveOptions.Join, exp);

            builder.Expressions.Add(expression);
            var joinInfo = new JoinInfo()
            {
                ExpressionId = expression.Id,
                JoinType = joinType,
                EntityInfo = TableInfo.Create(overriddenTableName, type, builder.NextTableIndex),
            };
            //joinInfo.EntityInfo.Deep = builder.Level;
            if (subQuery is not null)
            {
                joinInfo.IsSubQuery = true;
                joinInfo.SubQuery = subQuery.SqlBuilder;
            }
            builder.Joins.Add(joinInfo);
        }
    }
}

