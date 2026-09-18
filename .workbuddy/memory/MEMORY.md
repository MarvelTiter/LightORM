# LightORM 项目长期备忘

## 构建 / 命令环境（关键）
- bash 沙箱：**每条命令前加** `export PATH="/usr/bin:/bin:$PATH"`（初始 PATH 缺 /usr/bin，shell 状态不跨调用保留）。
- dotnet 缺环境变量 → restore 报 `Value cannot be null (path1)`。补：
  `export APPDATA="C:\Users\Marvel\AppData\Roaming" PROGRAMDATA="C:\ProgramData" && env "ProgramFiles=C:\Program Files" "ProgramFiles(x86)=C:\Program Files (x86)" dotnet <cmd>`
- dotnet 输出会被沙箱吞掉（PowerShell 工具偶尔也完全无回显）→ **重定向到文件最稳**：`dotnet test ... > out.txt 2>&1; echo "EXIT=$?"`，再 `grep -a`（中文日志必须 `-a`）。
- `--no-build` 前必须**刚 build 过目标项目自己**（共享 LightORMTest.dll 在各 provider bin 下是独立副本）。
- NuGet：本地源 `E:\GitRepositories\LocalNuget`；全局包目录 `E:\Nuget\.nuget\packages`。反编译 `C:\Users\Marvel\.dotnet\tools\ilspycmd.exe`。
- **不要用 `git stash`**（本仓库会留残缺 `refs/stash`，并让 status/log/diff 因 submodule `src/GenShared` 报错）；清理用 `git update-ref -d refs/stash`。submodule 本地完整副本在 `E:\GitRepositories\MT.Generators.Shared`；ssh 被沙箱拒绝，重克隆需用户本地执行。
- 判断"文件/对象是否真的缺失"要用**第二个独立工具**（PowerShell Test-Path）交叉验证，别只信 bash 沙箱。回收站可恢复 git 对象：枚举 `$R*` → zlib 头卷积 → 解压后以 `blob `/`tree `/`commit ` 开头即为对象 → 对解压内容求 SHA-1 → 缺失则复制（只增不改）。**同一仓库同时开 SourceTree / UGit / VS 的 Git 工具是高危。**

## 项目约定
- 多目标框架 **net462;net8.0;net10.0**（`src/SupportedFrameworks.props`；**netstandard2.0 已移除**，别再按旧记录找它）；版本号集中在 `src/versions.props`。
- 源生成器：`LightOrmTableContextGenerator` + `LightOrmExtensionGenerator`；用生成器的项目必须 `<ImplicitUsings>enable</ImplicitUsings>`。
- `AddLightOrm` 在 namespace `LightORM`（IoCExtensions.cs）。
- 驱动：MySqlConnector 2.6.0、Microsoft.Data.SqlClient 7.0.1（net462 走 System.Data.SqlClient）。
- 测试命令：`dotnet test test/LightORMTest.Sqlite/LightORMTest.Sqlite.csproj --filter "FullyQualifiedName~Update|FullyQualifiedName~Json" -l "console;verbosity=detailed"`。SqlGenerate 目录用例只 `Console.WriteLine` **不连库**（离线可跑）。
- DatabaseUtils(WPF) → DatabaseUtils.Avalonia 迁移：阶段一/二已完成，剩 ViewModel / View / 回归。

