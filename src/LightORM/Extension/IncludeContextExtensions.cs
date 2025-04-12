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
                SelectBuilder includeBuilder = BuildIncludeSqlBuilder(context.DbType, item, include);
                string sql = includeBuilder.ToSqlString();
                var param = includeBuilder.DbParameters;
                var typedQuery = QueryMethod.MakeGenericMethod(include.SelectedTable!.Type!);
                var result = typedQuery.Invoke(null, [executor, sql, param, null, CommandType.Text]);
                if (include.NavigateInfo!.IsMultiResult)
                {
                    var tolist = ToList.MakeGenericMethod(include.SelectedTable!.Type!);
                    result = tolist.Invoke(null, [result]);
                }
                else
                {
                    var firstOrDefault = FirstOrDefault.MakeGenericMethod(include.SelectedTable!.Type!);
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

        public static SelectBuilder BuildIncludeSqlBuilder(DbBaseType dbBaseType, object item, IncludeInfo include)
        {
            var mainWhere = BuildMainWhereExpression(item, include.ParentWhereColumn!);
            var includeBuilder = BuildSql(dbBaseType, include, mainWhere);
            return includeBuilder;
        }

        private static SelectBuilder BuildSql(DbBaseType dbBaseType, IncludeInfo include, Expression mainWhere)
        {
            SelectBuilder selectSql = new(dbBaseType);
            selectSql.SelectedTables.Add(include.SelectedTable!);
            selectSql.DbParameterStartIndex = include.ExpressionResolvedResult!.DbParameters?.Count ?? 0;
            selectSql.DbParameters.TryAddDictionary(include.ExpressionResolvedResult!.DbParameters);
            selectSql.Expressions.Add(new ExpressionInfo
            {
                Expression = BuildSelectAllExpression(include.SelectedTable!.Type!),
                ResolveOptions = SqlResolveOptions.Select,
            });

            selectSql.Expressions.Add(new ExpressionInfo
            {
                Expression = mainWhere,
                ResolveOptions = SqlResolveOptions.Where,
            });

            var mainNav = selectSql.MainTable.GetNavigateColumns(c => c.NavigateInfo?.MappingType == include.NavigateInfo!.MappingType).First().NavigateInfo!;
            var mainCol = selectSql.MainTable.GetColumnInfo(mainNav.MainName!);
            var parentTable = include.ParentTable!;
            if (include.NavigateInfo!.MappingType != null)
            {
                var mapTable = TableInfo.Create(include.NavigateInfo!.MappingType,selectSql.NextTableIndex);
                var subCol = mapTable.GetColumnInfo(mainNav.SubName!);
                selectSql.Joins.Add(new JoinInfo
                {
                    EntityInfo = mapTable,
                    JoinType = ExpressionSql.TableLinkType.LeftJoin,
                    Where = $"( {selectSql.AttachEmphasis(selectSql.MainTable.Alias)}.{selectSql.AttachEmphasis(mainCol.ColumnName)} = {selectSql.AttachEmphasis(mapTable.Alias)}.{selectSql.AttachEmphasis(subCol.ColumnName)} )"
                });
                subCol = parentTable.GetColumnInfo(include.NavigateInfo!.SubName!);
                parentTable.Index = selectSql.NextTableIndex;
                selectSql.Joins.Add(new JoinInfo
                {
                    EntityInfo = parentTable,
                    JoinType = ExpressionSql.TableLinkType.LeftJoin,
                    Where = $"( {selectSql.AttachEmphasis(parentTable.Alias)}.{selectSql.AttachEmphasis(include.ParentWhereColumn!.ColumnName)} = {selectSql.AttachEmphasis(mapTable.Alias)}.{selectSql.AttachEmphasis(subCol.ColumnName)} )"

                });
            }
            else
            {
                var subCol = parentTable.GetColumnInfo(include.NavigateInfo!.SubName!);
                parentTable.Index = selectSql.NextTableIndex;
                selectSql.Joins.Add(new JoinInfo
                {
                    EntityInfo = parentTable,
                    JoinType = ExpressionSql.TableLinkType.LeftJoin,
                    Where = $"( {selectSql.AttachEmphasis(parentTable.Alias)}.{selectSql.AttachEmphasis(include.ParentWhereColumn!.ColumnName)} = {selectSql.AttachEmphasis(selectSql.MainTable.Alias)}.{selectSql.AttachEmphasis(subCol.ColumnName)} )"

                });
            }

            if (include.ExpressionResolvedResult!.NavigateDeep > 0)
            {
                selectSql.Where.Add(include.ExpressionResolvedResult!.SqlString!);
            }
            return selectSql;
        }

        private static LambdaExpression BuildSelectAllExpression(Type type)
        {
            var p = Expression.Parameter(type, "t");
            var lambda = Expression.Lambda(p, p);
            return lambda;
        }
        private static LambdaExpression BuildMainWhereExpression(object item, ITableColumnInfo col)
        {
            var p = Expression.Parameter(col.TableType);
            var equal = Expression.Equal(Expression.Property(p, col.PropertyName), Expression.Constant(col.GetValue(item)));
            return Expression.Lambda(equal, p);
        }
    }
}
