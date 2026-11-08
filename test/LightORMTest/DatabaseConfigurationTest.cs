using LightORM;
using LightORM.Interfaces;
using LightORM.Providers.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightORMTest;

/// <summary>
/// 配置驱动的数据库注册（<c>AddLightOrm(..., IConfiguration)</c>）。
/// <para>
/// 用 Sqlite 当被测方言：provider 创建不连库，整个用例离线可跑。
/// 全局注册表是 static 的，所以每个用例用唯一 key，并在 Cleanup 里移除。
/// </para>
/// </summary>
[TestClass]
[DoNotParallelize]
public class DatabaseConfigurationTest
{
    private const string SectionName = "LightOrmDatabases";

    private string directory = null!;
    private string configFile = null!;
    private readonly List<string> registeredKeys = [];

    [TestInitialize]
    public void Initialize()
    {
        directory = Path.Combine(Path.GetTempPath(), "lightorm-config-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(directory);
        configFile = Path.Combine(directory, "db.json");
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (var key in registeredKeys)
        {
            _ = ExpressionSqlOptions.RemoveDatabase(key);
        }

        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch
        {
            // 文件监听可能还握着句柄，删不掉不影响用例结论
        }
    }

    [TestMethod]
    public void InitialLoad_RegistersDatabasesFromConfiguration()
    {
        var key = NewKey("init");
        using var provider = CreateProvider(Config(key, "Data Source=:memory:"));

        var db = provider.GetRequiredService<IExpressionContext>();

        Assert.IsTrue(db.Options.DatabaseProviders.ContainsKey(key), "初始加载没有发生：配置里的数据库没有被注册");
        Assert.AreEqual("Data Source=:memory:", db.Options.DatabaseProviders[key].MasterConnectionString);
    }

    [TestMethod]
    public void Reload_ReplacesConnectionStringOfExistingKey()
    {
        var key = NewKey("reload");
        using var provider = CreateProvider(Config(key, "Data Source=a.db"));
        var db = provider.GetRequiredService<IExpressionContext>();
        Assert.AreEqual("Data Source=a.db", db.Options.DatabaseProviders[key].MasterConnectionString);

        File.WriteAllText(configFile, Config(key, "Data Source=b.db"));

        Assert.IsTrue(
            WaitFor(() => db.Options.DatabaseProviders[key].MasterConnectionString == "Data Source=b.db"),
            $"连接串没有随配置更新，当前值：{db.Options.DatabaseProviders[key].MasterConnectionString}");
    }

    [TestMethod]
    public void Reload_RemovesDatabaseDeletedFromConfiguration()
    {
        var kept = NewKey("keep");
        var removed = NewKey("gone");
        using var provider = CreateProvider(ConfigWithTwoEntries(kept, "Data Source=:memory:", removed));
        var db = provider.GetRequiredService<IExpressionContext>();
        Assert.IsTrue(db.Options.DatabaseProviders.ContainsKey(removed));

        File.WriteAllText(configFile, Config(kept, "Data Source=:memory:"));

        Assert.IsTrue(
            WaitFor(() => !db.Options.DatabaseProviders.ContainsKey(removed)),
            "配置里已删除的数据库仍然留在注册表里");
        Assert.IsTrue(db.Options.DatabaseProviders.ContainsKey(kept), "保留下来的数据库不应被一起删除");
    }

    [TestMethod]
    public void UnknownType_ThrowsWithReadableMessage()
    {
        var key = NewKey("unknown");
        using var provider = CreateProvider(Config(key, "Data Source=:memory:", type: "NoSuchDatabase"));

        LightOrmException? error = null;
        try
        {
            _ = provider.GetRequiredService<IExpressionContext>();
        }
        catch (LightOrmException ex)
        {
            error = ex;
        }

        Assert.IsNotNull(error, "未知的 Type 应该抛 LightOrmException");
        StringAssert.Contains(error.Message, "NoSuchDatabase");
        StringAssert.Contains(error.Message, "已注册");
    }

    [TestMethod]
    public void TypeMatching_IsCaseInsensitive()
    {
        var key = NewKey("case");
        using var provider = CreateProvider(Config(key, "Data Source=:memory:", type: "sqlite"));

        var db = provider.GetRequiredService<IExpressionContext>();

        Assert.IsTrue(db.Options.DatabaseProviders.ContainsKey(key), "Type 匹配应该忽略大小写");
    }

    private string NewKey(string prefix)
    {
        var key = prefix + "-" + Guid.NewGuid().ToString("N")[..8];
        registeredKeys.Add(key);
        return key;
    }

    private static string Config(string key, string connectionString, string type = "Sqlite")
        => $$"""
        {
          "{{SectionName}}": [
            { "Key": "{{key}}", "Option": { "Type": "{{type}}", "ConnectionString": "{{connectionString}}" } }
          ]
        }
        """;

    private static string ConfigWithTwoEntries(string firstKey, string firstConnectionString, string secondKey)
        => $$"""
        {
          "{{SectionName}}": [
            { "Key": "{{firstKey}}", "Option": { "Type": "Sqlite", "ConnectionString": "{{firstConnectionString}}" } },
            { "Key": "{{secondKey}}", "Option": { "Type": "Sqlite", "ConnectionString": "Data Source=:memory:" } }
          ]
        }
        """;

    private ServiceProvider CreateProvider(string json)
    {
        File.WriteAllText(configFile, json);

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configFile, optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddLightOrm(options =>
        {
            options.SetTableContext<TestTableContext>();
            options.ConfigJsonHandler<JsonHandler>();
            options.RegisterDatabase(DbBaseType.Sqlite, database =>
                SqliteProvider.Create(setting => setting.MasterConnectionString = database.Option!.ConnectionString));
        }, configuration);

        return services.BuildServiceProvider();
    }

    private static bool WaitFor(Func<bool> condition, int timeoutMilliseconds = 8000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return true;
            }

            Thread.Sleep(100);
        }

        return condition();
    }
}
