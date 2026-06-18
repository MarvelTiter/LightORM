using System.Collections.Generic;
using System.Linq;
using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;

namespace LightOrmTableContextGenerator;

public partial class TableContextGenerator
{
    private static void CreateIncludeMethods()
    {
        // 增加Include处理方法

        List<MethodBuilder> includes = [];

        foreach (var p in columns)
        {
            if (!p.NavInfo.HasValue)
            {
                continue;
            }

            var m = CreateIncludeTarget(owner, p);
            includes.Add(m);
            // yield return m;
        }

        var handleInclude = MethodBuilder.Default.MethodName("HandleInclude")
            .AddParameter("global::LightORM.IContext context", "object value", " global::System.Collections.Generic.IEnumerable<global::LightORM.Models.IncludeInfo> info");
        // yield return handleInclude;

        var handleIncludeAsync = MethodBuilder.Default.MethodName("HandleIncludeAsync")
            .ReturnType("global::System.Threading.Tasks.Task")
            .AddParameter("global::LightORM.IContext context", "object value", " global::System.Collections.Generic.IEnumerable<global::LightORM.Models.IncludeInfo> info", "global::System.Threading.CancellationToken cancellationToken")
            .AddBody("return global::System.Threading.Tasks.Task.CompletedTask");
        // yield return handleIncludeAsync;

        // yield break;

        static MethodBuilder CreateIncludeTarget(INamedTypeSymbol owner, PropertyScanResult navCol)
        {
            var nav = navCol.NavInfo.Value;
            var navType = nav.TargetType.ToDisplayString();
            string methodBody = nav.MappingType is null ? WithoutMappingType() : WithMappingType();

            return MethodBuilder.Default.MethodName($"Include{navCol.PropertyName}")
                .AddParameter("global::LightORM.IContext context", " object value", " global::LightORM.Models.IncludeInfo info")
                .AddBody(methodBody);

            string WithoutMappingType()
            {
                return $$"""
                         if (value is {{owner.ToDisplayString()}} singleValue)
                         {
                             var whereExpression = info.IncludeWhereExpression as global::System.Linq.Expressions.Expression<Func<{{navType}}, bool>>;
                             var includeValue = context.Select<{{navType}}>()
                                 .Where(p => p.{{nav.SubName}} == singleValue.{{nav.MainName}})
                                 .WhereIf(whereExpression is not null, whereExpression!)
                                 .{{(nav.IsMultiResult ? "First" : "ToList")}}();
                             if (includeValue is not null && {{(nav.IsMultiResult ? "includeValue.Any() &&" : "")}} info.ThenIncludes?.Count > 0)
                                 {{nav.TargetType.FormatClassName(true)}}.HandleInclude(context, includeValue, info.ThenIncludes);
                             singleValue.{{navCol.PropertyName}} = includeValue;
                         }
                         else if (value is IEnumerable<{{owner.ToDisplayString()}}> collectionValue)
                         {
                             var list = collectionValue as IList<{{owner.ToDisplayString()}}> ?? collectionValue.ToList();
                             if (list.Count == 0) return;
                             var whereExpression = info.IncludeWhereExpression as Expression<Func<{{navType}}, bool>>;
                             var ids = list.Select(p => p.{{nav.MainName}});
                             var includeValue = context.Select<{{navType}}>()
                                 .Where(p => ids.Contains(p.{{nav.SubName}}))
                                 .WhereIf(whereExpression is not null, whereExpression!)
                                 .ToList().GroupBy(p => p.{{nav.SubName}})
                                 .ToDictionary(g => g.Key, g => g.{{(nav.IsMultiResult ? "First" : "ToList")}}());
                             foreach (var u in list)
                             {
                                 if (includeValue.TryGetValue(u.{{nav.MainName}}, out var value0))
                                 {
                                     u.{{navCol.PropertyName}} = value0;
                                 }
                             }

                             if (info.ThenIncludes?.Count > 0 && includeValue.Count > 0)
                             {
                                 var distinctValues = includeValue.Values.SelectMany(r => r).Distinct().ToList();
                                 {{nav.TargetType.FormatClassName(true)}}.HandleInclude(context, distinctValues, info.ThenIncludes);
                             }
                         }
                         """;
            }

            string WithMappingType()
            {
                NavigateContext tnav = nav.TargetType.GetProperties().Select(ScanProperty).First(p => EqualityComparer<ITypeSymbol>.Default.Equals(p.NavInfo?.MappingType, nav.MappingType)).NavInfo.Value;

                return $$"""
                         if (value is {{owner.ToDisplayString()}} singleValue)
                         {
                             var whereExpression = info.IncludeWhereExpression as global::System.Linq.Expressions.Expression<Func<{{navType}}, bool>>;
                             var userRoles = context.Select<{{navType}}>()
                                 .InnerJoin<{{nav.MappingType.ToDisplayString()}}>((p0,p1) => p0.{{tnav.MainName}} == p1.{{tnav.SubName}})
                                 .InnerJoin<User>((_, ur, u) => ur.UserId == u.UserId)
                                 .Where((_, ur, u) => u.UserId == user.UserId)
                                 .WhereIf(whereExpression is not null, whereExpression!)
                                 .ToList();
                             var userUserRoles = userRoles as IList<Role> ?? userRoles.ToList();
                             if (userRoles is not null && userUserRoles.Any() && info.ThenIncludes?.Count > 0)
                             {
                                 LightORMTest_Models_Role.HandleInclude(context, userRoles, info.ThenIncludes);
                             }

                             user.UserRoles = userUserRoles;
                         }
                         else if (value is IEnumerable<User> users)
                         {
                             var userList = users as IList<User> ?? users.ToList();
                             if (userList.Count == 0) return;
                             var whereExpression = info.IncludeWhereExpression as Expression<Func<{{nav.TargetType}}, bool>>;
                             var ids = userList.Select(p => p.UserId);
                             var includeValue = context.Select<{{nav.TargetType}}>()
                                 .InnerJoin<UserRole>((r, ur) => r.RoleId == ur.RoleId)
                                 .InnerJoin<User>((_, ur, u) => ur.UserId == u.UserId)
                                 .Where((_, ur, u) => ids.Contains(u.UserId))
                                 .WhereIf(whereExpression is not null, whereExpression!)
                                 .ToList((r, ur, u) => new { Role = r, u.UserId })
                                 .GroupBy(v => v.UserId)
                                 .ToDictionary(g => g.Key, g => g.Select(gg => gg.Role).ToList());
                             foreach (var u in userList)
                             {
                                 if (includeValue.TryGetValue(u.UserId, out var roles))
                                 {
                                     u.UserRoles = roles;
                                 }
                             }
                             if (includeValue is not null && includeValue.Any() && info.ThenIncludes?.Count > 0)
                             {
                                 var distinctRoles = includeValue.Values.SelectMany(r => r).Distinct().ToList();
                                 LightORMTest_Models_Role.HandleInclude(context, distinctRoles, info.ThenIncludes);
                             }
                         }
                         """;
            }
        }
    }
}