using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using static LightOrmTableContextGenerator.TableContextGenerator;
namespace LightOrmTableContextGenerator
{
    internal static class TableContextGeneratorHelpers
    {

        internal static CodeFile? CreateAggregationContextClass(INamedTypeSymbol target, INamedTypeSymbol[] items)
        {
            List<Node> members = [
    FieldBuilder.Default
            .MemberType("global::System.Collections.Generic.Dictionary<global::System.Type, global::LightORM.Interfaces.ITableEntityInfo>")
            .FieldName("tableInfos"),
           FieldBuilder.Default
            .MemberType("global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Action<global::LightORM.Interfaces.ITableColumnInfo, object, object?>>")
            .FieldName("table_sets"),
            FieldBuilder.Default
            .MemberType("global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Func<global::LightORM.Interfaces.ITableColumnInfo, object, object?>>")
            .FieldName("table_gets"),
];
            List<Statement> dicInits = [
                "tableInfos = new global::System.Collections.Generic.Dictionary<global::System.Type, global::LightORM.Interfaces.ITableEntityInfo>()",
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
                members.Add(PropertyBuilder.Default
                    .Modifiers("public static")
                    .PropertyName(item.FormatClassName(true))
                    .MemberType($"{item.FormatClassName(true)}TableInfo")
                    .Readonly()
                    .InitializeWith($"new {item.FormatClassName(true)}TableInfo()")
                    );
                //members.Add(MethodBuilder.Default
                //    .Modifiers("public static")
                //    .ReturnType("global::LightORM.Interfaces.ITableEntityInfo")
                //    .MethodName(item.FormatClassName(true))
                //    .Lambda($"new {item.FormatClassName(true)}TableInfo()"));
            }
            #region GetTableInfoMethod
            {
                List<Statement> methodStatements = [];
                //foreach (var item in items)
                //{
                //    var ifs = IfStatement.Default.If($"type == typeof({item.ToDisplayString()}) || type.IsAssignableFrom(typeof({item.ToDisplayString()}))").AddStatement($"return {item.FormatClassName(true)}");
                //    getTableInfoMethodStatements.Add(ifs);
                //}
                var dicCheck = IfStatement.Default.If("tableInfos.TryGetValue(type, out var ti)").AddStatement("return ti");

                var notFound = IfStatement.Default.If("type.IsAbstract || type.IsInterface")
                    .AddStatement(ForeachStatement.Default.Foreach("var kvp in tableInfos").AddStatements(
                    IfStatement.Default.If("kvp.Key.IsAssignableFrom(type)").AddStatement("return kvp.Value")));
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
    }
}