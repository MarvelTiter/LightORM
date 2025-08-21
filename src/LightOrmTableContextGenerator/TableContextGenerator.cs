using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

            var file = TableContextGeneratorHelpers.CreateAggregationContextClass(ctxSymbol, allTableType);
            if (file != null)
                context.AddSource(file);
        });

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

        var columns = target.GetAllMembers(s => s.IsAbstract)
            .Where(i => i.Kind == SymbolKind.Property && i is IPropertySymbol p && p.DeclaredAccessibility == Accessibility.Public)
            .Cast<IPropertySymbol>().ToArray();

        // Func<IDataReader, object>? DataReaderDeserializer { get; }
        MethodBuilder staticDeserializerMethod = CreateDeserializeMethod(target, columns);
        var deserializer = PropertyBuilder.Default
            .PropertyName("DataReaderDeserializer")
            .MemberType($"global::System.Func<global::System.Data.IDataReader, {target.ToDisplayString()}>?")
            .Lambda(staticDeserializerMethod.Name!);

        members.Add(CreateInitColumnInfoMethod(target, columns));
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
            .AddMembers(NamespaceBuilder.Default.Namespace("LightORM.GeneratedTableContext").AddMembers(r));
    }

    private static MethodBuilder CreateDeserializeMethod(INamedTypeSymbol target, IPropertySymbol[] columns)
    {
        //var initInstance = $"var p = {target.New()}";
        //var forStatement = ForStatement.Default.For("int i = 0; i < reader.FieldCount; i++");
        //var dbnullCheck = IfStatement.Default.If("reader.IsDBNull(i)")
        //    .AddStatement("continue");
        //forStatement.AddStatements(dbnullCheck);
        //forStatement.AddStatements("string columnName = reader.GetName(i)");
        //var switchSet = SwitchStatement.Default
        //    .Switch("columnName");

        //foreach (var column in columns)
        //{
        //    if (column.IsReadOnly || column.SetMethod?.IsInitOnly == true)
        //        continue;
        //    if (column.Type.TypeKind == TypeKind.Class
        //        && column.Type.SpecialType == SpecialType.None
        //        && column.HasAttribute(LightFlatAttributeFullName))
        //    {
        //        var flatProps = column.Type.GetMembers().Where(i => i.Kind == SymbolKind.Property && i is IPropertySymbol p && p.DeclaredAccessibility == Accessibility.Public).Cast<IPropertySymbol>();
        //        foreach (var flat in flatProps)
        //        {
        //            switchSet.AddBreakCase($"\"{flat.Name}\"",
        //                //$"p.{column.Name}.{flat.Name}",
        //                IfStatement.Default.If($"p.{column.Name} is null")
        //                .AddStatement($"p.{column.Name} = {column.Type.New()};"),
        //                $"p.{column.Name}.{flat.Name} = {GetValueExpression("reader", flat, "i")}"
        //                );
        //        }
        //    }
        //    else
        //    {
        //        switchSet.AddBreakCase($"\"{column.Name}\"", $"p.{column.Name} = {GetValueExpression("reader", column, "i")}");
        //    }
        //}
        //switchSet.AddDefaultCase("throw new global::LightORM.LightOrmException()");
        //forStatement.AddStatements(switchSet);
        return MethodBuilder.Default
            .MethodName($"Deserialize{target.MetadataName}FromDbDataReader")
            .Modifiers("public static")
            .AddParameter("global::System.Data.IDataReader reader")
            .ReturnType(target.ToDisplayString())
            .AddBody("throw new NotImplementedException()");
        
        //static string GetValueExpression(string instanceName, IPropertySymbol property, string indexVar)
        //{
        //    if (property.Type.TypeKind == TypeKind.Array
        //        && property.Type is IArrayTypeSymbol array
        //        && array.ElementType.SpecialType == SpecialType.System_Byte)
        //    {
        //        // 处理 byte[] 类型
        //        return $"global::LightORM.Utils.DataRecordFieldHandleHelper.RecordFieldToBytes({instanceName}, {indexVar})";
        //    }
        //    else if (IsUnsignType(property.Type))
        //    {
        //        if (typeMapMethod.TryGetValue(property.Type.SpecialType, out var method))
        //        {
        //            return $"{method}({instanceName}, {indexVar})";
        //        }
        //    }
        //    else
        //    {
        //        if (typeMapMethod.TryGetValue(property.Type.SpecialType, out var method))
        //        {
        //            return $"{instanceName}.{method}({indexVar})";
        //        }
        //    }
        //    return "";
        //}
        //static bool IsUnsignType(ITypeSymbol type)
        //{
        //    return type.SpecialType switch
        //    {
        //        SpecialType.System_SByte => true,
        //        SpecialType.System_UInt16 => true,
        //        SpecialType.System_UInt32 => true,
        //        SpecialType.System_UInt64 => true,
        //        _ => false
        //    };
        //}
    }

    private static MethodBuilder CreateInitColumnInfoMethod(INamedTypeSymbol owner, IPropertySymbol[] columns)
    {
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
                var flatType = p.Type.WithNullableAnnotation(NullableAnnotation.NotAnnotated).ToDisplayString();
                foreach (var item in flattedProps)
                {
                    var fr = ScanProperty(item);
                    bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType}, "{item.Name}", {fr.CustomName}, {fr.PrimaryKey}, {fr.IsNotMap}, {fr.AutoIncrement}, {fr.NotNull}, {fr.Len}, {fr.DefaultValue}, {fr.Comment}, {fr.CanRead}, {fr.CanWrite}, {fr.CanInit}, {fr.NavInfo}, typeof({flatType}), false, true, false, {fr.IgnoreUpdate})""");
                }
                //bodies.Add($"""var gen_{p.Name} = new global::LightORM.Models.ColumnInfo({tableType}, "{p.Name}", null, false, true, false, false, 0, null, null, true, true, true, null)""");
                //bodies.Add($"gen_{p.Name}.IsAggregated = true");
                bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType}, "{p.Name}", null, false, true, false, false, 0, null, null, true, true, true, null, typeof({flatType}), true, false, false, true)""");
                continue;
            }
            var r = ScanProperty(p);
            bodies.Add($"""cols[{i++}] = new global::LightORM.Models.ColumnInfo({tableType}, "{p.Name}", {r.CustomName}, {r.PrimaryKey}, {r.IsNotMap}, {r.AutoIncrement}, {r.NotNull}, {r.Len}, {r.DefaultValue}, {r.Comment}, {r.CanRead}, {r.CanWrite}, {r.CanInit}, {r.NavInfo}, null, false, false, {r.IsVersion}, {r.IgnoreUpdate})""");
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
            var commentId = p.GetDocumentationCommentId();
            var commentDoc = p.GetDocumentationCommentXml();
            System.Diagnostics.Debug.WriteLine($"{commentId} -> 注释: {commentDoc}");
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
            var version = GetBoolValue(lightCol, "Version");
            var ignoreUpdate = GetBoolValue(lightCol, "IgnoreUpdate");
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
                IgnoreUpdate = ignoreUpdate
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

    readonly static Dictionary<SpecialType, string> typeMapMethod = new(37)
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
