using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightOrmTableContextGenerator;

[Generator(LanguageNames.CSharp)]
public class TableContextGenerator : IIncrementalGenerator
{
    public const string ContextAttributeFullName = "LightORM.LightORMTableContextAttribute";
    public const string ContextInterfaceFullName = "LightORM.ITableContext";
    public const string LightTableAttributeFullName = "LightORM.LightTableAttribute";
    public const string LightFlatAttributeFullName = "LightORM.LightFlatAttribute";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            ContextAttributeFullName
            , static (node, _) => node is ClassDeclarationSyntax
            , (ctx, _) => ctx);

        context.RegisterSourceOutput(source, static (context, source) =>
        {
            var ctxSymbol = (INamedTypeSymbol)source.TargetSymbol;
            //if (ctxSymbol.HasInterface(ContextInterfaceFullName) == false)
            //{
            //    context.ReportDiagnostic(DiagnosticDefinitions.TCG00001(source.TargetNode.GetLocation()));
            //    return;
            //}

            var allTableType = source.SemanticModel.Compilation.GetAllSymbols(LightTableAttributeFullName).ToArray();
            int i = 0;
            List<INamedTypeSymbol> flatTypes = [];
            foreach (var t in allTableType)
            {
                // 生成  {Type}Context.g.cs
                var c = GenerateTypeContextClass(t, i, flatTypes);
                if (c != null)
                {
                    var tt = c.ToString();
                    context.AddSource(c);
                    i++;
                }
            }

            var file = CreateAggregationContextClass(ctxSymbol, allTableType);
            if (file != null)
                context.AddSource(file);
        });

    }

    private static CodeFile? CreateAggregationContextClass(INamedTypeSymbol target, INamedTypeSymbol[] items)
    {
        List<Node> members = [
FieldBuilder.Default
            .MemberType("global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Func<global::LightORM.Interfaces.ITableEntityInfo>>")
            .FieldName("tableInfos"),
           FieldBuilder.Default
            .MemberType("global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Action<global::LightORM.Interfaces.ITableColumnInfo, object, object?>>")
            .FieldName("table_sets"),
            FieldBuilder.Default
            .MemberType("global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Func<global::LightORM.Interfaces.ITableColumnInfo, object, object?>>")
            .FieldName("table_gets"),
];
        List<Statement> dicInits = [
            "tableInfos = new global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Func<global::LightORM.Interfaces.ITableEntityInfo>>()",
            "table_sets = new global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Action<global::LightORM.Interfaces.ITableColumnInfo, object, object?>>()",
            "table_gets = new global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Func<global::LightORM.Interfaces.ITableColumnInfo, object, object?>>()"
            ];
        foreach (var item in items)
        {
            dicInits.Add($"tableInfos.Add(typeof({item.ToDisplayString()}), {item.FormatClassName(true)})");
            dicInits.Add($"table_sets.Add(typeof({item.ToDisplayString()}), {item.FormatClassName(true)}TableInfo.SetValue)");
            dicInits.Add($"table_gets.Add(typeof({item.ToDisplayString()}), {item.FormatClassName(true)}TableInfo.GetValue)");
        }

        var ctor = ConstructorBuilder.Default.MethodName(target.MetadataName)
            .AddBody(
            [.. dicInits]
            );
        members.Add(ctor);

        foreach (var item in items)
        {
            members.Add(MethodBuilder.Default
                .Modifiers("public static")
                .ReturnType("global::LightORM.Interfaces.ITableEntityInfo")
                .MethodName(item.FormatClassName(true))
                .Lambda($"new {item.FormatClassName(true)}TableInfo()"));
        }
        #region GetTableInfoMethod
        {
            List<Statement> methodStatements = [];
            //foreach (var item in items)
            //{
            //    var ifs = IfStatement.Default.If($"type == typeof({item.ToDisplayString()}) || type.IsAssignableFrom(typeof({item.ToDisplayString()}))").AddStatement($"return {item.FormatClassName(true)}");
            //    getTableInfoMethodStatements.Add(ifs);
            //}
            var dicCheck = IfStatement.Default.If("tableInfos.TryGetValue(type, out var factory)").AddStatement("return factory()");

            var notFound = IfStatement.Default.If("type.IsAbstract || type.IsInterface")
                .AddStatement(ForeachStatement.Default.Foreach("var kvp in tableInfos").AddStatements(
                IfStatement.Default.If("kvp.Key.IsAssignableFrom(type)").AddStatement("return kvp.Value()")));
            methodStatements.Add(dicCheck);
            methodStatements.Add(notFound);
            methodStatements.Add("return null");

            var method = MethodBuilder.Default.ReturnType("global::LightORM.Interfaces.ITableEntityInfo?")
                .MethodName("GetTableInfo")
                .AddParameter("Type type")
                .AddBody([.. methodStatements]);

            members.Add(method);
        }

        #endregion

        #region GetSetMethod

        {
            List<Statement> methodStatements = [];
            //foreach (var item in items)
            //{
            //    var ifs = IfStatement.Default.If($"type == typeof({item.ToDisplayString()}) || type.IsAssignableFrom(typeof({item.ToDisplayString()}))").AddStatement($"return {item.FormatClassName(true)}");
            //    getTableInfoMethodStatements.Add(ifs);
            //}
            var dicCheck = IfStatement.Default.If("table_sets.TryGetValue(type, out var factory)").AddStatement("return factory");

            var notFound = ForeachStatement.Default.Foreach("var kvp in table_sets").AddStatements(
                IfStatement.Default.If("kvp.Key.IsAssignableFrom(type)").AddStatement("return kvp.Value"));
            methodStatements.Add(dicCheck);
            methodStatements.Add(notFound);
            methodStatements.Add("return null");

            var method = MethodBuilder.Default.ReturnType("global::System.Action<global::LightORM.Interfaces.ITableColumnInfo, object, object?>?")
                .MethodName("GetSetMethod")
                .AddParameter("Type type")
                .AddBody([.. methodStatements]);

            members.Add(method);
        }

        #endregion

        #region GetGetMethod

        {
            List<Statement> methodStatements = [];
            //foreach (var item in items)
            //{
            //    var ifs = IfStatement.Default.If($"type == typeof({item.ToDisplayString()}) || type.IsAssignableFrom(typeof({item.ToDisplayString()}))").AddStatement($"return {item.FormatClassName(true)}");
            //    getTableInfoMethodStatements.Add(ifs);
            //}
            var dicCheck = IfStatement.Default.If("table_gets.TryGetValue(type, out var factory)").AddStatement("return factory");

            var notFound = ForeachStatement.Default.Foreach("var kvp in table_gets").AddStatements(
                IfStatement.Default.If("kvp.Key.IsAssignableFrom(type)").AddStatement("return kvp.Value"));
            methodStatements.Add(dicCheck);
            methodStatements.Add(notFound);
            methodStatements.Add("return null");

            var method = MethodBuilder.Default.ReturnType("global::System.Func<global::LightORM.Interfaces.ITableColumnInfo, object, object?>?")
                .MethodName("GetGetMethod")
                .AddParameter("Type type")
                .AddBody([.. methodStatements]);

            members.Add(method);
        }

        #endregion

        var ctxClass = ClassBuilder.Default.ClassName(target.FormatClassName())
            .Modifiers("partial")
            .Interface(ContextInterfaceFullName)
            .AddMembers([.. members])
            .AddGeneratedCodeAttribute(typeof(TableContextGenerator));

        var s = ctxClass.ToString();

        return CodeFile.New($"{target.FormatFileName()}.LightORM.g.cs").AddMembers(NamespaceBuilder.Default.Namespace(target.ContainingNamespace.ToDisplayString()).AddMembers(ctxClass)).AddUsings("using LightORM.GeneratedTableContext;");
    }

    private static CodeFile? GenerateTypeContextClass(INamedTypeSymbol target, int index, List<INamedTypeSymbol> flatTypes)
    {
        _ = target.GetAttribute(LightTableAttributeFullName, out var lightTable);
        _ = target.GetAttribute("System.ComponentModel.DataAnnotations.Schema.TableAttribute", out var componentTable);
        _ = target.GetAttribute("System.ComponentModel.DescriptionAttribute", out var des);
        List<Node> members = [
            // private LightORM.Interfaces.ITableColumnInfo[] columns;
            FieldBuilder.Default.Modifiers("private static").MemberType("global::System.Lazy<global::LightORM.Interfaces.ITableColumnInfo[]>").FieldName("columns")
            .InitializeWith("new global::System.Lazy<global::LightORM.Interfaces.ITableColumnInfo[]>(CollectColumnInfo)"),
            // public Type Type { get; } = typeof(Product);
            PropertyBuilder.Default.MemberType("Type").PropertyName("Type").Readonly().InitializeWith($"typeof({target.ToDisplayString()})"),
            // public string TableName => CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常");
            PropertyBuilder.Default.MemberType("string").PropertyName("TableName").Lambda("""CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常")"""),
            // public string? Alias { get; set; } = "a1";
            PropertyBuilder.Default.MemberType("string?").PropertyName("Alias").InitializeWith($"\"a{index}\""),
            // public bool IsAnonymousType => false;
            PropertyBuilder.Default.MemberType("bool").PropertyName("IsAnonymousType").InitializeWith("false"),
            PropertyBuilder.Default.MemberType("bool").PropertyName("IsTempTable").InitializeWith("false")
        ];

        // public string? CustomName { get; set; }
        var customNameProperty = PropertyBuilder.Default.MemberType("string?").PropertyName("CustomName");
        if (lightTable.GetNamedValue("Name", out var customName))
        {
            customNameProperty = customNameProperty.InitializeWith($"\"{customName}\"");
        }
        else
        {
            if (componentTable.GetNamedValue("Name", out customName))
            {
                customNameProperty = customNameProperty.InitializeWith($"\"{customName}\"");
            }
        }

        members.Add(customNameProperty);

        // public string? TargetDatabase => null;
        lightTable.GetNamedValue("DatabaseKey", out var dbKey);
        members.Add(PropertyBuilder.Default.MemberType("string?").PropertyName("TargetDatabase").Lambda($"{(dbKey == null ? "null" : $"\"{dbKey}\"")}"));

        // public string? Description => null;
        var desValue = "null";
        if (des?.GetNamedValue("Description", out var description) == true)
        {
            desValue = description!.ToString();
        }
        else if (des?.GetConstructorValue(0, out description) == true)
        {
            desValue = description!.ToString();
        }
        members.Add(PropertyBuilder.Default.MemberType("string?").PropertyName("Description").Lambda(desValue));
        // public List<ColumnInfo> Columns { get; } = [];
        members.Add(PropertyBuilder.Default.MemberType("global::LightORM.Interfaces.ITableColumnInfo[]").PropertyName("Columns").Lambda("columns.Value"));

        var columns = target.GetMembers().Where(i => i.Kind == SymbolKind.Property && i is IPropertySymbol p && p.DeclaredAccessibility == Accessibility.Public).Cast<IPropertySymbol>().ToArray();
        
        // GetValue   object? GetValue(ColumnInfo col, object target);
        members.Add(CreateGetValueMethod(target, columns));
        // SetValue   void SetValue(ColumnInfo col, object target, object? value)
        members.Add(CreateSetValueMethod(target, columns));

        members.Add(CreateInitColumnInfoMethod(target, columns));

        var r = ClassBuilder.Default.MakeRecord().ClassName($"{target.FormatClassName(true)}TableInfo")
            .Interface("global::LightORM.Interfaces.ITableEntityInfo")
            .AddGeneratedCodeAttribute(typeof(TableContextGenerator))
            .AddMembers([.. members]);

        //var s = r.ToString();

        return CodeFile.New($"{target.FormatFileName()}.TableInfo.g.cs")
            .AddMembers(NamespaceBuilder.Default.Namespace("LightORM.GeneratedTableContext").AddMembers(r));
    }

    private static MethodBuilder CreateInitColumnInfoMethod(INamedTypeSymbol owner, IPropertySymbol[] columns)
    {
        //var columns = target.GetMembers().Where(i => i.Kind == SymbolKind.Property && i is IPropertySymbol p && p.DeclaredAccessibility == Accessibility.Public).Cast<IPropertySymbol>().ToArray();

        List<Statement> bodies = [];
        var i = 0;
        var tableType = $"typeof({owner.ToDisplayString()})";
        foreach (var p in columns)
        {
            if (p.Type.TypeKind == TypeKind.Class
                && p.Type.SpecialType == SpecialType.None
                && p.HasAttribute(LightFlatAttributeFullName))
            {
                var flattedProps = p.Type.GetMembers().Where(i => i.Kind == SymbolKind.Property && i is IPropertySymbol p && p.DeclaredAccessibility == Accessibility.Public).Cast<IPropertySymbol>();
                foreach (var item in flattedProps)
                {
                    var fr = ScanProperty(p);
                    bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType}, "{item.Name}", {fr.CustomName}, {fr.PrimaryKey}, {fr.IsNotMap}, {fr.AutoIncrement}, {fr.NotNull}, {fr.Len}, {fr.DefaultValue}, {fr.Comment}, {fr.CanRead}, {fr.CanWrite}, {fr.CanInit}, {fr.NavInfo}, typeof({p.Type.ToDisplayString()}), false, true)""");
                }
                //bodies.Add($"""var gen_{p.Name} = new global::LightORM.Models.ColumnInfo({tableType}, "{p.Name}", null, false, true, false, false, 0, null, null, true, true, true, null)""");
                //bodies.Add($"gen_{p.Name}.IsAggregated = true");
                bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType}, "{p.Name}", null, false, true, false, false, 0, null, null, true, true, true, null, typeof({p.Type.ToDisplayString()}), true, false)""");
                continue;
            }
            var r = ScanProperty(p);
            bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType}, "{p.Name}", {r.CustomName}, {r.PrimaryKey}, {r.IsNotMap}, {r.AutoIncrement}, {r.NotNull}, {r.Len}, {r.DefaultValue}, {r.Comment}, {r.CanRead}, {r.CanWrite}, {r.CanInit}, {r.NavInfo}, null, false, false)""");
        }
        bodies.Add("return cols");
        bodies = [
            $"var cols = new global::LightORM.Interfaces.ITableColumnInfo[{i}]",
            ..bodies
            ];
        return MethodBuilder.Default.MethodName("CollectColumnInfo").Modifiers("private static").ReturnType("global::LightORM.Interfaces.ITableColumnInfo[]").AddBody([.. bodies]);

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
            _ = p.GetAttribute("System.ComponentModel.DataAnnotations.Schema.ColumnAttribute", out var cmCol);
            _ = p.GetAttribute("System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute", out var notmap);
            _ = p.GetAttribute("System.ComponentModel.DataAnnotations.KeyAttribute", out var key);
            _ = p.GetAttribute("LightORM.LightColumnAttribute", out var lightCol);
            _ = p.GetAttribute("LightORM.IgnoreAttribute", out var ignore);
            _ = p.GetAttribute("LightORM.LightNavigateAttribute", out var nav);
            var customName = "null";

            if (lightCol.GetNamedValue("Name", out var cname))
            {
                customName = $"\"{cname}\"";
            }
            else if (cmCol.GetNamedValue("Name", out cname))
            {
                customName = $"\"{cname}\"";
            }
            var primaryKey = GetBoolValue(lightCol, "PrimaryKey", () =>
            {
                return key == null ? "false" : "true";
            });
            var isnotmap = (ignore != null || notmap != null) ? "true" : "false";
            var autoincrement = GetBoolValue(lightCol, "AutoIncrement");
            var notnull = GetBoolValue(lightCol, "NotNull");
            var len = GetAttributeValueOrNull(lightCol, "Length");
            var def = GetAttributeValueOrNull(lightCol, "Default");
            var comment = GetAttributeValueOrNull(lightCol, "Comment", true);
            var canRead = (p.GetMethod is not null) ? "true" : "false";
            var canWrite = (p.SetMethod is not null && p.SetMethod?.IsInitOnly == false) ? "true" : "false";
            var canInit = (p.SetMethod is not null) ? "true" : "false";
            var navInfo = "null";
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
                    var mappingType = nav.GetNamedValue("ManyToMany") as INamedTypeSymbol;
                    var mpt = mappingType == null ? "null" : $"typeof({mappingType.ToDisplayString()})";
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
                NavInfo = navInfo
            };
        }
    }

    private static MethodBuilder CreateGetValueMethod(INamedTypeSymbol target, IPropertySymbol[] columns)
    {
        List<Statement> bodies = [
            $"var p = target as {target.ToDisplayString()}",
            "ArgumentNullException.ThrowIfNull(p)",
            "if (!col.CanRead)",
            "   return null"
         ];


        var builder = MethodBuilder.Default
            .Modifiers("public static")
            .MethodName("GetValue")
            .ReturnType("object?")
            .AddParameter("global::LightORM.Interfaces.ITableColumnInfo col", "object target")
            .AddBody([.. bodies]);

        builder.AddSwitchStatement("col.PropertyName", ss =>
        {
            foreach (IPropertySymbol column in columns)
            {
                if (column.Type.TypeKind == TypeKind.Class
                && column.Type.SpecialType == SpecialType.None
                && column.HasAttribute(LightFlatAttributeFullName))
                {
                    var flatProps = column.Type.GetMembers().Where(i => i.Kind == SymbolKind.Property && i is IPropertySymbol p && p.DeclaredAccessibility == Accessibility.Public).Cast<IPropertySymbol>();
                    foreach (var flat in flatProps)
                    {
                        ss.AddReturnCase($"\"{flat.Name}\"", $"p.{column.Name}.{flat.Name}");
                    }
                }
                else
                {
                    ss.AddReturnCase($"\"{column.Name}\"", $"p.{column.Name}");
                }
            }
            ss.AddDefaultCase("throw new ArgumentException()");
        });

        return builder;
    }

    private static MethodBuilder CreateSetValueMethod(INamedTypeSymbol target, IPropertySymbol[] columns)
    {
        List<Statement> bodies = [
            $"var p = target as {target.ToDisplayString()}",
            "ArgumentNullException.ThrowIfNull(p)",
            "if (!col.CanWrite)",
            "   return",
            "if (value == null)",
            "   return"
         ];
        var method = MethodBuilder.Default
            .Modifiers("public static")
            .MethodName("SetValue")
            .AddParameter("global::LightORM.Interfaces.ITableColumnInfo col", "object target", "object? value")
            .AddBody([.. bodies]);

        method.AddSwitchStatement("col.PropertyName", ss =>
        {
            foreach (var column in columns)
            {
                if (column.IsReadOnly || column.SetMethod?.IsInitOnly == true)
                    continue;
                if (column.Type.TypeKind == TypeKind.Class
                && column.Type.SpecialType == SpecialType.None
                && column.HasAttribute(LightFlatAttributeFullName))
                {
                    var flatProps = column.Type.GetMembers().Where(i => i.Kind == SymbolKind.Property && i is IPropertySymbol p && p.DeclaredAccessibility == Accessibility.Public).Cast<IPropertySymbol>();
                    foreach (var flat in flatProps)
                    {
                        ss.AddBreakCase($"\"{flat.Name}\"",
                            //$"p.{column.Name}.{flat.Name}",
                            IfStatement.Default.If($"p.{column.Name} is null")
                            .AddStatement($"p.{column.Name} = new {column.Type.ToDisplayString()}();"),
                            $"p.{column.Name}.{flat.Name} = ({flat.Type.ToDisplayString()})value"
                            );
                    }
                }
                else
                {
                    ss.AddBreakCase($"\"{column.Name}\"", $"p.{column.Name} = ({column.Type.ToDisplayString()})value");
                }
            }
            ss.AddDefaultCase("throw new ArgumentException()");
        });

        return method;
    }
}