## 方言能力开关现状
| 方言 | 现有手写开关 | 位置 | 影响 |
|---|---|---|---|
| SqlServer | ~~`SqlServerVersion`(V1/Over2012/Over2017)~~ **已删除**，改由版本推导 | `SqlServerCapabilities.Features` ← provider 构造期 `Start(...)`；adapter / MethodResolver 均接收 capabilities | `OffsetFetch`(2012+) 分页、`TrimFunction`(2017+) |
| MySQL | ~~`MySqlTableOptions.Version`~~ **已删除**，改由版本推导 | `CustomMySqlAdapter` ← `MySqlCapabilities.Features` | `GroupByRollup`(8.0+)：`ROLLUP()/CUBE/GROUPING SETS` vs 旧式 `WITH ROLLUP`；CUBE/GROUPING SETS 低版本交给数据库报错 |
| Oracle | ~~`OracleTableOptions.OverVersion`~~ **已删除**，改由版本推导 | `OracleTableHandler.UseIdentityColumn` ← `OracleCapabilities.Features` | 自增列 IDENTITY vs 序列+触发器 |
| Oracle | ~~DDL 写死 `JSON` / 写死 `JSON(:p)`~~ **已接能力位** | `OracleTableHandler.UseJsonNativeType` / `CustomOracleAdapter.UseJsonNativeType` ← `OracleFeatures.JsonNativeType` | 列类型 `JSON` vs `CLOB`；路径更新值 `JSON(:p)` vs `:p FORMAT JSON` |
| SQLite | ~~`JSONBackend.Binary` 无条件生效~~ **已接能力位** | `SqliteCapabilities.UseBinaryJson()` ← `SqliteFeatures.Jsonb`(3.45+) | 老版本自动降级 `jsonb_*` → `json_*`；`OpenBeforeRead => false`（**探测不建库文件，实测**） |
| PG / KingbaseES / Dameng | **不接入探测** | — | 实测没有按版本二选一的分支 ⇒ 探测结果无处可用，接入只会白搭一次连库开销 |

- **未知版本的基线策略是方言私有的，可以相反**：Oracle / MySQL 取**保守**（未知 → `None`：序列+触发器、CLOB、旧式 `WITH ROLLUP`），因为新写法在旧库直接失败；SqlServer / **SQLite** 取**乐观**（未知 → `Full`），因为它们的旧写法在新库仍可跑，且乐观能保证 `DetectVersion=false` 的 ToSql 场景**生成出与引入探测前一致的 SQL**（SQLite 的 `JSONBackend.Binary` 正是靠这条不变）。别把某一方的策略当通用规则。
- **能力位只登记"有多套写法"的能力**（用户口径：只在涉及不同实现的地方才按版本判断）。SqlServer 只留 `OffsetFetch`（`OFFSET/FETCH` vs `ROW_NUMBER` 包裹）与 `TrimFunction`（`TRIM()` vs `LTRIM(RTRIM())`）。像 `JSON_VALUE`/`JSON_MODIFY`（仅 2016+）、`STRING_AGG`（仅 2017+）只有一种实现 → **不判版本**，低版本交给数据库报错（"自己抛 NotSupported 和数据库报错差别不大"）。
- **SqlServer 分页的硬约束**：`OFFSET ... FETCH NEXT` 前面必须有 `ORDER BY`，否则报 `Incorrect syntax near 'OFFSET'` / `Invalid usage of the option NEXT`。`CustomSqlServerAdapter.Paging` 在 `builder.OrderBy.Count == 0` 时按主键补一个 `ORDER BY`；`SelectBuilder` 正是 `OrderBy.Count > 0` 才写 ORDER BY，故该信号精确。**注意**：这条路径以前从未被覆盖——老代码 `Paging` 只判 `== Over2012`，而测试用的是 `V1`/`Over2017`，全部走 ROW_NUMBER 分支，所以接上探测后（探测到 16.0）才第一次真正执行 `OFFSET/FETCH`。
- **探测是可选特性**：`TableOptions.SpecificVersion` 有值 → 不探测（`ForVersion`，`Source=Explicit`）；`TableOptions.DetectVersion = false`（基类，默认 true）→ 不探测；连接串为空 → 不探测。三条出口 + 探测失败，各方言按自己的基线（SqlServer=完整功能）。典型用途：ToSql 纯 SQL 生成场景（不连库）。
- 版本在**构造期烘焙**：`SqlServerProvider` 构造里先建 `Capabilities`，再 `SqlServerTableHandler` / `SqlServerMethodResolver(Capabilities)` / `CustomSqlServerAdapter(Capabilities, …)`（MySQL/Oracle 同序：必须先取 `option.NewFactory ?? XxxFactory.Instance`，因为原来 `DbProviderFactory` 属性是在最后一行才赋值的）。
- **进程内版本切换**：SqlServer 的 `SqlServerMethodResolver` 现在有无参构造（= `Baseline` = 完整功能），公开构造改收 `SqlServerCapabilities`。
- **能力档案基类只有一层**（`src/LightORM/Implements/DbCapabilities.cs`）：`DbCapabilities<TDatabase, TDatabaseFeatures>`，给 `Baseline` / `ForVersion` / `Start` / `ServerVersion` / `Version` / `Source` / `IsResolved` / `Override` / `Features`；方言给 `ProbeTarget`（抽象）+ `MapFeatures`（抽象），`PrepareProbeConnectString` / `OpenBeforeRead` 是**虚成员带默认值**（默认不改写连接串、默认先 Open）。
  - `ProbeTarget` / `PrepareProbeConnectString` / `MapFeatures` **都是实例成员**（见下条原因）。
  - **不要为"无能力位的方言"拆一个 1 参基类**：那层会变成零实现的死抽象。用户口径是"探测的唯一目的是喂能力位，没有能力位就不要探测"。若将来真出现"要探测但暂时没能力位"的方言，再补一个框架自带的空枚举 + `MapFeatures` 默认实现。
  - **net462 / netstandard2.0 不支持接口静态抽象成员**（`static abstract` 需 .NET 7+ 运行时），基类也没有抽象静态方法 ⇒ 静态工厂要拿方言信息，只能 `new TDatabase()`（`new()` 约束）借一个**空实例**当元数据载体，读出探测键/连接串改写/`OpenBeforeRead`，再把 `DbVersionProbe` 写回该实例返回（CRTP 下基类通过类型参数访问 `protected` 成员是合法的）。
  - `Probe` 是 `protected { get; set; }`（原为 `public required`）；`OpenBeforeRead` 是 `protected virtual`，**SQLite 覆盖为 false**——它未 Open 即可读 `ServerVersion`，而 Open 会在文件不存在时建出空库文件（纯 SQL 生成场景绝不能碰）。
