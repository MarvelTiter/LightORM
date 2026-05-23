using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace LightORM.Extension
{
    internal static class IncludeContextExtensions
    {
        private readonly static MethodInfo QueryMethod = typeof(SqlExecutorExtensions).GetMethods().First(m => m.Name == nameof(SqlExecutorExtensions.Query) && m.IsGenericMethod);
        private readonly static MethodInfo ToList = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!;
        private readonly static MethodInfo FirstOrDefault = typeof(Enumerable).GetMethod(nameof(Enumerable.FirstOrDefault), Type.EmptyTypes)!;

#if NET8_0_OR_GREATER

#endif
        public static void BindIncludeDatas(this IncludeContext context, ISqlExecutor executor, object data)
        {
            // TODO 在TableContext生成器中，增加Include相关方法的生成
            if (data is IEnumerable datas)
            {
                foreach (object item in datas)
                {
                    foreach (IncludeInfo include in context.Includes)
                    {
                        Do(context, executor, item, include);
                    }
                }
            }
            else
            {
                foreach (IncludeInfo include in context.Includes)
                {
                    Do(context, executor, data, include);
                }
            }
        }
#if NET8_0_OR_GREATER

#endif
        private static void Do(IncludeContext context, ISqlExecutor executor, object item, IncludeInfo include)
        {
            var database = executor.Database.DatabaseAdapter;
            SelectBuilder includeBuilder = BuildIncludeSqlBuilder(database, item, include);
            var selectedType = include.NavigateInfo!.NavigateType;
            string sql = includeBuilder.ToSqlString(database);
            var param = includeBuilder.DbParameters;
            var typedQuery = QueryMethod.MakeGenericMethod(selectedType);
            var result = typedQuery.Invoke(null, [executor, sql, param, null, CommandType.Text]);
            if (include.NavigateInfo!.IsMultiResult)
            {
                var tolist = ToList.MakeGenericMethod(selectedType);
                result = tolist.Invoke(null, [result]);
            }
            else
            {
                var firstOrDefault = FirstOrDefault.MakeGenericMethod(selectedType);
                result = firstOrDefault.Invoke(null, [result]);
            }
            if (result == null)
            {
                return;
            }
            include.ParentNavigateColumn!.SetValue(item, result);
            context.ThenInclude?.BindIncludeDatas(executor, result);
        }

        public static SelectBuilder BuildIncludeSqlBuilder(IDatabaseAdapter database, object item, IncludeInfo include)
        {
            var includeBuilder = BuildSql(database, include, item);
            return includeBuilder;
        }

        private static SelectBuilder BuildSql(IDatabaseAdapter database, IncludeInfo include, object item)
        {
            SelectBuilder selectSql = SelectBuilder.GetSelectBuilder();
            var selectedType = include.NavigateInfo!.NavigateType;
            selectSql.SelectedTables.Add(TableInfo.Create(selectedType));

            var mainNav = selectSql.MainTable.GetNavigateColumns(c => c.NavigateInfo?.MappingType == include.NavigateInfo!.MappingType).First().NavigateInfo!;
            var mainCol = selectSql.MainTable.GetColumnInfo(mainNav.MainName!);
            var parentTable = include.ParentTable!;
            if (include.NavigateInfo!.MappingType != null)
            {
                var mapTable = TableInfo.Create(include.NavigateInfo!.MappingType, selectSql.NextTableIndex);
                var subCol = mapTable.GetColumnInfo(mainNav.SubName!);
                selectSql.Joins.Add(new JoinInfo
                {
                    EntityInfo = mapTable,
                    JoinType = TableLinkType.InnerJoin,
                    Where = $"( {selectSql.MainTable.Alias}.{database.AttachEmphasis(mainCol.ColumnName)} = {mapTable.Alias}.{database.AttachEmphasis(subCol.ColumnName)} )"
                });
                subCol = parentTable.GetColumnInfo(include.NavigateInfo!.SubName!);
                parentTable.Index = selectSql.NextTableIndex;
                selectSql.Joins.Add(new JoinInfo
                {
                    EntityInfo = parentTable,
                    JoinType = TableLinkType.InnerJoin,
                    Where = $"( {parentTable.Alias}.{database.AttachEmphasis(include.ParentWhereColumn!.ColumnName)} = {mapTable.Alias}.{database.AttachEmphasis(subCol.ColumnName)} )"
                });
            }
            else
            {
                var subCol = parentTable.GetColumnInfo(include.NavigateInfo!.SubName!);
                parentTable.Index = selectSql.NextTableIndex;
                selectSql.Joins.Add(new JoinInfo
                {
                    EntityInfo = parentTable,
                    JoinType = TableLinkType.InnerJoin,
                    Where = $"( {parentTable.Alias}.{database.AttachEmphasis(include.ParentWhereColumn!.ColumnName)} = {selectSql.MainTable.Alias}.{database.AttachEmphasis(subCol.ColumnName)} )"
                });
            }
            ParameterExpression[] all = [.. selectSql.AllTables().Select(t => Expression.Parameter(t.Type))];
            selectSql.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Select, BuildSelectAllExpression(all)));

            selectSql.Expressions.Add(new ExpressionInfo(SqlResolveOptions.Where, BuildMainWhereExpression(item, include.ParentWhereColumn!, all)));
            if (include.IncludeWhereExpression is not null)
            {
                selectSql.Expressions.Add(new(SqlResolveOptions.Where, BuildIncludeNavigateExpression(selectSql, include.IncludeWhereExpression)));
            }
            return selectSql;
        }
#if NET8_0_OR_GREATER

#endif
        private static LambdaExpression BuildSelectAllExpression(ParameterExpression[] allTables)
        {
            var lambda = Expression.Lambda(allTables[0], allTables);
            return lambda;
        }
#if NET8_0_OR_GREATER

#endif
        private static LambdaExpression BuildMainWhereExpression(object item, ITableColumnInfo col, ParameterExpression[] allTables)
        {
            var p = allTables.Last();
            var equal = Expression.Equal(Expression.Property(p, col.PropertyName), Expression.Constant(col.GetValue(item)));
            return Expression.Lambda(equal, allTables);
        }
        private static LambdaExpression BuildIncludeNavigateExpression(SelectBuilder _, Expression body)
        {
            if (body.TryGetLambdaExpression(out var mainLambda))
            {
                return mainLambda!;
            }
            throw new LightOrmException("Include的Where条件不是一个LambdaExpression");
        }
    }
}
