using System;
using System.Collections.Generic;
using System.Linq;
using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;

namespace LightOrmTableContextGenerator;

public partial class TableContextGenerator
{
    private static IEnumerable<MethodBuilder> CreateInitColumnInfoMethod(INamedTypeSymbol owner, IPropertySymbol[] columns)
    {
        List<Statement> bodies = [];
        var i = 0;
        var tableType = $"typeof({owner.ToDisplayString()})";
        List<(IPropertySymbol, PropertyScanResult)> navProps = [];
        foreach (var p in columns)
        {
            // 处理 Flat 属性
            if (p.Type.TypeKind == TypeKind.Class
                && p.Type.SpecialType == SpecialType.None
                && p.HasAttribute(LightFlatAttributeFullName))
            {
                var flattedProps = p.Type.GetMembers().Where(i => i.Kind == SymbolKind.Property && i is IPropertySymbol p && p.DeclaredAccessibility == Accessibility.Public).Cast<IPropertySymbol>();
                var flatType = p.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString();
                foreach (var item in flattedProps)
                {
                    var fr = ScanProperty(item);
                    if (item.Name == p.Name)
                    {
                        throw new InvalidOperationException($"{owner.ToDisplayString()}的聚合属性的子属性[{item.Name}]和聚合属性[{p.Name}]命名冲突");
                    }

                    if (fr.NavInfo != "null")
                    {
                        navProps.Add((item, fr));
                    }

                    var propertyType = $"typeof({item.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString()})";
                    bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType},{propertyType}, "{item.Name}", {fr.CustomName}, {fr.PrimaryKey}, {fr.IsNotMap}, {fr.AutoIncrement}, {fr.NotNull}, {fr.Len}, {fr.DefaultValue}, {fr.Comment}, {fr.CanRead}, {fr.CanWrite}, {fr.CanInit}, {fr.NavInfo}, typeof({flatType}), false, true, false, {fr.IgnoreUpdate}, {fr.IgnoreInsert}, {fr.IsJson})""");
                }

                //bodies.Add($"""var gen_{p.Name} = new global::LightORM.Models.ColumnInfo({tableType}, "{p.Name}", null, false, true, false, false, 0, null, null, true, true, true, null)""");
                //bodies.Add($"gen_{p.Name}.IsAggregated = true");
                bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType},typeof({flatType}), "{p.Name}", null, false, true, false, false, 0, null, null, true, true, true, null, typeof({flatType}), true, false, false, true, true, false)""");
                continue;
            }

            var r = ScanProperty(p);
            if (r.NavInfo != "null")
            {
                navProps.Add((p, r));
            }

