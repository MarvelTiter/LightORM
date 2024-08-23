﻿using LightOrmTableContextGenerator.Builder;

namespace LightOrmTableContextGenerator.Extensions;

internal static class NameSpaceBuilderExtensions
{
    public static NamespaceBuilder Namespace(this NamespaceBuilder builder, string @namespace)
    {
        builder.Namespace = @namespace;
        return builder;
    }
}
