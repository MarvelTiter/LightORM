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
            foreach (var t in allTableType)
            {
                // 生成  {Type}Context.g.cs
                var c = GenerateTypeContextClass(t, i);
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
        List<Node> members = [];

        foreach (var item in items)
        {
            members.Add(PropertyBuilder.Default
                .Modifiers("public static")
                .MemberType("global::LightORM.Interfaces.ITableEntityInfo")
                .PropertyName(item.FormatClassName(true))
                .Lambda($"new {item.FormatClassName(true)}Context()"));
        }
        List<Statement> statements = [];
        foreach (var item in items)
        {
            var ifs = IfStatement.Default.If($"type == typeof({item.ToDisplayString()}) || type.IsAssignableFrom(typeof({item.ToDisplayString()}))").AddStatement($"return {item.FormatClassName(true)}");
            statements.Add(ifs);
        }
        statements.Add("return null");
        var method = MethodBuilder.Default.ReturnType("global::LightORM.Interfaces.ITableEntityInfo?")
            .MethodName("GetTableInfo")
            .AddParameter("Type type")
            .AddBody([.. statements]);

        members.Add(method);

        var ctxClass = ClassBuilder.Default.ClassName(target.FormatClassName())
            .Modifiers("partial")
            .Interface(ContextInterfaceFullName)
            .AddMembers([.. members])
            .AddGeneratedCodeAttribute(typeof(TableContextGenerator));

        var s = ctxClass.ToString();

        return CodeFile.New($"{target.FormatFileName()}.LightORM.g.cs").AddMembers(NamespaceBuilder.Default.Namespace(target.ContainingNamespace.ToDisplayString()).AddMembers(ctxClass)).AddUsings("using LightORM.GeneratedTableContext;");
    }

    private static CodeFile? GenerateTypeContextClass(INamedTypeSymbol target, int index)
    {

        _ = target.GetAttribute(LightTableAttributeFullName, out var lightTable);
        _ = target.GetAttribute("System.ComponentModel.DataAnnotations.Schema.TableAttribute", out var componentTable);
        _ = target.GetAttribute("System.ComponentModel.DescriptionAttribute", out var des);
        List<Node> members = [
            // private LightORM.Interfaces.ITableColumnInfo[] columns;
            FieldBuilder.Default.Modifiers("private").MemberType("global::LightORM.Interfaces.ITableColumnInfo[]?").FieldName("columns"),
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
        members.Add(PropertyBuilder.Default.MemberType("global::LightORM.Interfaces.ITableColumnInfo[]").PropertyName("Columns").Lambda("columns ??= CollectColumnInfo()"));

        var columns = target.GetMembers().Where(i => i.Kind == SymbolKind.Property && i is IPropertySymbol p && p.DeclaredAccessibility == Accessibility.Public).Cast<IPropertySymbol>().ToArray();

        // GetValue   object? GetValue(ColumnInfo col, object target);
        members.Add(CreateGetValueMethod(target, columns));
        // SetValue   void SetValue(ColumnInfo col, object target, object? value)
        members.Add(CreateSetValueMethod(target, columns));

        members.Add(CreateInitColumnInfoMethod(columns));

        var r = ClassBuilder.Default.MakeRecord().ClassName($"{target.FormatClassName(true)}Context")
            .Interface("global::LightORM.Interfaces.ITableEntityInfo")
            .AddGeneratedCodeAttribute(typeof(TableContextGenerator))
            .AddMembers([.. members]);

        //var s = r.ToString();

        return CodeFile.New($"{target.FormatFileName()}.Context.g.cs")
            .AddMembers(NamespaceBuilder.Default.Namespace("LightORM.GeneratedTableContext").AddMembers(r));
    }

    private static MethodBuilder CreateInitColumnInfoMethod(IPropertySymbol[] columns)
    {
        //var columns = target.GetMembers().Where(i => i.Kind == SymbolKind.Property && i is IPropertySymbol p && p.DeclaredAccessibility == Accessibility.Public).Cast<IPropertySymbol>().ToArray();

        List<Statement> bodies = [
            $"columns = new global::LightORM.Interfaces.ITableColumnInfo[{columns.Length}]"
            ];
        var i = 0;
        foreach (var p in columns)
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
            bodies.Add($"""columns[{i++}] = new global::LightORM.Models.ColumnInfo(this, "{p.Name}", {customName}, {primaryKey}, {isnotmap}, {autoincrement}, {notnull}, {len}, {def}, {comment}, {canRead}, {canWrite}, {canInit}, {navInfo})""");
        }
        bodies.Add("return columns");

        return MethodBuilder.Default.MethodName("CollectColumnInfo").Modifiers("private").ReturnType("global::LightORM.Interfaces.ITableColumnInfo[]").AddBody([.. bodies]);

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
    }

    private static MethodBuilder CreateGetValueMethod(INamedTypeSymbol target, IPropertySymbol[] columns)
    {
        List<Statement> bodies = [
            $"var p = target as {target.ToDisplayString()}",
            "ArgumentNullException.ThrowIfNull(p)",
            "if (!col.CanRead)",
            "   return null"
         ];


        var builder = MethodBuilder.Default.MethodName("GetValue").ReturnType("object?").AddParameter("global::LightORM.Interfaces.ITableColumnInfo col", "object target").AddBody([.. bodies]);

        builder.AddSwitchStatement("col.PropertyName", ss =>
        {
            foreach (var column in columns)
            {
                ss.AddReturnCase($"\"{column.Name}\"", $"p.{column.Name}");
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
        var method = MethodBuilder.Default.MethodName("SetValue").AddParameter("global::LightORM.Interfaces.ITableColumnInfo col", "object target", "object? value").AddBody([.. bodies]);

        method.AddSwitchStatement("col.PropertyName", ss =>
        {
            foreach (var column in columns)
            {
                if (column.IsReadOnly || column.SetMethod?.IsInitOnly == true)
                    continue;
                ss.AddBreakCase($"\"{column.Name}\"", $"p.{column.Name} = ({column.Type.ToDisplayString()})value");
            }
            ss.AddDefaultCase("throw new ArgumentException()");
        });

        return method;
    }
}