- **实测服务端版本串**（写临时 MSTest 打印 `conn.ServerVersion`，跑完删除）：Sqlite `3.46.1` / PG `16.4 (Debian 16.4-1.pgdg120+1)` / **KingbaseES `12.1`（是 PG 兼容版本号，不是 `V009R001C001B0025`）** / Dameng `8.1.4.6`。
  - 备用（PG/KES/Dameng 目前**不**接入探测，故未使用，需要时直接可用）：`NpgsqlConnectionStringBuilder.Timeout`、`KdbndpConnectionStringBuilder.Timeout`、`DmConnectionStringBuilder.ConnectionTimeout` 三者都存在且已编译验证；SQLite 无需改写连接串。
- **达梦 JSONB 实测（DM 8.1.4.6）**：`CREATE TABLE t (C1 JSONB)` 能建成功，但 `JSONB_VALUE` / `JSONB_QUERY` **函数不存在**（`JSON_VALUE` 正常）。⇒ `JSONBackend.Binary` 在达梦不可用，但**与版本无关**，属"只有一种写法"，**不登记能力位**，保留默认 `Text`。
- **`Resolve` 表达式缓存是静态且方言无关**：key = `(SqlAction, Hash, NestedLevel)`（`Utils/ExpressionResolver.cs`），无方言维度 → 同进程内不同方言/不同版本的同类 provider 会串味（**既存缺陷，非版本探测引入**）。
  - 正确键模型 = `(SqlAction, 标识符引用身份, Hash, Depth)`；身份至少含 `Emphasis` + 生效引用开关（`QuoteIdentifiers ?? UseIdentifierQuote`）。**只放 `Emphasis` 不充分**（Oracle 与 PG 都是 `""`；且同方言逐语句 `.QuoteIdentifiers()/.NoQuoteIdentifiers()` 就翻转）。
  - **参数前缀不是维度**：解析期不读 `database.Prefix`，它由 `SqlBuilder.HandleSqlParameters` 的 `AttachPrefix` 事后补。
  - **状态（09-17）：该改动已整体回滚**（用户决定先不处理），`ExpressionResolver.cs` 与 HEAD 零差异；详细反例与证据留档在 `memory/2026-09-17.md`。重新捡起时**先查**"多方言同进程时 PG 输出整段是 Oracle 的"那条异常（疑点：投影段 `ResolveContext.Database` 不是当次 adapter / `TryGet` 探不到）。
