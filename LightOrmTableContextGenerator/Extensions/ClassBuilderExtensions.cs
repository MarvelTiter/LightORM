using LightOrmTableContextGenerator.Builder;

namespace LightOrmTableContextGenerator.Extensions;

internal static class ClassBuilderExtensions
{
    public static ClassBuilder ClassName(this ClassBuilder builder, string className)
    {
        builder.Name = className;
        return builder;
    }

    public static ClassBuilder MakeRecord(this ClassBuilder builder)
    {
        builder.ClassType = "record";
        return builder;
    }

    public static ClassBuilder BaseType(this ClassBuilder builder, string baseType)
    {
        builder.BaseType = baseType;
        return builder;
    }

    public static ClassBuilder Interface(this ClassBuilder builder, params string[] interfaces)
    {
        foreach (var i in interfaces)
        {
            builder.Interfaces.Add(i);
        }
        return builder;
    }
}

internal static class PropertyBuilderExtensions
{
    public static PropertyBuilder PropertyName(this PropertyBuilder builder, string name)
    {
        builder.Name = name;
        return builder;
    }
    public static PropertyBuilder Readonly(this PropertyBuilder builder)
    {
        builder.CanRead = true;
        builder.CanWrite = false;
        return builder;
    }

    public static PropertyBuilder Writeonly(this PropertyBuilder builder)
    {
        builder.CanRead = false;
        builder.CanWrite = true;
        return builder;
    }    

    public static PropertyBuilder Lambda(this PropertyBuilder builder, string body)
    {
        builder.IsLambdaBody = true;
        builder.Readonly();
        builder.InitializeWith(body);
        return builder;
    }
}
