using LightORM.Models;
using System.Data.Common;

namespace LightORM.Options;

/// <summary>
/// 配置模型 <see cref="DatabaseOptions"/> → 代码模型 <c>DataBaseOption&lt;T&gt;</c> 的通用搬运。
/// <para>
/// 方言的 <c>Create(DatabaseOptions)</c> 只需要拼好连接串，剩下的通用字段（从库、标识符引用、
/// 版本相关）由这里统一处理，不要在各方言里各抄一遍。
/// </para>
/// </summary>
public static class DatabaseOptionsExtensions
{
    /// <summary>
    /// 把配置项转换成代码路径的 <see cref="DataBaseOption{T}"/>。
    /// </summary>
    /// <param name="database">配置项。</param>
    /// <param name="connectionString">已由方言拼装好的主库连接串。</param>
    public static DataBaseOption<T> ToDbOption<T>(this DatabaseOptions database, string connectionString)
        where T : TableOptions, new()
    {
        LightOrmException.ThrowIfNull(database, "数据库配置不能为 null");
        var definition = database.Option
            ?? throw new LightOrmException($"数据库 '{database.Key}' 缺少 Option 配置。");

        var dbOption = new DataBaseOption<T>
        {
            DbKey = database.Key,
            MasterConnectionString = connectionString,
            SalveConnectionStrings = definition.SlaveConnectionStrings,
        };

        if (definition.IsUseIdentifierQuote is { } useQuote)
        {
            _ = dbOption.UseIdentifierQuote(useQuote);
        }

        definition.ApplyTo(dbOption.GenerateOption);
        return dbOption;
    }

    /// <summary>
    /// 把与方言无关的通用选项（显式版本 / 是否探测 / 默认字符串长度）写到 <see cref="TableOptions"/>。
    /// </summary>
    public static void ApplyTo(this DatabaseDefinition definition, TableOptions options)
    {
        ArgumentNullException.ThrowIfNull(definition);
        LightOrmException.ThrowIfNull(options, "options 不能为 null");

        if (!string.IsNullOrWhiteSpace(definition.SpecificVersion))
        {
            if (!Version.TryParse(definition.SpecificVersion, out var version))
            {
                throw new LightOrmException($"SpecificVersion '{definition.SpecificVersion}' 不是合法版本号。");
            }

            options.SpecificVersion = version;
        }

        if (definition.DetectVersion is { } detectVersion)
        {
            options.DetectVersion = detectVersion;
        }

        if (definition.DefaultStringLength is { } stringLength)
        {
            options.DefaultStringLength = stringLength;
        }
    }

    /// <summary>
    /// 是否提供了"连接串之外"的分散字段。统一在这里判断，方言不用各写一遍
    /// （新增字段时只改这一处，避免某个方言悄悄漏掉）。
    /// </summary>
    public static bool HasSplitConnectionFields(this DatabaseDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return definition.Server is not null
            || definition.Port is not null
            || definition.UserId is not null
            || definition.Password is not null
            || definition.Database is not null
            || definition.DataSource is not null
            || definition.ServiceName is not null
            || definition.Sid is not null;
    }
}

/// <summary>
/// 连接串拼装的通用外壳：<c>ConnectionString</c> 为底 + 分散字段逐项覆盖。
/// <para>
/// 各方言的差异只有"字段 → builder 属性"的映射（键名 / 有的方言没有某些字段），
/// 由调用方通过 <c>applyFields</c> 表达；overlay 与空值校验在这里统一做。
/// </para>
/// </summary>
public static class DatabaseConnectionString
{
    /// <summary>
    /// 构建连接串。
    /// </summary>
    /// <param name="key">配置项名，仅用于异常信息。</param>
    /// <param name="definition">连接定义。</param>
    /// <param name="factory">方言的 builder 工厂；参数非 null 时用它作为底。</param>
    /// <param name="applyFields">把分散字段写到 builder 上；只覆盖有值的字段。</param>
    public static string Build<T>(
        string? key,
        DatabaseDefinition definition,
        Func<string?, T> factory,
        Action<T, DatabaseDefinition> applyFields)
        where T : DbConnectionStringBuilder
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(applyFields);

        var existing = string.IsNullOrWhiteSpace(definition.ConnectionString) ? null : definition.ConnectionString;

        // 没有分散字段就别让 builder 重新归一化连接串（键名大小写/顺序都会变）
        if (existing is not null && !definition.HasSplitConnectionFields())
        {
            return existing;
        }

        var builder = factory(existing);
        applyFields(builder, definition);

        if (builder.Count == 0)
        {
            throw new LightOrmException(
                $"数据库 '{key}' 既没有 ConnectionString，也没有可用于拼装的字段（Server / DataSource / ServiceName / UserId / Database ...）。");
        }

        return builder.ConnectionString;
    }
}
