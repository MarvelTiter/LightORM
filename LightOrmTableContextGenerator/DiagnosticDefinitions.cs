using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightOrmTableContextGenerator;

internal class DiagnosticDefinitions
{
    /// <summary>
    /// 需要实现 IAutoMap 接口
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static Diagnostic TCG00001(Location? location) => Diagnostic.Create(new DiagnosticDescriptor(
                    id: "TCG00001",
                    title: "需要实现 ITableContext 接口",
                    messageFormat: "需要实现 ITableContext 接口",
                    category: typeof(TableContextGenerator).FullName!,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true), location);
}
