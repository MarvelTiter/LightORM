# LightORM 项目长期备忘

> 细节推导与实验原始记录见 `.workbuddy/memory/YYYY-MM-DD.md`；本文件只留结论与铁律。

## 构建 / 命令环境
- bash 沙箱：**每条命令前加** `export PATH="/usr/bin:/bin:$PATH"`（shell 状态不跨调用保留）。
- dotnet 缺环境变量 → restore 报 `Value cannot be null (path1)`。补：
  `env "ProgramFiles=C:\Program Files" "ProgramFiles(x86)=C:\Program Files (x86)" APPDATA="C:\Users\<user>\AppData\Roaming" PROGRAMDATA="C:\ProgramData" dotnet <cmd>`
- dotnet 输出常被沙箱吞掉 → **重定向到文件**：`dotnet test ... > out.txt 2>&1; echo "EXIT=$?"`，再 `grep -a`（中文日志必须 `-a`）。
- `--no-build` 前必须**刚 build 过目标项目自己**（各 provider bin 下 `LightORMTest.dll` 是独立副本）。
- NuGet：本地源 `E:\GitRepositories\LocalNuget`；全局包目录 `E:\Nuget\.nuget\packages`；反编译 `ilspycmd`。
- **不要用 `git stash`**（留残缺 `refs/stash`，并让 status/log/diff 因 submodule `src/GenShared` 报错）；清理用 `git update-ref -d refs/stash`。submodule 完整副本 `E:\GitRepositories\MT.Generators.Shared`；ssh 被沙箱拒，重克隆需用户本地执行。
- 判断"文件/对象是否真缺失"要用**第二个独立工具**（PowerShell `Test-Path`）交叉验证。**同一仓库同时开 SourceTree / UGit / VS 的 Git 工具是高危**（回收站可恢复 git 对象：`$R*` → zlib 解压 → `blob `/`tree `/`commit ` 开头 → 求 SHA-1 → 缺失则复制）。
- `ArgumentNullException.ThrowIfNull` 在 net462/netstandard2.0 靠 `src/LightORM/ExceptionPolyfills.cs`（`internal`，在 `namespace System` 用 C# 14 extension block）+ 各方言/DI 项目的 `InternalsVisibleTo` 解析 —— **跨程序集能用是靠 IVT**，不是 BCL。

## 项目约定
- 多目标 **net462;net8.0;net10.0**（`src/SupportedFrameworks.props`；netstandard2.0 已从核心移除）。版本号集中在 `src/versions.props`。
- 源生成器：`LightOrmTableContextGenerator` + `LightOrmExtensionGenerator`；用生成器的项目必须 `<ImplicitUsings>enable</ImplicitUsings>`。
- `AddLightOrm` 在 namespace `LightORM`（`IoCExtensions.cs`）。
- 驱动：MySqlConnector 2.6.0、Microsoft.Data.SqlClient 7.0.1（net462 走 System.Data.SqlClient）。
- 测试：`dotnet test test/LightORMTest.Sqlite/... --filter "FullyQualifiedName~Json" -l "console;verbosity=detailed"`。SqlGenerate 目录用例只 `Console.WriteLine` 不连库（离线可跑）。
- DatabaseUtils(WPF) → DatabaseUtils.Avalonia 迁移：阶段一/二已完成，剩 ViewModel / View / 回归。

## 版本探测与方言能力位
| 方言 | 能力位 | 影响 |
|---|---|---|
| SqlServer | `OffsetFetch`(2012+) / `TrimFunction`(2017+) | `OFFSET/FETCH` vs `ROW_NUMBER`；`TRIM()` vs `LTRIM(RTRIM())` |
| MySQL | `GroupByRollup`(8.0+) | `ROLLUP()/CUBE` vs 旧式 `WITH ROLLUP` |
| Oracle | `UseIdentityColumn` / `JsonNativeType`(21c+) | IDENTITY vs 序列+触发器；`JSON` 列 vs `CLOB`；`JSON(:p)` vs `:p FORMAT JSON` |
| SQLite | `Jsonb`(3.45+) | `jsonb_*` vs `json_*` |
| PG / KingbaseES / Dameng | **不接入探测** | 没有按版本二选一的分支，接入只是白搭一次连库 |

