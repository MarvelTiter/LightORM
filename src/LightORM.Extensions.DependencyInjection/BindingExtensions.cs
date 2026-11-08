using LightORM.Options;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace LightORM;

/// <summary>
/// 手写的配置绑定器（等价于 EnableConfigurationBindingGenerator 的产物，但收窄到本库需要的类型）。
/// <para>
/// 为什么不用反射版 Binder：<c>services.Configure&lt;T&gt;(IConfiguration)</c> 走的是反射绑定，AOT/裁剪下会失效。
/// 这个绑定器只认下列类型 —— <b>给 <see cref="DatabaseOptions"/> / <see cref="DatabaseDefinition"/> 加属性时，
/// 必须同步改这里</b>，否则新属性会被静默忽略。
/// </para>
/// </summary>
internal static class BindingExtensions
{
    #region 入口
    /// <summary>把配置绑定到 <see cref="DatabaseList"/>（节名 <c>LightOrmDatabases</c>）。</summary>
    public static void Bind_DatabaseList(this IConfiguration configuration, DatabaseList instance)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(instance);

        BindCore(configuration, ref instance, defaultValueIfNotFound: false, binderOptions: null);
    }
    #endregion

    #region 核心绑定
    private readonly static Lazy<HashSet<string>> s_configKeys_DatabaseDefinition = new(() => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Type", "ConnectionString", "UserId", "Password", "Server", "Port", "Database",
        "DataSource", "ServiceName", "Sid",
        "SlaveConnectionStrings", "IsUseIdentifierQuote", "SpecificVersion", "DetectVersion", "DefaultStringLength", "Settings"
    });
    private readonly static Lazy<HashSet<string>> s_configKeys_DatabaseOptions = new(() => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Key", "Option" });
    private readonly static Lazy<HashSet<string>> s_configKeys_DatabaseList = new(() => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "LightOrmDatabases" });

    public static void BindCore(IConfiguration configuration, ref DatabaseDefinition instance, bool defaultValueIfNotFound, BinderOptions? binderOptions)
    {
        ValidateConfigurationKeys(typeof(DatabaseDefinition), s_configKeys_DatabaseDefinition, configuration, binderOptions);

        var target = instance;

        // 字符串
        BindString(configuration, "Type", value => target.Type = value);
        BindString(configuration, "ConnectionString", value => target.ConnectionString = value);
        BindString(configuration, "UserId", value => target.UserId = value);
        BindString(configuration, "Password", value => target.Password = value);
        BindString(configuration, "Server", value => target.Server = value);
        BindString(configuration, "Database", value => target.Database = value);
        BindString(configuration, "DataSource", value => target.DataSource = value);
        BindString(configuration, "ServiceName", value => target.ServiceName = value);
        BindString(configuration, "Sid", value => target.Sid = value);
        BindString(configuration, "SpecificVersion", value => target.SpecificVersion = value);

        // 整数
        BindInt(configuration, "Port", value => target.Port = value);
        BindInt(configuration, "DefaultStringLength", value => target.DefaultStringLength = value);

        // 布尔
        BindBool(configuration, "IsUseIdentifierQuote", value => target.IsUseIdentifierQuote = value);
        BindBool(configuration, "DetectVersion", value => target.DetectVersion = value);

        var slaveSection = configuration.GetSection("SlaveConnectionStrings");
        if (HasChildren(slaveSection))
        {
            var slaves = new List<string>();
            foreach (var item in slaveSection.GetChildren())
            {
                if (item.Value is { } value)
                {
                    slaves.Add(value);
                }
            }

            target.SlaveConnectionStrings = [.. slaves];
        }

        var settingsSection = configuration.GetSection("Settings");
        if (HasChildren(settingsSection))
        {
            var settings = target.Settings ?? [];
            foreach (var item in settingsSection.GetChildren())
            {
                // 只支持一层：值是字符串。嵌套结构请用强类型替代表达
                settings[item.Key] = item.Value ?? string.Empty;
            }

            target.Settings = settings;
        }

        instance = target;
    }

    public static void BindCore(IConfiguration configuration, ref DatabaseOptions instance, bool defaultValueIfNotFound, BinderOptions? binderOptions)
    {
        ValidateConfigurationKeys(typeof(DatabaseOptions), s_configKeys_DatabaseOptions, configuration, binderOptions);

        if (TryGetConfigurationValue(configuration, "Key", out var key))
        {
            instance.Key = key;
        }

        var optionSection = configuration.GetSection("Option");
        if (HasChildren(optionSection))
        {
            var definition = instance.Option ?? new DatabaseDefinition();
            BindCore(optionSection, ref definition, defaultValueIfNotFound: false, binderOptions);
            instance.Option = definition;
        }
    }

    public static void BindCore(IConfiguration configuration, ref List<DatabaseOptions> instance, bool defaultValueIfNotFound, BinderOptions? binderOptions)
    {
        foreach (var section in configuration.GetChildren())
        {
            // 跳过标量节点，避免把 key: value 形式的杂项当成数据库条目
            if (!string.IsNullOrEmpty(section.Value) && !section.GetChildren().Any())
            {
                continue;
            }

            var value = new DatabaseOptions();
            BindCore(section, ref value, defaultValueIfNotFound: false, binderOptions);
            instance.Add(value);
        }
    }

    public static void BindCore(IConfiguration configuration, ref DatabaseList instance, bool defaultValueIfNotFound, BinderOptions? binderOptions)
    {
        ValidateConfigurationKeys(typeof(DatabaseList), s_configKeys_DatabaseList, configuration, binderOptions);

        var section = configuration.GetSection("LightOrmDatabases");
        if (HasChildren(section))
        {
            // options 每次重建时 instance 都是新建的，这里直接绑到新列表，
            // 不要在已有列表上追加（否则会累积出重复条目）。
            var list = new List<DatabaseOptions>();
            BindCore(section, ref list, defaultValueIfNotFound: false, binderOptions);
            instance.LightOrmDatabases = list;
        }
    }
    #endregion

    #region 基础设施
    public static bool TryGetConfigurationValue(IConfiguration configuration, string key, out string? value)
    {
        if (configuration is ConfigurationSection section)
        {
            return section.TryGetValue(key, out value);
        }

        value = configuration[key];
        return value is not null;
    }

    private static bool HasChildren(IConfigurationSection section)
    {
        foreach (var _ in section.GetChildren())
        {
            return true;
        }

        return false;
    }

    private static void BindString(IConfiguration configuration, string key, Action<string?> set)
    {
        if (TryGetConfigurationValue(configuration, key, out var value))
        {
            set(value);
        }
    }

    private static void BindInt(IConfiguration configuration, string key, Action<int?> set)
    {
        if (!TryGetConfigurationValue(configuration, key, out var value))
        {
            return;
        }

        set(string.IsNullOrEmpty(value) ? null : ParseInt(value, configuration.GetSection(key).Path));
    }

    private static void BindBool(IConfiguration configuration, string key, Action<bool?> set)
    {
        if (!TryGetConfigurationValue(configuration, key, out var value))
        {
            return;
        }

        set(string.IsNullOrEmpty(value) ? null : ParseBool(value, configuration.GetSection(key).Path));
    }

    public static void ValidateConfigurationKeys(Type type, Lazy<HashSet<string>> keys, IConfiguration configuration, BinderOptions? binderOptions)
    {
        if (binderOptions?.ErrorOnUnknownConfiguration is not true)
        {
            return;
        }

        List<string>? unknown = null;
        foreach (var section in configuration.GetChildren())
        {
            if (!keys.Value.Contains(section.Key))
            {
                (unknown ??= []).Add($"'{section.Key}'");
            }
        }

        if (unknown is not null)
        {
            throw new InvalidOperationException(
                $"'ErrorOnUnknownConfiguration' was set on the provided BinderOptions, but the following properties were not found on the instance of {type}: {string.Join(", ", unknown)}");
        }
    }

    public static int ParseInt(string value, string? path)
    {
        try
        {
            return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to convert configuration value '{value}' at '{path}' to type '{typeof(int)}'.", exception);
        }
    }

    public static bool ParseBool(string value, string? path)
    {
        try
        {
            return bool.Parse(value);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to convert configuration value '{value}' at '{path}' to type '{typeof(bool)}'.", exception);
        }
    }
    #endregion
}