- 版本探测线**已落地**（保留中）：`Models/DbServerVersion.cs`（`CapabilitySource` + 版本模型）、`Implements/DbVersionProbe.cs`（构造期后台探测：单飞 / 限时 / 永不 fault / 后台自愈，**零方言词汇**）、`Providers.Oracle/OracleCapabilities.cs`（`[Flags] OracleFeatures` + `MapFeatures`，方言语义归 provider）。核心只做机制，不认识任何方言能力名。
- **显式版本优先于探测**：`TableOptions.SpecificVersion`（**已在基类** `DbStruct/TableGenerateOption.cs:9`）有值时，`OracleCapabilities.Start(..., specificVersion)` 走 `ForVersion` → **不发起后台 Task、不建连**（实测 0ms / `Source=Explicit`）。
  - 坑：`DbVersionProbe.None` 原为**共享单例**而 `Override` 改实例字段 → 已改为 `=> new(null)` 每次新实例，否则显式覆盖会污染 `OracleCapabilities.Baseline`。
- **Oracle 身份列 DDL 的两条硬约束（实测 21c，必须同时满足）**：`GENERATED ALWAYS AS IDENTITY` 必须紧跟数据类型（写在 `NOT NULL` 之后报 `ORA-00907`），且身份列不能显式写 `NULL`（报 `ORA-30670`）。见 `OracleTableHandler.Writer.BuildColumn`。
- **连接超时由方言"改写连接串"**（核心不枚举、不猜测键名）：`DbVersionProbe.Start(..., Func<string,int,string?>? prepareConnectString)` —— 核心只给策略（探测必须秒级返回），表达方式是方言私有知识。Oracle 用强类型 `new OracleConnectionStringBuilder(cs){ ConnectionTimeout = seconds }`（实测改写生效：黑洞地址 5090ms → 2001ms，构造仍 1ms 不阻塞，读 Version 恰好一次超时 2014ms → Assumed）。
  - 不用"超时键名字符串"的理由：写错只能运行期发现；且实测 SqlServer/MySql/PG/KES **都接受 `Command Timeout` 且读回值相等**（映射另一属性）⇒ 按键名试探无法区分语义，"写回读校验"也兜不住。
  - 坑：ODP.NET 的 `OracleConnectionStringBuilder` 输出会**把键名大写归一化**（`CONNECTION TIMEOUT=2`），键名本身大小写不敏感，别用原文 Contains 做断言。
  - 委托 `internal static string? PrepareProbeConnectString(cs, seconds)`：返回 null / 抛异常 → 核心沿用原连接串。
- **Oracle JSON 能力的版本分界（实测 + 官方文档）**：原生 `JSON` 列类型 / `JSON()` 构造器 / `JSON_TRANSFORM` 均 **21c+**（19c 建 `JSON` 列报 ORA-00902；`JSON_TRANSFORM` 被回移到 19c 但**仅较新 RU**：19.3 无、19.17 有 ⇒ 版本号不可靠）。`JSON_VALUE`/`JSON_QUERY` 需 **12.1.0.2+**，且**对 CLOB 文本列同样有效**（无需 `IS JSON` 约束）。`FORMAT JSON` 12.1 起语法被接受但**18c 才真正生效**（12c 文档标注"仅为语义清晰"，含该子句会不稳定地影响输出）。`CAST('...' AS JSON)` 在 21.3 XE 上报 **ORA-00600 内部错误**，别用。
  - 非原生分支用 `:p FORMAT JSON` 而非 `JSON_MERGEPATCH`：后者是**整文档合并**语义，等价不了"设置某路径"（对象会合并、数组下标无法设），会静默写错数据。
  - `SqlFn.JsonSet` 口子**不接能力位**：它的值是用户手写的 SQL 表达式（PG 侧要写 `'"NewName"'`），套 `FORMAT JSON` 会把 `'test'` 变成非法 JSON 文本。
- **`InternalCreateTableAsync` 吞异常并 return false**（`ExpressionCoreSqlContextMethodImpl.cs:211`）→ DDL 失败不抛，只在后续操作以 `ORA-00942 表或视图不存在` 暴露。排查 ORA-00942 先怀疑建表静默失败。
- `ExpressionSqlOptions.SetDatabase` 只保留每个 `DbBaseType.Name` 的**第一个** adapter（`customDatabases`），`ResolveContext.Create(DbBaseType)` 走它；且 `databaseProviders/customDatabases/defaultDbKey` **全是 static 全局**，同名 key 走 `TryAdd` = **先注册者胜、后来者静默丢弃**（多方言 A/B 必须用具名 key + `SetDefault` 切换）。
- `Builder/AdapterCapability.TryGet<T>` 已存在：穿透 `ScopedDatabaseAdapter` 探测能力接口 → 新增能力不必改 `IDatabaseAdapter`（不破坏第三方实现）。

