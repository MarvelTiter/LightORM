using LightOrmTableContextGenerator.Builder;
using LightOrmTableContextGenerator.Extensions;
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
            if (ctxSymbol.HasInterface(ContextInterfaceFullName) == false)
            {
                context.ReportDiagnostic(DiagnosticDefinitions.TCG00001(source.TargetNode.GetLocation()));
                return;
            }

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


            var file = CodeFile.New($"{ctxSymbol.FormatFileName()}.LightORM.g.cs");
            context.AddSource(file);
        });

    }

    private static CodeFile? GenerateTypeContextClass(INamedTypeSymbol target, int index)
    {

        _ = target.GetAttribute(LightTableAttributeFullName, out var lightTable);
        _ = target.GetAttribute("System.ComponentModel.DataAnnotations.Schema.TableAttribute", out var componentTable);
        _ = target.GetAttribute("System.ComponentModel.DescriptionAttribute", out var des);
        List<Node> members = [
            // private LightORM.Models.ColumnInfo[] columns;
            FieldBuilder.Default.Modifiers("private").MemberType("global::LightORM.Models.ColumnInfo[]").FieldName("columns"),
            // public Type Type { get; } = typeof(Product);
            PropertyBuilder.Default.MemberType("Type").PropertyName("Type").Readonly().InitializeWith($"typeof({target.ToDisplayString()})"),
            // public string TableName => CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常");
            PropertyBuilder.Default.MemberType("string").PropertyName("TableName").Lambda("""CustomName ?? Type?.Name ?? throw new LightOrmException("获取表名异常")"""),
            // public string? Alias { get; set; } = "a1";
            PropertyBuilder.Default.MemberType("string?").PropertyName("Alias").InitializeWith($"\"a{index}\""),
            // public bool IsAnonymousType => false;
            PropertyBuilder.Default.MemberType("bool").PropertyName("IsAnonymousType").InitializeWith("false")
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
        members.Add(PropertyBuilder.Default.MemberType("global::LightORM.Models.ColumnInfo[]").PropertyName("Columns").Lambda("CollectColumnInfo()"));
        // GetValue   object? GetValue(ColumnInfo col, object target);
        members.Add(MethodBuilder.Default.MethodName("GetValue").ReturnType("object?").AddParameter("global::LightORM.Models.ColumnInfo col", "object target").Lambda("throw new Exception()"));
        // SetValue   void SetValue(ColumnInfo col, object target, object? value)
        members.Add(MethodBuilder.Default.MethodName("SetValue").AddParameter("global::LightORM.Models.ColumnInfo col", "object target", "object? value").Lambda("throw new Exception()"));

        var initColumnMethod = MethodBuilder.Default.MethodName("CollectColumnInfo").Modifiers("private").ReturnType("global::LightORM.Models.ColumnInfo[]").AddBody("return []");

        members.Add(initColumnMethod);

        var r = ClassBuilder.Default.MakeRecord().ClassName($"{target.FormatClassName()}Context")
            .Interface("global::LightORM.Interfaces.ITableEntityInfo")
            .AddMembers([.. members]);



        return CodeFile.New($"{target.FormatFileName()}Context.g.cs")
            .AddMembers(NamespaceBuilder.Default.Namespace("LightORM.GeneratedTableContext").AddMembers(r));
    }
}
