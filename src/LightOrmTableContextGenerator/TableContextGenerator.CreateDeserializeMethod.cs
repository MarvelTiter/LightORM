using Generators.Shared;
using Generators.Shared.Builder;
using Microsoft.CodeAnalysis;

namespace LightOrmTableContextGenerator;

public partial class TableContextGenerator
{
    private static MethodBuilder CreateDeserializeMethod(INamedTypeSymbol target, PropertyScanResult[] columns)
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
}