## 反向读表结构（`GetTablesSql` / `GetTableStructSql` / `ParseDataType`）
- 现状（2026-09-18）：**七个方言全部实现**，不再有 `NotImplementedException`。
- 达梦实现（`src/Providers/LightORM.Providers.Dameng/DamengTableHandler.cs`）——**坑最多，动手前先看这几条**：
  - 列表：`SELECT TABLE_NAME AS TableName FROM USER_TABLES ORDER BY TABLE_NAME`。
  - 结构主表 `USER_TAB_COLUMNS`（Oracle 风格），另需三张辅助对象（全部 `LEFT JOIN`，挂不上只会让该列退化为 `NO`/空串）：
    `USER_COL_COMMENTS`（注释）、`USER_CONSTRAINTS`+`USER_CONS_COLUMNS`（主键）、
    **`SYSCOLUMNS`**（经 `SYSOBJECTS.NAME=tc.TABLE_NAME AND TYPE$='SCHOBJ' AND SUBTYPE$='UTAB'` 定位）。
  - **`USER_TAB_COLUMNS` 没有 `IDENTITY_COLUMN` 列**（实测报"无效的列名"），IDENTITY 列的 `DATA_DEFAULT` 也是空的 ⇒ 自增只能从 `SYSCOLUMNS.INFO2` 的 bit0 判：`(NVL(s.INFO2,0) & 1) = 1`。实测 IDENTITY 列 `INFO2=1`、普通列 `0`。
  - **`USER_TAB_COLUMNS.DATA_LENGTH` 对 `NVARCHAR2` 是字节数**：`NVARCHAR2(64)` 给 128，且 `CHAR_LENGTH=0`。
    **`SYSCOLUMNS.LENGTH$` 才是字符数**（给 64）⇒ Length 一律取 `s.LENGTH$`。这条最容易踩。
  - **JSON / JSONB 物理存成 `BLOB`**（`DATA_TYPE='BLOB'`），与真 BLOB 无法靠类型名区分。
    判别特征：`SYSCOLUMNS.SCALE = 16384`（达梦给 JSON 的 BLOB 打的内部子类型标记，普通 BLOB 为 0）。
    SQL 里改写成 `CASE WHEN DATA_TYPE='BLOB' AND s.SCALE=16384 THEN 'JSON' ELSE DATA_TYPE END`。
    另有官方视图 `USER_JSON_COLUMNS(TABLE_NAME,COLUMN_NAME,FORMAT,DATA_TYPE)` 可用，但它是随 JSON 特性附带引入的，
    用 `SYSCOLUMNS` 少一层"视图是否存在"依赖（失败模式也更温和：读成 byte[] 而非整条 SQL 报错）。
  - **`DATA_SCALE` 不是纯小数位**：非数值类型也占它 —— JSON/JSONB=16384、`NVARCHAR2`/`NVARCHAR`=7、`NCHAR`=8、`TIMESTAMP`=6。
    所以数值分支要 `WHEN s.TYPE$ IN ('DECIMAL','NUMERIC','NUMBER','DEC') AND s.LENGTH$ > 0` 双重限定。
  - Length 表达式：LOB 类（TEXT/CLOB/BLOB/IMAGE/LONGVARCHAR/LONGVARBINARY）给 `''`；数值类给 `"精度,标度"`（标度 0 时只给精度）；其余 `s.LENGTH$ > 0` 才给 `TO_CHAR(s.LENGTH$)`，否则 `''`。
  - `ParseDataType`：`DATA_TYPE` 是**不带括号的纯类型名**（`VARCHAR2`/`DECIMAL`/`CHAR`），所以定点细分只能靠 `column.Length`（`"18,2"` / `"9"`）。
    `NumericType` 分档：带逗号 → decimal；无精度信息 → **decimal**（达梦裸 `NUMBER` 是浮点语义，给 int 遇小数会炸）。
  - 类型事实（实测 DM 8.1.4.6）：**`BOOLEAN` 和 `UUID` 不能建列**（"无效的数据类型" / "非法的基类名"）；
    `NCLOB`/`XMLTYPE` 被归一化成 `TEXT`；`BINARY_DOUBLE`→`DOUBLE`；
    **`FLOAT` 是 8 字节双精度、`REAL` 才是 4 字节单精度**（所以 `ParseDataType` 里 FLOAT→double、REAL→float，与 writer 的 `Single→FLOAT` 刻意不对称）。
  - 与 writer 不对称是刻意的：`bool` 写 `CHAR(1)`、`Guid` 写 `CHAR(36)`，读回都只能给 `string`（真 `CHAR(1)` 多存 `'Y'`/`'N'`，猜 bool 会反序列化失败）。口径同 SQLite：**不做 writer 私有约定的逆向猜测**。
  - 达梦支持 `DROP TABLE IF EXISTS`（探针用得上）。表名精确匹配（`WHERE tc.TABLE_NAME = '{t}'`），不做 `UPPER()` 模糊，避免大小写不同的同名表混在一起。