            var pt = $"typeof({p.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString()})";
            bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType},{pt}, "{p.Name}", {r.CustomName}, {r.PrimaryKey}, {r.IsNotMap}, {r.AutoIncrement}, {r.NotNull}, {r.Len}, {r.DefaultValue}, {r.Comment}, {r.CanRead}, {r.CanWrite}, {r.CanInit}, {r.NavInfo}, null, false, false, {r.IsVersion}, {r.IgnoreUpdate}, {r.IgnoreInsert}, {r.IsJson})""");
        }

        bodies.Add("return cols");
        bodies =
        [
            $"var cols = new global::LightORM.Interfaces.ITableColumnInfo[{i}]",
            ..bodies
        ];
        yield return MethodBuilder.Default.MethodName("CollectColumnInfo").Modifiers("private static").ReturnType("global::LightORM.Interfaces.ITableColumnInfo[]").AddBody([.. bodies]);

        // 增加Include处理方法

        List<MethodBuilder> includes = [];

        foreach (var p in navProps)
        {
            var m = CreateIncludeTarget(p.Item1, p.Item2);
            includes.Add(m);
            yield return m;
        }

        var handleInclude = MethodBuilder.Default.MethodName("HandleInclude")
            .AddParameter("global::LightORM.IContext context", "object value", " global::System.Collections.Generic.IEnumerable<global::LightORM.Models.IncludeInfo> info");
        yield return handleInclude;

        var handleIncludeAsync = MethodBuilder.Default.MethodName("HandleIncludeAsync")
            .ReturnType("global::System.Threading.Tasks.Task")
            .AddParameter("global::LightORM.IContext context", "object value", " global::System.Collections.Generic.IEnumerable<global::LightORM.Models.IncludeInfo> info");
        yield return handleIncludeAsync;

        yield break;

        static string GetBoolValue(AttributeData? lightCol, string name, Func<string>? elseAction = null)
        {
            var b = "false";
            if (lightCol.GetNamedValue(name, out var v))
            {
                b = $"{v}";
            }
            else
            {
                if (elseAction != null)
                    b = elseAction.Invoke();
            }

            return b.ToLower();
        }

        static string GetAttributeValueOrNull(AttributeData? a, string name, bool isString = false)
        {
            var v = "null";
            if (a.GetNamedValue(name, out var val))
            {
                if (isString)
                {
                    v = $"\"{val}\"";
                }
                else
                {
                    v = $"{v}";
                }
            }

            return v;
        }

        static PropertyScanResult ScanProperty(IPropertySymbol p)
        {
            var commentId = p.GetDocumentationCommentId();
            var commentDoc = p.GetDocumentationCommentXml();
            System.Diagnostics.Debug.WriteLine($"{commentId} -> 注释: {commentDoc}");
            _ = p.GetAttribute("System.ComponentModel.DataAnnotations.Schema.ColumnAttribute", out var cmCol);
            _ = p.GetAttribute("System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute", out var notmap);
            _ = p.GetAttribute("System.ComponentModel.DataAnnotations.KeyAttribute", out var key);
            _ = p.GetAttribute("LightORM.LightColumnAttribute", out var lightCol);
            _ = p.GetAttribute("LightORM.LightNavigateAttribute", out var nav);
            _ = p.GetAttribute(LightJsonMapAttributeFullName, out var json);
            var customName = "null";

            if (lightCol.GetNamedValue("Name", out var cname))
            {
                customName = $"\"{cname}\"";
            }
            else if (cmCol.GetNamedValue("Name", out cname))
            {
                customName = $"\"{cname}\"";
            }

            var primaryKey = GetBoolValue(lightCol, "PrimaryKey", () => key != null ? "true" : "false");
            var isnotmap = GetBoolValue(lightCol, "Ignore", () => notmap != null ? "true" : "false");
            var autoincrement = GetBoolValue(lightCol, "AutoIncrement");
            var notnull = GetBoolValue(lightCol, "NotNull");
            var len = GetAttributeValueOrNull(lightCol, "Length");
            var def = GetAttributeValueOrNull(lightCol, "Default");
            var comment = GetAttributeValueOrNull(lightCol, "Comment", true);
            var canRead = (p.GetMethod is not null) ? "true" : "false";
            var canWrite = (p.SetMethod is not null && p.SetMethod?.IsInitOnly == false) ? "true" : "false";
            var canInit = (p.SetMethod is not null) ? "true" : "false";
            var version = GetBoolValue(lightCol, "Version");
            var ignoreUpdate = GetBoolValue(lightCol, "IgnoreUpdate");
            var ignoreInsert = GetBoolValue(lightCol, "IgnoreInsert");
            var navInfo = "null";
            var isJson = json is null ? "false" : "true";
            if (nav != null)
            {
                var ismulti = p.Type.HasInterfaceAll("System.Collections.IEnumerable") && p.Type.SpecialType == SpecialType.None;
                var multi = ismulti ? "true" : "false";
                var elementType = p.Type.GetElementType();
                if (nav.ConstructorArguments.Length == 2)
                {
                    var mn = nav.ConstructorArguments[0].Value;
                    var sn = nav.ConstructorArguments[1].Value;
                    navInfo = $"new global::LightORM.Models.NavigateInfo(typeof({elementType.ToDisplayString()}), null, \"{mn}\", \"{sn}\", {multi})";
                }
                else if (nav.ConstructorArguments.Length == 3)
                {
                    var mpt = (INamedTypeSymbol)nav.ConstructorArguments[0].Value!;
                    var mn = nav.ConstructorArguments[1].Value;
                    var sn = nav.ConstructorArguments[2].Value;
                    navInfo = $"new global::LightORM.Models.NavigateInfo(typeof({elementType.ToDisplayString()}), typeof({mpt.ToDisplayString()}), \"{mn}\", \"{sn}\", {multi})";
                }
                else
                {
                    var mpt = nav.GetNamedValue("ManyToMany") is not INamedTypeSymbol mappingType ? "null" : $"typeof({mappingType.ToDisplayString()})";
                    var mn = GetAttributeValueOrNull(nav, "MainName", true);
                    var sn = GetAttributeValueOrNull(nav, "SubName", true);
                    navInfo = $"new global::LightORM.Models.NavigateInfo(typeof({elementType.ToDisplayString()}), {mpt}, {mn}, {sn}, {multi})";
                }
            }

            return new()
            {
                CustomName = customName,
                PrimaryKey = primaryKey,
                IsNotMap = isnotmap,
                AutoIncrement = autoincrement,
                NotNull = notnull,
                Len = len,
                DefaultValue = def,
                Comment = comment,
                CanRead = canRead,
                CanWrite = canWrite,
                CanInit = canInit,
                NavInfo = navInfo,
                IsVersion = version,
                IgnoreUpdate = ignoreUpdate,
                IgnoreInsert = ignoreInsert,
                IsJson = isJson
            };
        }

        static MethodBuilder CreateIncludeTarget(IPropertySymbol target, PropertyScanResult scanResult)
        {
            return MethodBuilder.Default.MethodName($"Include{target.MetadataName}")
                .AddParameter("global::LightORM.IContext context", " object value", " global::LightORM.Models.IncludeInfo info");
        }
    }
}