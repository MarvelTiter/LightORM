using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatabaseUtils.Models;

[JsonSerializable(typeof(Dictionary<string, string?>))]
public partial class JsonContext : JsonSerializerContext
{
    public static readonly JsonSerializerOptions JsonOption = new()
    {
        WriteIndented = true,
    };
}
/// <summary>
/// 应用配置，替代 WPF 的 Properties/Local.settings，
/// 使用 JSON 文件持久化到 ApplicationData 目录。
/// </summary>
public class Config
{
    private static readonly string SettingsDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DatabaseUtils.Avalonia");

    private static readonly string SettingsFile = Path.Combine(SettingsDirectory, "settings.json");

    private static readonly Dictionary<string, string?> Store = Load();

    private static Dictionary<string, string?> Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<Dictionary<string, string?>>(json, JsonContext.Default.DictionaryStringString)
                       ?? [];
            }
        }
        catch
        {
            // 配置文件损坏时静默忽略，返回空配置
        }

        return [];
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            File.WriteAllText(SettingsFile, JsonSerializer.Serialize(Store, JsonContext.Default.DictionaryStringString));
        }
        catch (IOException)
        {
            // 持久化失败不影响主流程
        }
    }

    private static string? Get([CallerMemberName] string name = "")
    {
        Store.TryGetValue(name, out var value);
        return value;
    }

    private static void Set(string? value, [CallerMemberName] string name = "")
    {
        Store[name] = value;
        Save();
    }

    public string? LastSelectedDb
    {
        get => Get();
        set => Set(value);
    }

    public string? Connectstring
    {
        get => Get();
        set => Set(value);
    }

    public string? Prefix
    {
        get => Get();
        set => Set(value);
    }

    public string? Separator
    {
        get => Get() ?? "_";
        set => Set(value);
    }

    public string? Namespace
    {
        get => Get();
        set => Set(value);
    }

    public string SavedPath
    {
        get => Get() ?? "";
        set => Set(value);
    }
}