- 探针方法（本次全程这么干的，以后摸新方言照此办理）：写临时 MSTest 类 + `DmConnection` 直连，
  先**逐类型 `CREATE TABLE ("C1" <type>)`** 打出支持/不支持，再建一张"全类型表"用 `SELECT * FROM USER_TAB_COLUMNS` /
  `SYSCOLUMNS` 打印**每个列的元数据**（DATA_TYPE/DATA_LENGTH/DATA_PRECISION/DATA_SCALE/CHAR_LENGTH/LENGTH$/SCALE/INFO2），
  最后把**最终 SQL 原样预演一遍**。比反复改 C# 跑测试快一个数量级。
- SQLite 实现（`src/Providers/LightORM.Providers.Sqlite/SqliteTableHandler.cs`）：
  - 列表：`sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite\_%' ESCAPE '\'`（排除引擎内部表）。
  - 结构：用**表值函数 `pragma_table_info('t')`**，**不能**用 `PRAGMA table_info(t)` 命令 —— 命令不能出现在 SELECT/子查询里，而"是不是 rowid 别名"要数同表主键列个数。
  - `IsIdentity`：`pk>0 AND upper(trim(type))='INTEGER' AND (SELECT COUNT(*) ... pk>0)=1`。
  - `Nullable`：`notnull=1 OR pk>0` → `'NO'`；`Comments` 恒 `''`（SQLite 无列注释）；`Length` 从声明类型括号里切。
  - 表名要塞进字符串字面量 ⇒ 必须 `Replace("'", "''")`（带单引号的表名否则把查询拆坏）。
  - `ParseDataType`：先剥 `(len)` 再 `ToUpperInvariant()` 精确匹配；未命中按 **SQLite 官方亲和性顺序**兜底（INT → CHAR/CLOB/TEXT → BLOB → REAL/FLOA/DOUB），都不中 → `object?` 且 `return false`（NUMERIC 兜底桶不猜）。**顺序不可改**：`FLOATING POINT` 里 `POINT` 含 `INT` ⇒ 引擎判定它是 INTEGER 亲和性。
  - net462 无 `System.Range`/`System.Index` ⇒ 用 `Substring(0, paren)`，不能写 `declared[..paren]`。
  - 写/读**不一一对应**：写出去 byte/sbyte/short/ushort/int/uint/long/ulong/bool 全落 `INTEGER`，float/double/decimal 全落 `REAL`，读回只能取 `int` / `double`；要精确类型得显式声明（`BIGINT`、`DECIMAL(18,2)`）。
- 回归测试：`test/LightORMTest.Sqlite/SchemaReadTest.cs`（`SqliteSchemaReadTest`，5 用例）、
  `test/LightORMTest.Dameng/SchemaReadTest.cs`（`DamengSchemaReadTest`，4 用例）。
  两者都表自建自删、不继承 InitData 重建（达梦单次 init ≈5s，别把这类测试挂到共享 ExecutionTest 下）。

## JSON 列与方言约定
架构：序列化下沉到 `IDatabaseParameterBinder.BindParameter`；`DbParameterValue` 携带原始 CLR 值；`CustomDatabaseAdapter` 给默认 json 绑定。`src/LightORM/Utils/JsonParameterHelper.cs`：`Serialize` + `IsCompositeJsonValue`（标量白名单：string/char/数值/Guid/DateTime/TimeSpan/byte[]/Uri/Version/object）+ `IsCompositeJsonLeaf(JsonColumnContext)`。
**缓存约束**：SQL 侧"形状分支"只能由表达式结构（成员类型）决定，不能用运行时值。

