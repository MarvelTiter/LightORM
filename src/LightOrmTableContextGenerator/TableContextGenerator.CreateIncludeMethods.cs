using System.Collections.Generic;
using System.Linq;
using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;

namespace LightOrmTableContextGenerator;

public partial class TableContextGenerator
{
    private static IEnumerable<MethodBuilder> CreateIncludeMethods(INamedTypeSymbol owner, PropertyScanResult[] columns)
    {
        // 增加Include处理方法
        List<(MethodBuilder, PropertyScanResult)> includes = [];
        List<(MethodBuilder, PropertyScanResult)> asyncIncludes = [];
        foreach (var p in columns)
        {
            if (!p.NavInfo.HasValue)
            {
                continue;
            }

            var method = CreateIncludeTarget(owner, p, false);
            includes.Add((method, p));
            yield return method;
            var asyncMethod = CreateIncludeTarget(owner, p, true);
            asyncIncludes.Add((asyncMethod, p));
            yield return asyncMethod;
        }

        var handleInclude = MethodBuilder.Default.MethodName("HandleInclude")
            .AddParameter("global::LightORM.IContext context", "object value", " global::System.Collections.Generic.IEnumerable<global::LightORM.Models.IncludeInfo> info");

        var handleIncludeAsync = MethodBuilder.Default.MethodName("HandleIncludeAsync")
            .ReturnType("global::System.Threading.Tasks.Task")
            .AddParameter("global::LightORM.IContext context", "object value", " global::System.Collections.Generic.IEnumerable<global::LightORM.Models.IncludeInfo> info", "global::System.Threading.CancellationToken cancellationToken")
            .Async();

        var foreachStatement = ForeachStatement.Default.Foreach("var item in info")
            .AddStatements("var nt = item.NavigateInfo?.NavigateType");
        var foreachStatementAsync = ForeachStatement.Default.Foreach("var item in info")
            .AddStatements("var nt = item.NavigateInfo?.NavigateType");
        foreach (var i in includes)
        {
            var caseItem = IfStatement.Default.If($"nt == typeof({i.Item2.NavInfo!.Value.TargetType.ToDisplayString()})")
                 .AddStatement($"{i.Item1.Name}(context, value, item)", "return");
            foreachStatement.AddStatements(caseItem);
        }

        foreach (var i in asyncIncludes)
        {
            var caseItemAsync = IfStatement.Default.If($"nt == typeof({i.Item2.NavInfo!.Value.TargetType.ToDisplayString()})")
                .AddStatement($"await {i.Item1.Name}(context, value, item, cancellationToken)", "return");
            foreachStatementAsync.AddStatements(caseItemAsync);
        }

        handleInclude.AddBody(foreachStatement);
        handleIncludeAsync.AddBody(foreachStatementAsync);
        yield return handleInclude;
        yield return handleIncludeAsync;

        yield break;

        static MethodBuilder CreateIncludeTarget(INamedTypeSymbol owner
            , PropertyScanResult navCol
            , bool isAsync)
        {
            var nav = navCol.NavInfo!.Value;
            var navType = nav.TargetType.ToDisplayString();
            var useAwait = isAsync ? "await " : "";
            var multiResult = isAsync ? "ToListAsync" : "ToList";
            var singleResult = isAsync ? "FirstAsync" : "First";
            var asyncSuffix = isAsync ? "Async" : "";
            var resultMethod = nav.IsMultiResult ? multiResult : singleResult;
            var tokenParameter = isAsync ? ", cancellationToken" : "";
            string methodBody = nav.MappingType is null ? WithoutMappingType() : WithMappingType();
            var meta = navCol.Symbol.Type.MetadataName;
            var mb = MethodBuilder.Default.MethodName($"Include{navCol.PropertyName}{asyncSuffix}")
                .AddParameter("global::LightORM.IContext context", " object value", " global::LightORM.Models.IncludeInfo info")
                .Async(isAsync)
                .AddBody(methodBody);
            if (isAsync)
            {
                mb.ReturnType("global::System.Threading.Tasks.Task");
                mb.AddParameter("global::System.Threading.CancellationToken cancellationToken");
            }

            return mb;

            string WithoutMappingType()
            {
                return $$"""
                         if (value is {{owner.ToDisplayString()}} singleValue)
                                {
                                    var whereExpression = info.IncludeWhereExpression as global::System.Linq.Expressions.Expression<Func<{{navType}}, bool>>;
                                    var includeValue = {{useAwait}}context.Select<{{navType}}>()
                                        .Where(p => p.{{nav.SubName}} == singleValue.{{nav.MainName}})
                                        .WhereIf(whereExpression is not null, whereExpression!)
                                        .{{resultMethod}}();
                                    if ({{(nav.IsMultiResult ? "includeValue.Any()" : "includeValue is not null")}} && info.ThenIncludes?.Count > 0)
                                    {
                                        {{useAwait}}{{nav.TargetType.FormatClassName(true)}}.HandleInclude{{asyncSuffix}}(context, includeValue, info.ThenIncludes{{tokenParameter}});
                                    }
                                    singleValue.{{navCol.PropertyName}} = {{GetAssignExpression("includeValue", navCol.Symbol.Type, nav.IsMultiResult, isAsync)}};
                                }
                                else if (value is IEnumerable<{{owner.ToDisplayString()}}> collectionValue)
                                {
                                    var list = collectionValue as IList<{{owner.ToDisplayString()}}> ?? collectionValue.ToList();
                                    if (list.Count == 0) return;
                                    var whereExpression = info.IncludeWhereExpression as global::System.Linq.Expressions.Expression<Func<{{navType}}, bool>>;
                                    var ids = list.Select(p => p.{{nav.MainName}}).ToList();
                                    var includeValue = ({{useAwait}}context.Select<{{navType}}>()
                                        .Where(p => ids.Contains(p.{{nav.SubName}}))
                                        .WhereIf(whereExpression is not null, whereExpression!)
                                        .{{multiResult}}()).GroupBy(p => p.{{nav.SubName}})
                                        .ToDictionary(g => g.Key, g => g.{{(nav.IsMultiResult ? "ToList" : "First")}}());

                                    if (info.ThenIncludes?.Count > 0 && includeValue.Count > 0)
                                    {
                                        var distinctValues = {{(nav.IsMultiResult ? "includeValue.Values.SelectMany(r => r).Distinct()" : "includeValue.Values")}}.ToList();
                                        {{useAwait}}{{nav.TargetType.FormatClassName(true)}}.HandleInclude{{asyncSuffix}}(context, distinctValues, info.ThenIncludes{{tokenParameter}});
                                    }
                                    
                                    foreach (var u in list)
                                    {
                                        if (includeValue.TryGetValue(u.{{nav.MainName}}, out var value0))
                                        {
                                            u.{{navCol.PropertyName}} = {{GetAssignExpression("value0", navCol.Symbol.Type, nav.IsMultiResult, isAsync)}};
                                        }
                                    }
                                }
                         """;
            }

            string WithMappingType()
            {
                NavigateContext tnav = nav.TargetType.GetProperties().Select(ScanProperty).First(p => EqualityComparer<ITypeSymbol?>.Default.Equals(p.NavInfo?.MappingType, nav.MappingType)).NavInfo ?? throw new System.InvalidOperationException($"导航信息配置错误: 未在[{nav.TargetType.ToDisplayString()}]类型中找到导航信息");

                return $$"""
                         if (value is {{owner.ToDisplayString()}} singleValue)
                                {
                                    var whereExpression = info.IncludeWhereExpression as global::System.Linq.Expressions.Expression<Func<{{navType}}, bool>>;
                                    var includeValue = {{useAwait}}context.Select<{{navType}}>()
                                        .InnerJoin<{{nav.MappingType.ToDisplayString()}}>((p0, p1) => p0.{{tnav.MainName}} == p1.{{tnav.SubName}})
                                        .InnerJoin<{{owner.ToDisplayString()}}>((p0, p1, p2) => p1.{{nav.SubName}} == p2.{{nav.MainName}})
                                        .Where((_, p1, p2) => p2.{{nav.MainName}} == singleValue.{{nav.MainName}})
                                        .WhereIf(whereExpression is not null, whereExpression!)
                                        .{{resultMethod}}();
                                    if ({{(nav.IsMultiResult ? "includeValue.Any()" : "includeValue is not null")}} && info.ThenIncludes?.Count > 0)
                                    {
                                        {{useAwait}}{{nav.TargetType.FormatClassName(true)}}.HandleInclude{{asyncSuffix}}(context, includeValue, info.ThenIncludes{{tokenParameter}});
                                    }
                                    singleValue.{{navCol.PropertyName}} = {{GetAssignExpression("includeValue", navCol.Symbol.Type, nav.IsMultiResult, isAsync)}};
                                }
                                else if (value is IEnumerable<{{owner.ToDisplayString()}}> collectionValue)
                                {
                                    var list = collectionValue as IList<{{owner.ToDisplayString()}}> ?? collectionValue.ToList();
                                    if (list.Count == 0) return;
                                    var whereExpression = info.IncludeWhereExpression as global::System.Linq.Expressions.Expression<Func<{{nav.TargetType}}, bool>>;
                                    var ids = list.Select(p => p.{{nav.MainName}}).ToList();
                                    var includeValue = ({{useAwait}}context.Select<{{nav.TargetType}}>()
                                        .InnerJoin<{{nav.MappingType.ToDisplayString()}}>((p0, p1) => p0.{{tnav.MainName}} == p1.{{tnav.SubName}})
                                        .InnerJoin<{{owner.ToDisplayString()}}>((p0, p1, p2) => p1.{{nav.SubName}} == p2.{{nav.MainName}})
                                        .Where((_, p1, p2) => ids.Contains(p2.{{nav.MainName}}))
                                        .WhereIf(whereExpression is not null, whereExpression!)
                                        .{{multiResult}}((p0, p1, p2) => new { Item = p0, Key = p2.{{nav.MainName}} }))
                                        .GroupBy(v => v.Key)
                                        .ToDictionary(g => g.Key, g => g.Select(gg => gg.Item).{{(nav.IsMultiResult ? "ToList" : "First")}}());
                                    
                                    if (includeValue.Any() && info.ThenIncludes?.Count > 0)
                                    {
                                        var distinctValues = {{(nav.IsMultiResult ? "includeValue.Values.SelectMany(r => r).Distinct()" : "includeValue.Values")}}.ToList();
                                        {{useAwait}}{{nav.TargetType.FormatClassName(true)}}.HandleInclude{{asyncSuffix}}(context, distinctValues, info.ThenIncludes{{tokenParameter}});
                                    }
                                    
                                    foreach (var u in list)
                                    {
                                        if (includeValue.TryGetValue(u.{{nav.MainName}}, out var value0))
                                        {
                                            u.{{navCol.PropertyName}} = {{GetAssignExpression("value0", navCol.Symbol.Type, nav.IsMultiResult, isAsync)}};
                                        }
                                    }
                                }
                         """;
            }

            static string GetAssignExpression(string varName, ITypeSymbol propType, bool isMultiResult, bool isAsync)
            {
                if (!isMultiResult)
                {
                    return varName; // T → T
                }

                if (propType is IArrayTypeSymbol)
                {
                    return $"{varName}.ToArray()";
                }

                if (propType is INamedTypeSymbol named)
                {
                    var elementType = named.TypeArguments[0].ToDisplayString();

                    // IEnumerable<T> → 始终直接赋值
                    if (named.MetadataName == "IEnumerable`1")
                    {
                        return varName;
                    }

                    // IList<T> 实现的接口 + List<T> → 异步直接赋值，同步需 .ToList()
                    if (named.MetadataName is "List`1" or "IList`1" or "ICollection`1"
                        or "IReadOnlyList`1" or "IReadOnlyCollection`1")
                    {
                        return isAsync ? varName : $"{varName}.ToList()";
                    }

                    // HashSet<T> / ISet<T>
                    if (named.MetadataName is "HashSet`1" or "ISet`1")
                    {
                        return $"new global::System.Collections.Generic.HashSet<{elementType}>({varName})";
                    }

                    // ObservableCollection<T>
                    if (named.MetadataName == "ObservableCollection`1")
                    {
                        return $"new global::System.Collections.ObjectModel.ObservableCollection<{elementType}>({varName})";
                    }

                    // SortedSet<T>
                    if (named.MetadataName == "SortedSet`1")
                    {
                        return $"new global::System.Collections.Generic.SortedSet<{elementType}>({varName})";
                    }

                    // Collection<T>
                    if (named.MetadataName == "Collection`1")
                    {
                        return $"new global::System.Collections.ObjectModel.Collection<{elementType}>({varName})";
                    }
                }

                // 回退
                return $"{varName}.ToList()";
            }
        }
    }
}