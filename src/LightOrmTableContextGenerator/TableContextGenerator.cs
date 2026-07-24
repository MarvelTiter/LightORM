using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LightOrmTableContextGenerator;

[Generator(LanguageNames.CSharp)]
public partial class TableContextGenerator : IIncrementalGenerator
{
    public const string ContextAttributeFullName = "LightORM.LightORMTableContextAttribute";
    public const string ContextInterfaceFullName = "LightORM.ITableContext";
    public const string LightTableAttributeFullName = "LightORM.LightTableAttribute";
    public const string LightFlatAttributeFullName = "LightORM.LightFlatAttribute";
    public const string LightJsonMapAttributeFullName = "LightORM.LightJsonMapAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            ContextAttributeFullName
            , static (node, _) => node is ClassDeclarationSyntax
            , (ctx, _) => ctx);

        context.RegisterSourceOutput(source, static (context, source) =>
        {
            try
            {
                var ctxSymbol = (INamedTypeSymbol)source.TargetSymbol;
                var allTableType = source.SemanticModel.Compilation.GetAllSymbols(LightTableAttributeFullName).ToArray();
                int i = 0;
                List<INamedTypeSymbol> flatTypes = [];
                //var customMap = ctxSymbol.GetAttributes("LightORM.Attributes.Mapping.MappingBaseAttribute", true).ToArray();
                var staticUsing = $"using static {ctxSymbol.ContainingNamespace.ToDisplayString()}.{ctxSymbol.FormatClassName()};";
                foreach (var t in allTableType)
                {
                    // 生成  {Type}Context.g.cs
                    var c = GenerateTypeContextClass(t, staticUsing, flatTypes);
                    if (c != null)
                    {
                        //var tt = c.ToString();
                        context.AddSource(c);
                        i++;
                    }
                }

                var file = TableContextGeneratorHelpers.CreateAggregationContextClass(ctxSymbol, allTableType);
                if (file != null)
                    context.AddSource(file);
            }
            catch (Exception ex)
            {
                //var message = $"""
                //{ex.Message}
                //{ex.StackTrace}
                //""";
                context.ReportDiagnostic(DiagnosticDefinitions.TCG00002(source.TargetNode.GetLocation(), ex.Message));
            }
        });
    }

    private static CodeFile? GenerateTypeContextClass(INamedTypeSymbol target, string staticUsing, List<INamedTypeSymbol> flatTypes)
    {
        _ = target.GetAttribute(LightTableAttributeFullName, out var lightTable);
        _ = target.GetAttribute("System.ComponentModel.DataAnnotations.Schema.TableAttribute", out var componentTable);
        _ = target.GetAttribute("System.ComponentModel.DescriptionAttribute", out var des);
        List<Node> members =
        [
            // private LightORM.Interfaces.ITableColumnInfo[] columns;
            FieldBuilder.Default.Modifiers("private static").MemberType("global::System.Lazy<global::LightORM.Interfaces.ITableColumnInfo[]>").FieldName("columns")
                .InitializeWith("new global::System.Lazy<global::LightORM.Interfaces.ITableColumnInfo[]>(CollectColumnInfo)"),
            // public Type Type { get; } = typeof(Product);
            PropertyBuilder.Default.MemberType("Type").PropertyName("Type").Readonly().InitializeWith($"typeof({target.ToDisplayString()})"),
            // public string TableName => CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常");
            PropertyBuilder.Default.MemberType("string").PropertyName("TableName").Lambda("""CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常")"""),
            // public string? Alias { get; set; } = "a1";
            //PropertyBuilder.Default.MemberType("string?").PropertyName("Alias").InitializeWith($"\"a{index}\""),
            // public bool IsAnonymousType => false;
            PropertyBuilder.Default.MemberType("bool").PropertyName("IsAnonymousType").InitializeWith("false"),
            PropertyBuilder.Default.MemberType("bool").PropertyName("IsTempTable").InitializeWith("false")
        ];

        // public string? CustomName { get; set; }
        var customNameProperty = PropertyBuilder.Default.MemberType("string?").PropertyName("CustomName");
        if (lightTable.GetNamedValue("Name", out var customName))
        {
            customNameProperty.InitializeWith($"\"{customName}\"");
        }
        else
        {
            if (componentTable.GetNamedValue("Name", out customName))
            {
                customNameProperty.InitializeWith($"\"{customName}\"");
            }
        }

        members.Add(customNameProperty);

        // public string? TargetDatabase => null;
        lightTable.GetNamedValue("DatabaseKey", out var dbKey);
        lightTable.GetNamedValue("Schema", out var schema);
        members.Add(PropertyBuilder.Default.MemberType("string?").PropertyName("TargetDatabase").Lambda($"{(dbKey == null ? "null" : $"\"{dbKey}\"")}"));
        members.Add(PropertyBuilder.Default.MemberType("string?").PropertyName("Schema").Lambda($"{(schema == null ? "null" : $"\"{schema}\"")}"));
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

        var columns = target.GetAllMembers(s => s.IsAbstract)
            .Where(i => i.Kind == SymbolKind.Property && i is IPropertySymbol { DeclaredAccessibility: Accessibility.Public })
            .Cast<IPropertySymbol>().ToArray();

        // Func<IDataReader, object>? DataReaderDeserializer { get; }
        var columnInfos = CollectProperties(target, columns).ToArray();
        MethodBuilder staticDeserializerMethod = CreateDeserializeMethod(target, columnInfos);
        var deserializer = PropertyBuilder.Default
            .PropertyName("DataReaderDeserializer")
            .MemberType($"global::System.Func<global::System.Data.IDataReader, {target.ToDisplayString()}>?")
            .Lambda(staticDeserializerMethod.Name!);


        members.Add(CreateInitColumnInfoMethod(target, columnInfos));
        members.AddRange(CreateIncludeMethods(target, columnInfos));
        members.Add(deserializer);
        members.Add(staticDeserializerMethod);
        // GetValue   object? GetValue(ColumnInfo col, object target);
        members.Add(CreateGetValueMethod(target, columns));
        // SetValue   void SetValue(ColumnInfo col, object target, object? value)
        members.Add(CreateSetValueMethod(target, columns));
        // static global::LightORM.Interfaces.ITableColumnInfo[] CollectColumnInfo()

        var r = ClassBuilder.Default.MakeRecord().ClassName($"{target.FormatClassName(true)}TableInfo")
            .Interface("global::LightORM.Interfaces.ITableEntityInfo")
            .Interface($"global::LightORM.Interfaces.ITableEntityInfo<{target.ToDisplayString()}>")
            .AddGeneratedCodeAttribute(typeof(TableContextGenerator))
            .AddMembers([.. members]);

        return CodeFile.New($"{target.FormatFileName()}.TableInfo.g.cs")
            .AddMembers(NamespaceBuilder.Default.Namespace("LightORM.GeneratedTableContext").AddMembers(r).FileScoped())
            .AddUsings(staticUsing);
    }

    private static MethodBuilder CreateGetValueMethod(INamedTypeSymbol target, IPropertySymbol[] columns)
    {
        List<Statement> bodies =
        [
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
                    var nullable = column.Type.NullableAnnotation == NullableAnnotation.Annotated ? "?" : "";
                    foreach (var flat in flatProps)
                    {
                        ss.AddReturnCase($"\"{flat.Name}\"", $"p.{column.Name}{nullable}.{flat.Name}");
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
        List<Statement> bodies =
        [
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
                                .AddStatement($"p.{column.Name} = {column.Type.New()};"),
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

    private static IEnumerable<PropertyScanResult> CollectProperties(INamedTypeSymbol owner, IPropertySymbol[] props)
    {
        foreach (var p in props)
        {
            // 处理 Flat 属性
            if (p.Type.TypeKind == TypeKind.Class
                && p.Type.SpecialType == SpecialType.None
                && p.HasAttribute(LightFlatAttributeFullName))
            {
                var flattedProps = p.Type.GetMembers().Where(ii => ii.Kind == SymbolKind.Property && ii is IPropertySymbol { DeclaredAccessibility: Accessibility.Public }).Cast<IPropertySymbol>();
                var flatType = p.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString();
                foreach (var item in flattedProps)
                {
                    var fr = ScanProperty(item);
                    if (item.Name == p.Name)
                    {
                        throw new InvalidOperationException($"{owner.ToDisplayString()}的聚合属性的子属性[{item.Name}]和聚合属性[{p.Name}]命名冲突");
                    }

                    yield return fr with { IsFlat = true, FlatType = flatType, IsFlatProperty = true };
                }

                yield return new()
                {
                    Symbol = p,
                    PropertyName = p.Name,
                    IsFlat = true,
                    FlatType = flatType,
                    IsFlatProperty = false
                };
                continue;
            }

            yield return ScanProperty(p);
        }
    }

    private static PropertyScanResult ScanProperty(IPropertySymbol p)
    {
        //var commentId = p.GetDocumentationCommentId();
        //var commentDoc = p.GetDocumentationCommentXml();
        //System.Diagnostics.Debug.WriteLine($"{commentId} -> 注释: {commentDoc}");
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
        NavigateContext? navInfo = null;
        var isJson = json is null ? "false" : "true";
        if (nav != null)
        {
            var ismulti = p.Type.HasInterfaceAll("System.Collections.IEnumerable") && p.Type.SpecialType == SpecialType.None;
            var multi = ismulti ? "true" : "false";
            var elementType = p.Type.GetElementType();
            if (nav.ConstructorArguments.Length == 2)
            {
                var mn = nav.ConstructorArguments[0].Value!.ToString();
                var sn = nav.ConstructorArguments[1].Value!.ToString();
                // navInfo = $"new global::LightORM.Models.NavigateInfo(typeof({elementType.ToDisplayString()}), null, \"{mn}\", \"{sn}\", {multi})";
                navInfo = new(elementType, mn, sn, ismulti);
            }
            else if (nav.ConstructorArguments.Length == 3)
            {
                var mpt = (INamedTypeSymbol)nav.ConstructorArguments[0].Value!;
                var mn = nav.ConstructorArguments[1].Value!.ToString();
                var sn = nav.ConstructorArguments[2].Value!.ToString();
                // navInfo = $"new global::LightORM.Models.NavigateInfo(typeof({elementType.ToDisplayString()}), typeof({mpt.ToDisplayString()}), \"{mn}\", \"{sn}\", {multi})";
                navInfo = new(elementType, mn, sn, ismulti, mpt);
            }
            else
            {
                // var mpt = nav.GetNamedValue("ManyToMany") is not INamedTypeSymbol mappingType ? "null" : $"typeof({mappingType.ToDisplayString()})";
                var mappingType = nav.GetNamedValue("ManyToMany") as INamedTypeSymbol;
                var mn = GetAttributeValueOrNull(nav, "MainName", true, true);
                var sn = GetAttributeValueOrNull(nav, "SubName", true, true);
                // navInfo = $"new global::LightORM.Models.NavigateInfo(typeof({elementType.ToDisplayString()}), {mpt}, {mn}, {sn}, {multi})";
                navInfo = new(elementType, mn, sn, ismulti, mappingType);
            }
        }

        return new()
        {
            Symbol = p,
            PropertyName = p.Name,
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

        static string GetAttributeValueOrNull(AttributeData? a, string name, bool isString = false, bool raw = false)
        {
            var v = "null";
            if (a.GetNamedValue(name, out var val))
            {
                if (isString)
                {
                    if (raw) return val!.ToString();
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


    static readonly Dictionary<SpecialType, string> typeMapMethod = new(37)
    {
        [SpecialType.System_Byte] = "GetByte",
        [SpecialType.System_SByte] = "GetByte",
        [SpecialType.System_Int16] = "GetInt16",
        [SpecialType.System_UInt16] = "global::LightORM.Utils.DataRecordFieldHandleHelper.RecordFieldToUInt16",
        [SpecialType.System_Int32] = "GetInt32",
        [SpecialType.System_UInt32] = "global::LightORM.Utils.DataRecordFieldHandleHelper.RecordFieldToUInt32",
        [SpecialType.System_Int64] = "GetInt64",
        [SpecialType.System_UInt64] = "global::LightORM.Utils.DataRecordFieldHandleHelper.RecordFieldToUInt64",
        [SpecialType.System_Single] = "GetFloat",
        [SpecialType.System_Double] = "GetDouble",
        [SpecialType.System_Decimal] = "GetDecimal",
        [SpecialType.System_Boolean] = "GetBoolean",
        [SpecialType.System_String] = "GetString",
        [SpecialType.System_Char] = "GetChar",
        [SpecialType.System_DateTime] = "GetDateTime",
        [SpecialType.System_Array] = ""
    };
}