| 方言 | 路径更新值参数 | 读末级标量 / 复合 |
|---|---|---|
| PG / KingbaseES | `@p::JSONB` | `(col->>'p')::T` / 同左（数组 `col->N`） |
| SQLite | `JSON(@p)` | `json_extract` / 同左 |
| SqlServer | 复合 `JSON_QUERY(@p)`；标量传原生值 | `JSON_VALUE` / `JSON_QUERY`（已修，未实测） |
| MySQL | `CAST(?p AS JSON)`（5.7.44 实测通过） | `col->>'$.p'` / 同左 |
| Dameng | `CAST(:p AS JSON)`（实测一律正确） | `JSON_VALUE`（Binary 起 `JSONB_VALUE`）/ **未验证** |
| Oracle | 21c+ `JSON(:p)`；≤19c `:p FORMAT JSON`（均在 JSON_TRANSFORM 内） | `JSON_VALUE` / `JSON_QUERY`（21c XE 通过） |

- **整列更新一律 `col = @p`**（无路径成员时）。MySQL `JSON_SET` 首参 NULL → 整体 NULL（静默丢值）；KingbaseES `JSONB_SET` 空路径不能替换整文档。
- 例外：`SqlFn.JsonSet(path,value)` 走 `XxxMethodResolver`，值参数**不序列化**；PG 侧用户自传 JSON 文本（`"\"NewName\""`）。MySQL 侧传原始 CLR 值。
- MySQL 受影响行数：`ON DUPLICATE KEY UPDATE` 插入 1 / 更新 2 → 断言按 DbType 分支。
- `DbBaseType` 是 record（按 Name 相等），Kingbase 的 Name 是 `"KingbaseES"` → 写 `DbType.Name is "PostgreSQL" or "KingbaseES"`。
- 达梦列类型默认 `JSON`（>320000 走 CLOB）；`JSONBackend.Binary` 在本机 DM8 不可用，用默认 `Text`。
- 定位 json 问题先写**方言语义探针**（Python sqlite3 / sqlcmd / DmCommand）验证函数行为，比反复改 C# 跑测试快。

## 测试基础设施
- 容器常驻：`dm8` 5236 / `kes` 54321 / `oracle21xe` 1521(`localhost:1521/XE`，lightorm_test) / `mysql` 3306（**本机 MySQL 5.7.44**：无 CTE/窗口函数，CUBE 抛 NotSupported，MySql 全量 144/155，失败 11 个全部由此而来）/ `pgsql` 5432 / `mssql` 1433。
- 用例数：Dameng 270 / Oracle 194 / Sqlite 168 / KingbaseES 100。
- **Sqlite 全量别想一次跑完**：单用例含 20s 级底噪，全量 >15min 会被工具超时打断（要用 `--filter` 收窄，如 `~Json`）。收窄跑时看到 `Group_Cube_Test` 因 `no such function: CUBE` 失败——SQLite 本就不支持 `CUBE`（MySQL 侧早就改成"交给数据库报错"，这个用例在 Sqlite 上注定失败），与版本能力位无关。
- 每个用例前 `ExecutionTest.InitData.cs` 的 `[TestInitialize]` 重建整个 schema（10 表 DROP+CREATE + 8 表清空重插）→ 每个用例都有几秒底噪，MSTest 耗时含它。
- 达梦慢的真原因：① DDL 单条约 25ms/条且每条自带提交，单次 init ≈4.8s（Kingbase 同项 0.57s）；② `DamengTableHandler.Writer` 把 CREATE/COMMENT/INDEX 各 yield 一次（10 表 = 131 条语句 = 131 次往返），PG/Kingbase 每表 yield 一次。**批量化已实测不可行**（`;` 多语句被拒；匿名块慢 7 倍）→ 只能减少 DDL 条数/次数。
- 重复测试类：Dameng / PostgreSQL / SqlServer 项目里 `ResultTest/ExecutionTest.cs` 与 `SelectResult.cs` 都继承共享 `ExecutionTest`(100 用例) → 共享套件跑两遍。