- **未知版本基线是方言私有的、可以相反**：Oracle/MySQL 取保守（未知→`None`），因为新写法在旧库直接失败；SqlServer/**SQLite** 取乐观（未知→`Full`），因为旧写法在新库仍可跑，且能保证 `DetectVersion=false` 的 ToSql 场景**生成与引入探测前一致的 SQL**。
- **能力位只登记"有多套写法"的能力**。单一实现（`JSON_VALUE`、`STRING_AGG`）不判版本，低版本交给数据库报错。
- 探测三条关闭出口：`TableOptions.SpecificVersion` 有值（`Source=Explicit`，实测 0ms 不建连）／`DetectVersion=false`／连接串为空。
- 版本在**provider 构造期烘焙**（先 `Capabilities`，再 handler / adapter / MethodResolver）。`Probe` 是 `protected`；`OpenBeforeRead` 是 `protected virtual`，**SQLite 覆盖为 false**（Open 会建出空库文件，纯 SQL 生成场景不能碰）。
- 基类只有一层 `DbCapabilities<TDatabase,TFeatures>`（`Implements/DbCapabilities.cs`）。**不要为"无能力位的方言"拆 1 参基类**（零实现的死抽象）。net462 不支持 `static abstract` ⇒ 静态工厂用 `new TDatabase()` 借空实例当元数据载体。`DbVersionProbe.None` 必须每次 `new` 而非共享单例（`Override` 改实例字段会污染 `Baseline`）。
- 连接超时靠**方言改写连接串**（`Func<string,int,string?>`，核心只给策略，不枚举键名）。踩过的坑：ODP.NET 会把键名大写归一化；SqlServer/MySql/PG/KES 都接受 `Command Timeout` 且读回值相等 ⇒ 按键名试探无法区分语义。
- **SqlServer 分页硬约束**：`OFFSET ... FETCH NEXT` 前必须有 `ORDER BY`。`CustomSqlServerAdapter.Paging` 在 `OrderBy.Count == 0` 时按主键补。这条路径接入探测后才第一次被真正执行。
- **Oracle 身份列 DDL**：`GENERATED ALWAYS AS IDENTITY` 必须紧跟数据类型（写在 `NOT NULL` 后报 ORA-00907）；身份列不能显式写 `NULL`（ORA-30670）。
- **Oracle JSON 分界**：原生 `JSON` 列、`JSON()`、`JSON_TRANSFORM` 均 21c+（19c 建 JSON 列报 ORA-00902）；`JSON_VALUE/JSON_QUERY` 需 12.1.0.2+ 且对 CLOB 同样有效；`FORMAT JSON` 18c 才真正生效；`CAST(... AS JSON)` 在 21.3 XE 报 ORA-00600。非原生分支**不用 `JSON_MERGEPATCH`**（整文档合并语义，会静默写错）。`SqlFn.JsonSet` 不接能力位。
- **达梦 JSONB 实测（8.1.4.6）**：能建 `JSONB` 列，但 `JSONB_VALUE/JSONB_QUERY` 函数不存在 ⇒ `JSONBackend.Binary` 不可用，且**与版本无关**，不登记能力位，默认 `Text`。
- **`InternalCreateTableAsync` 吞异常并 return false**（`ExpressionCoreSqlContextMethodImpl.cs:211`）→ DDL 失败不抛，只在后续以 `ORA-00942 表或视图不存在` 暴露。

## 全局静态状态（已知缺陷源）
- `ExpressionSqlOptions` 的 `databaseProviders / customDatabases / databaseHandlers / defaultDbKey` **全是 static**，且 `SetDatabase` 用 `TryAdd` = **先注册者胜、后来者静默丢弃**。`customDatabases` 按 `DbBaseType.Name` 只留第一个 adapter，而能力位是烘焙在 adapter 实例里的 ⇒ **同一方言多实例（不同版本/不同 UseIdentifierQuote）会串味**。多方言 A/B 必须用具名 key + `SetDefault` 切换。
- `Resolve` 表达式缓存 key 只有 `(SqlAction, Hash, NestedLevel)`，**无方言维度** ⇒ 同进程跨方言串味（既存缺陷）。正确键还需标识符引用身份（`Emphasis` + 生效引用开关）；参数前缀不是维度（解析期不读 `database.Prefix`）。09-17 整体回滚，重拾前先查"多方言同进程 PG 输出整段是 Oracle 的"。
- `Builder/AdapterCapability.TryGet<T>` 可穿透 `ScopedDatabaseAdapter` 取能力接口 → 新增能力不必改 `IDatabaseAdapter`。

## 反向读表结构（七方言已全部实现）
- **达梦坑最多**（`Providers.Dameng/DamengTableHandler.cs`）：
  - 主表 `USER_TAB_COLUMNS` + `LEFT JOIN` `USER_COL_COMMENTS` / `USER_CONSTRAINTS`+`USER_CONS_COLUMNS` / `SYSCOLUMNS`（经 `SYSOBJECTS.NAME=tc.TABLE_NAME AND TYPE$='SCHOBJ' AND SUBTYPE$='UTAB'` 定位）。
  - **`USER_TAB_COLUMNS` 没有 `IDENTITY_COLUMN` 列** ⇒ 自增只能判 `SYSCOLUMNS.INFO2 & 1 = 1`。
  - **`DATA_LENGTH` 对 `NVARCHAR2` 是字节数**（64 字符给 128）；**Length 一律取 `SYSCOLUMNS.LENGTH$`**。
  - **JSON/JSONB 物理是 `BLOB`**，靠 `SYSCOLUMNS.SCALE=16384` 区分（普通 BLOB 为 0）。
  - **`DATA_SCALE` 不是纯小数位**（NVARCHAR2=7、NCHAR=8、TIMESTAMP=6）⇒ 数值分支要加 `TYPE$ IN (...)` 双重限定。
  - 裸 `NUMBER` 给 **decimal**（达梦是浮点语义，给 int 遇小数会炸）。`FLOAT`=8 字节双精度、`REAL`=4 字节单精度。
  - **`BOOLEAN`/`UUID` 不能建列**；`bool` 写 `CHAR(1)`、`Guid` 写 `CHAR(36)`，读回只给 `string`（不做 writer 私有约定的逆向猜测，同 SQLite 口径）。
  - 支持 `DROP TABLE IF EXISTS`；表名精确匹配，不做 `UPPER()`。
- **SQLite**：列表排除 `sqlite_%`；结构必须用**表值函数 `pragma_table_info('t')`**（`PRAGMA table_info` 不能进 SELECT）。表名需 `Replace("'", "''")`。`ParseDataType` 未命中按**官方亲和性顺序**兜底（顺序不可改：`FLOATING POINT` 里 `POINT` 含 `INT`）。net462 无 `System.Range` ⇒ 用 `Substring`。
- 探针方法论：写临时 MSTest 直连，先逐类型 `CREATE TABLE ("C1" <type>)` 探支持度，再建全类型表打印**每列元数据**，最后把最终 SQL 原样预演。比改 C# 跑测试快一个数量级。
- 反向读表的回归测试（SchemaReadTest）**已被用户删除、未入库**，别当既成事实。

## JSON 列约定
- 序列化下沉到 `IDatabaseParameterBinder.BindParameter`；`DbParameterValue` 携带原始 CLR 值；`Utils/JsonParameterHelper.cs` 提供标量白名单与 `IsCompositeJsonLeaf`。
- **缓存约束**：SQL 侧形状分支只能由表达式结构决定，不能用运行时值。

| 方言 | 路径更新值 | 读末级标量/复合 |
|---|---|---|
| PG / KingbaseES | `@p::JSONB` | `(col->>'p')::T` / 数组 `col->N` |
| SQLite | `JSON(@p)` | `json_extract` |
| SqlServer | 复合 `JSON_QUERY(@p)`；标量传原生值 | `JSON_VALUE` / `JSON_QUERY` |
| MySQL | `CAST(?p AS JSON)` | `col->>'$.p'` |
| Dameng | `CAST(:p AS JSON)` | `JSON_VALUE` |
| Oracle | 21c+ `JSON(:p)`；≤19c `:p FORMAT JSON` | `JSON_VALUE` / `JSON_QUERY` |

- 整列更新一律 `col = @p`（MySQL `JSON_SET` 首参 NULL 会整体置 NULL；KingbaseES `JSONB_SET` 空路径不能替换整文档）。
- MySQL `ON DUPLICATE KEY UPDATE` 受影响行数 1/2 分叉，断言按 DbType 分支。`DbBaseType` 是 record（按 Name），Kingbase 的 Name 是 `"KingbaseES"`。

## 测试基础设施
- 容器：`dm8` 5236 / `kes` 54321 / `oracle21xe` 1521(`localhost:1521/XE`) / `mysql` 3306（**本机 5.7.44**：无 CTE/窗口函数，CUBE 抛 NotSupported）/ `pgsql` 5432 / `mssql` 1433。
- 用例数：Dameng 270 / Oracle 194 / Sqlite 168 / KingbaseES 100。
- **Sqlite 全量跑不完**：单用例 20s 级底噪，全量 >15min 会被工具超时打断，用 `--filter` 收窄。`Group_Cube_Test` 在 Sqlite 注定失败（本就不支持 CUBE）。
- `ExecutionTest.InitData` 的 `[TestInitialize]` 每用例重建整个 schema → 每个用例几秒底噪。
- 达梦慢的原因：DDL 单条 ~25ms 且自带提交，单次 init ≈4.8s（Kingbase 0.57s）；`DamengTableHandler.Writer` 每表 yield CREATE/COMMENT/INDEX 多次（10 表=131 条）。**批量化实测不可行**（`;` 多语句被拒，匿名块慢 7 倍）。
- Dameng/PostgreSQL/SqlServer 项目里 `ResultTest/ExecutionTest.cs` 与 `SelectResult.cs` 都继承共享 `ExecutionTest`(100 用例) → 同一套件跑两遍。

## JSON 配置驱动注册数据库（2026-11-08 已落地，七方言全覆盖）
- 链路：`AddLightOrm(options, configuration)` → `option.UseXxx()` 空参重载 → `IDatabaseRegistry.RegisterDatabase` → `RegisterCreator(DbBaseType, Func<DatabaseOptions, IDatabaseProvider>)`；首次 resolve `IExpressionContext` 时 `DatabaseListReloader`（DI 单例）做初始加载并订阅 `IOptionsMonitor.OnChange`。
- **新方言接入只需两处**（核心/DI 不动）：① `XxxProvider.Create(DatabaseOptions)`；② `public void UseXxx() => options.RegisterDatabase(XxxProvider.Xxx, d => XxxProvider.Create(d));`。已接：Dameng / PostgreSQL / SqlServer / MySql / Sqlite / Oracle / KingbaseES。
- 核心 helper（`LightORM/Options/DatabaseOptionsExtensions.cs`，public，第三方 provider 可用）：
  - `database.ToDbOption<T>(connectionString)`：统一搬 `SlaveConnectionStrings` / `IsUseIdentifierQuote` / 版本字段，方言只管自己私有的（如 `TableSpace` 走 `Settings`）。
  - `definition.HasSplitConnectionFields()`：**字段清单只在这里维护**，加字段只改一处。
  - `DatabaseConnectionString.Build<T>(key, definition, factory, applyFields)`：**泛型**（`where T : DbConnectionStringBuilder`），`hasSplitFields` 由内部算、不用传；无分散字段时原样返回连接串（不归一化）；拼完为空抛可读异常。
- **字段范围的口径（用户明确定过）**：只收"拼出可用连接串"必需的字段 —— `ConnectionString` / `Server` / `Port` / `UserId` / `Password` / `Database` / `DataSource` / `ServiceName` / `Sid`。**连接超时、命令超时、连接池、应用名这类调优项不进模型**，需要就写进 `ConnectionString`。
- 各方言构建连接串的坑：
  - **SqlServer**：`SqlConnectionStringBuilder` 没有 Port → 写成 `Data Source=host,port`；Server 含 `,`/`\` 时再给 Port 直接报错；`Database` → `InitialCatalog`。
  - **PG / KingbaseES**：键是 `Host`/`Username`/`Port`/`Database`（不是 Server/UserId）。
  - **MySql**：`Server`/`Port`(uint)/`UserID`/`Database`；构造期校验库名前，原来那条正则只认字母下划线（库名带数字会误判"未发现数据库"），已换成 `MySqlConnectionStringBuilder(cs).Database`。
  - **Sqlite**：只有 `DataSource`；`DataSource` 优先，其次把 `Database` 当库文件路径，都没有且无连接串时抛错并说明 Server/Port/凭据无意义。
  - **Oracle**：只有 `DataSource`，组装优先级 = 显式 `DataSource`（TNS 别名/描述符/EZ Connect）→ `ServiceName`（`host:port/service`，默认端口 1521）→ `Sid`（完整 `(DESCRIPTION=(ADDRESS=...)(CONNECT_DATA=(SID=...)))`）→ 裸 `Server`（当 TNS 别名，`Port` 有值才拼 `host:port`）。ServiceName 与 Sid 同时给、或给了 ServiceName/Sid 却没 Server → 抛可读异常。
- `IsUseIdentifierQuote` 默认值不定死：PG 构造期 `?? false`，SqlServer/Dameng 等 `?? true` —— 配置不填就沿用方言默认。
- `DetectVersion=false` 在配置里可表达（ToSql 场景不建连）；SqlServer 不探测时 `Capabilities.IsResolved == true`（落 Baseline）。
- `IExpressionContextSetup` **不再加成员**：新增可选接口 `IDatabaseRegistry`，调用方走扩展方法 `setup.RegisterDatabase(...)`（判 `is not IDatabaseRegistry` 抛可读异常），第三方实现不会被破坏。
- `databaseCreators` / `customDatabases` / `databaseHandlers` 均 `OrdinalIgnoreCase`；配置里的 `Type` 与 `DbBaseType.Name` 大小写不敏感匹配，注册时用 `provider.DbBaseType`。
- 覆盖/移除语义：配置项 `ApplyDatabase(replace:true)` 覆盖同 key 的 provider（adapter/handler 跟着换）；配置里删掉的 key 走 `RemoveDatabase`；代码里 `SetDatabase` 注册、配置里没有的库不动。
- `BindingExtensions.cs` 是**手写**的 AOT 绑定器（`EnableConfigurationBindingGenerator` 已删）→ 给 `DatabaseOptions` / `DatabaseDefinition` 加属性必须同步改它。内部用 `BindString/BindInt/BindBool` 三个小 helper，每个字段一行。
- 测试：`test/LightORMTest/DatabaseConfigurationTest.cs`（5 用例，全过）。**`TestBase` 刻意不接配置**（否则会给所有 provider 用例叠加额外库、改变 `DatabaseProviders.First()` 的断言），原来的 `db.json` 已删除。构造真实连接串的验证一直是临时用例（各 provider 项目里写、跑完删），**没有常驻回归**。
- 既有失败（与本线无关，别当回归）：`SwitchDatabaseTest.TestConfig`（`UseSqlite("db1","")` 空串本来就抛）、`ConstantExpressionTest` 两个用例（不走 AddLightOrm，纯 `ExpressionResolver` 静态缓存串味）、`BenchmarkTest` 编译失败（固定旧 NuGet 包，`Ado` 不存在）。

