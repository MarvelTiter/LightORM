using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;

namespace LightOrmTableContextGenerator;

public partial class TableContextGenerator
{
    private static MethodBuilder CreateInitColumnInfoMethod(INamedTypeSymbol owner, PropertyScanResult[] columns)
    {
        List<Statement> bodies = [];
        var i = 0;
        var tableType = $"typeof({owner.ToDisplayString()})";
        // foreach (var p in columns)
        // {
        //     // 处理 Flat 属性
        //     if (p.Type.TypeKind == TypeKind.Class
        //         && p.Type.SpecialType == SpecialType.None
        //         && p.HasAttribute(LightFlatAttributeFullName))
        //     {
        //         var flattedProps = p.Type.GetMembers().Where(ii => ii.Kind == SymbolKind.Property && ii is IPropertySymbol { DeclaredAccessibility: Accessibility.Public }).Cast<IPropertySymbol>();
        //         var flatType = p.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString();
        //         foreach (var item in flattedProps)
        //         {
        //             var fr = ScanProperty(item);
        //             if (item.Name == p.Name)
        //             {
        //                 throw new InvalidOperationException($"{owner.ToDisplayString()}的聚合属性的子属性[{item.Name}]和聚合属性[{p.Name}]命名冲突");
        //             }
        //
        //             allFields.Add((item, fr));
        //             var propertyType = $"typeof({item.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString()})";
        //             bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType},{propertyType}, "{item.Name}", {fr.CustomName}, {fr.PrimaryKey}, {fr.IsNotMap}, {fr.AutoIncrement}, {fr.NotNull}, {fr.Len}, {fr.DefaultValue}, {fr.Comment}, {fr.CanRead}, {fr.CanWrite}, {fr.CanInit}, {fr.NavInfo}, typeof({flatType}), false, true, false, {fr.IgnoreUpdate}, {fr.IgnoreInsert}, {fr.IsJson})""");
        //         }
        //
        //         bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType},typeof({flatType}), "{p.Name}", null, false, true, false, false, 0, null, null, true, true, true, null, typeof({flatType}), true, false, false, true, true, false)""");
        //         continue;
        //     }
        //
        //     var r = ScanProperty(p);
        //     allFields.Add((p, r));
        //     var pt = $"typeof({p.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString()})";
        //     bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType},{pt}, "{p.Name}", {r.CustomName}, {r.PrimaryKey}, {r.IsNotMap}, {r.AutoIncrement}, {r.NotNull}, {r.Len}, {r.DefaultValue}, {r.Comment}, {r.CanRead}, {r.CanWrite}, {r.CanInit}, {r.NavInfo}, null, false, false, {r.IsVersion}, {r.IgnoreUpdate}, {r.IgnoreInsert}, {r.IsJson})""");
        // }
        foreach (var pr in columns)
        {
            var nav = pr.NavInfo.HasValue ? $"{pr.NavInfo.Value}" : "null";
            var propertyType = $"typeof({pr.Symbol.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString()})";
            if (pr.IsFlat)
            {
                if (pr.IsFlatProperty)
                {
                    bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType},{propertyType}, "{pr.PropertyName}", {pr.CustomName}, {pr.PrimaryKey}, {pr.IsNotMap}, {pr.AutoIncrement}, {pr.NotNull}, {pr.Len}, {pr.DefaultValue}, {pr.Comment}, {pr.CanRead}, {pr.CanWrite}, {pr.CanInit}, {nav}, typeof({pr.FlatType}), false, true, false, {pr.IgnoreUpdate}, {pr.IgnoreInsert}, {pr.IsJson})""");
                }
                else
                {
                    bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType},typeof({pr.FlatType}), "{pr.PropertyName}", null, false, true, false, false, 0, null, null, true, true, true, null, typeof({pr.FlatType}), true, false, false, true, true, false)""");
                }
            }
            else
            {
                bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType},{propertyType}, "{pr.PropertyName}", {pr.CustomName}, {pr.PrimaryKey}, {pr.IsNotMap}, {pr.AutoIncrement}, {pr.NotNull}, {pr.Len}, {pr.DefaultValue}, {pr.Comment}, {pr.CanRead}, {pr.CanWrite}, {pr.CanInit}, {nav}, null, false, false, {pr.IsVersion}, {pr.IgnoreUpdate}, {pr.IgnoreInsert}, {pr.IsJson})""");
            }
        }

        bodies.Add("return cols");
        bodies =
        [
            $"var cols = new global::LightORM.Interfaces.ITableColumnInfo[{i}]",
            ..bodies
        ];
        return MethodBuilder.Default.MethodName("CollectColumnInfo").Modifiers("private static").ReturnType("global::LightORM.Interfaces.ITableColumnInfo[]").AddBody([.. bodies]);
    }
}