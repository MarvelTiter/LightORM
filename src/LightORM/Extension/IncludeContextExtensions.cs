using System.Collections;
using System.Reflection;

namespace LightORM.Extension
{
    internal static class IncludeContextExtensions
    {
        private readonly static MethodInfo QueryMethod = typeof(SqlExecutorExtensions).GetMethods().First(m => m.Name == nameof(SqlExecutorExtensions.Query) && m.IsGenericMethod);
        private readonly static MethodInfo ToList = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!;
        private readonly static MethodInfo FirstOrDefault = typeof(Enumerable).GetMethod(nameof(Enumerable.FirstOrDefault), Type.EmptyTypes)!;
        public static void BindIncludeDatas(this IncludeContext context, ISqlExecutor executor, object data)
        {
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

            static void Do(IncludeContext context, ISqlExecutor executor, object item, IncludeInfo include)
            {
                var database = executor.Database.CustomDatabase;
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
        }

        public static SelectBuilder BuildIncludeSqlBuilder(ICustomDatabase database, object item, IncludeInfo include)
        {
            //var mainWhere = BuildMainWhereExpression(item, include.ParentWhereColumn!);
            var includeBuilder = BuildSql(database, include, item);
            return includeBuilder;
        }

        private static SelectBuilder BuildSql(ICustomDatabase database, IncludeInfo include, object item)
        {
            SelectBuilder selectSql = new();
            var selectedType = include.NavigateInfo!.NavigateType;
            selectSql.SelectedTables.Add(TableInfo.Create(selectedType));
            //selectSql.DbParameterStartIndex = include.ExpressionResolvedResult!.DbParameters?.Count ?? 0;
            //selectSql.DbParameters.TryAddDictionary(include.ExpressionResolvedResult!.DbParameters);

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
                    Where = $"( {database.AttachEmphasis(selectSql.MainTable.Alias)}.{database.AttachEmphasis(mainCol.ColumnName)} = {database.AttachEmphasis(mapTable.Alias)}.{database.AttachEmphasis(subCol.ColumnName)} )"
                });
                subCol = parentTable.GetColumnInfo(include.NavigateInfo!.SubName!);
                parentTable.Index = selectSql.NextTableIndex;
                selectSql.Joins.Add(new JoinInfo
                {
                    EntityInfo = parentTable,
                    JoinType = TableLinkType.InnerJoin,
                    Where = $"( {database.AttachEmphasis(parentTable.Alias)}.{database.AttachEmphasis(include.ParentWhereColumn!.ColumnName)} = {database.AttachEmphasis(mapTable.Alias)}.{database.AttachEmphasis(subCol.ColumnName)} )"
                });
            }
            else
            {
                var subCol = parentTable.GetColumnInfo(include.NavigateInfo!.SubName!);
                parentTable.Index = selectSql.NextTableIndex;
                selectSql.Joins.Add(new JoinInfo
                {
                    EntityInfo = parentTable,
                    JoinType = ExpressionSql.TableLinkType.InnerJoin,
                    Where = $"( {database.AttachEmphasis(parentTable.Alias)}.{database.AttachEmphasis(include.ParentWhereColumn!.ColumnName)} = {database.AttachEmphasis(selectSql.MainTable.Alias)}.{database.AttachEmphasis(subCol.ColumnName)} )"
                });
            }
            ParameterExpression[] all = [.. selectSql.AllTables().Select(t => Expression.Parameter(t.Type))];
            selectSql.Expressions.Add(new ExpressionInfo
            {
                Expression = BuildSelectAllExpression(all),
                ResolveOptions = SqlResolveOptions.Select,
            });

            selectSql.Expressions.Add(new ExpressionInfo
            {
                Expression = BuildMainWhereExpression(item, include.ParentWhereColumn!, all),
                ResolveOptions = SqlResolveOptions.Where,
            });
            if (include.IncludeWhereExpression is not null)
            {
                selectSql.Expressions.Add(new()
                {
                    Expression = BuildIncludeNavigateExpression(selectSql, include.IncludeWhereExpression),
                    ResolveOptions = SqlResolveOptions.Where
                });
            }
            return selectSql;
        }

        private static LambdaExpression BuildSelectAllExpression(ParameterExpression[] allTables)
        {
            var lambda = Expression.Lambda(allTables[0], allTables);
            return lambda;
        }
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
