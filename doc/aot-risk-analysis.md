# LightORM AOT 风险分析报告（第 4 版 · 实测校正）

> 分析日期：2026-09-07　|　基线 commit：`814f419`
> 修订说明：
> - v2 依据作者澄清，确认生成器 `DataReaderDeserializer`/`CreateDeserializeMethod` 为**废弃内容**（不作为方向）。
> - v3 修正一处**机制性误判**：`Expression.Compile()` 在 NativeAOT 下**并非不可行**，而是会**退化为解释模式执行**。
> - **v4（本次）依据实测校正**：作者已将 `test/AotTest/AotOracleTest` 改造成引用真实 LightORM（`UseOracle` + `SetTableContext<TableContext>` + `Select<User>().Include(Profile)` + `JsonSerializer` source-gen）的 NativeAOT 冒烟工程，**`PublishAot=true` 发布后运行正常**（`.csproj:8` `<PublishAot>true</PublishAot>`，引用 `LightORM.Extensions.DependencyInjection`/`LightORM.Providers.Oracle`）。
>   - 该实测**证伪了 v3 把「库内反射目标（如 `ExpressionBuilder` 的 7 个 helper）在裁剪下会被裁 → 运行时 `GetMethod` 返回 null → NRE」列为高置信风险的论断**：`BuildDeserializer<T>` 是唯一反序列化入口（`SqlExecutorExtensions.cs:332` 直接转发），首次调用必执行 `ExpressionBuilder` 的 cctor（`ReflectMethod.cs:39-46` 的 7 个 `GetMethod(nameof(...))!`）；实测正常 ⇒ **这 7 个 helper 在当前 AOT 产物中存活**。
>   - 因此 v4 把「库内反射被裁」类论断从"高置信运行时风险"**降级为「当前发布配置下实测未触发、依赖运行时裁剪保留策略、需以裁剪 IL 实证」的开放项**，并将风险重心校正到**消费方应用侧的实体/JSON 模型**（它们才在裁剪边界上）。
> - **遗留的机制开放项（v4 未武断断言原因）**：为何这 7 个"仅被 `GetMethod(nameof)` 反射引用、无直接调用者"的方法在 AOT 产物中存活，存在两种互斥解释，本文均不武断采信，需用「反编译裁剪后 IL / ILC 中间产物」实证确认：(a) 该库程序集在本次发布中被整体保留（未方法级裁剪）；(b) 被方法级裁剪但因保留策略/可达分析而存活。作者当前 `LightORM.csproj` 仅写 `<IsAotCompatible>true</IsAotCompatible>`、未显式写 `IsTrimmable`——而 .NET MAUI 文档称 .NET 9+ 的 `IsAotCompatible` 会隐式将 `IsTrimmable` 置 true（意为可裁剪），与实测存活现象存在张力，故 v4 将其标为待实证。
>
> 前置：主库 `<IsAotCompatible>true</IsAotCompatible>`（三个分析器开启），IL20xx/IL30xx 警告已注解/抑制。本文区分「真修复」与「仅抑制/已搁置」。

---

## 0. 结论速览（TL;DR）

| 级别 | 结论 | 性质 |
|---|---|---|
| 🔴 P0 | **反序列化是「运行时按 reader 造映射」**：入口唯一 `BuildDeserializer<T>(reader)`，但每列走哪条 `GetXxx`/转换分支、嵌套类型、可空分支，全部由运行时 `reader.GetFieldType/GetSchemaTable` 决定后才现场拼表达式树 | **架构使然 + 已搁置** |
| 🟠 | AOT 下上述表达式树**解释模式执行**（非崩溃，是性能损失 + 每次新 schema 造树开销） | 已知代价 |
| 🟡 P2→复查 | 「运行时分支」意味着裁剪保留面依赖运行时配置；但 **v4 实测：7 个反射 helper 在 AOT 产物中存活、运行正常**，「库内反射被裁即崩」未被触发。改为「消费方实体/JSON 模型保留面 + 库裁剪保留策略」待实证复查 | 待实证 |
| 🟠 P1 | 参数绑定 `DbParameterReader` 两处 `Compile()`、访问器 `ObjectExtensions`/`ResolveHelper`（同「运行时造树」模式） | 可治理 |
| 🟠 P1 | `LightOrmTableContextBase` 孤儿、`AOTSupported` 依赖显式 `SetTableContext`、ODP.NET 需 `TrimmerRootAssembly`、`AotOracleTest` 是裸 repro | 可治理 |
| 🟡 P2 | 匿名/Tuple/标量、AotAnalyzer 覆盖面、Provider/命名边界 | 特定场景 |

**一句话（v3）**：反序列化最大的 AOT 问题**不是**「动态编译在 AOT 下跑不起来」，而是「**映射逻辑不是一份编译期固定的静态方法，而是拿到运行时 `DbDataReader` 才现场决定走哪条分支、再现场造出来**」。这条链的运行时不确定性，是裁剪/AOT 无法在编译期证明「该保留哪些成员/方法」的根本原因——`IsAotCompatible=true` 因此只能覆盖「编译产物本身的兼容标注」，覆盖不了「运行时才展开的调用路径」。

---

## 1. 反序列化：真实机制（本次修正核心）

### 1.1 结构：一个入口 + 运行时展开的调用路径

外部约 18 个映射调用点（`ExecuteResult.cs` / `ExecuteResultExtensions.cs` / `MultipleResult.cs` / `LightOrmQuery.cs:27` / `SqlExecutorExtensions.cs`）**收口到唯一入口**：

```
BuildDeserializer<T>(DbDataReader reader)        // 唯一入口
  └─ BuildFunc<T>(reader, culture)
        ├─ 运行时读 reader.GetSchemaTable()        // 列名/DataType/AllowDBNull
        ├─ 遍历 T 的属性 / 查 TableContext 列信息
        └─ 对每个成员【现场】拼表达式树 → lambda.Compile()
```

### 1.2 关键：调用路径由 `DbDataReader` 实例在运行时决定

`BuildFunc` 内部不是固定逻辑，而是**逐列读运行时 reader 后选分支**。断点（全部以 `reader` 运行时值为输入）：

| 行 | 运行时决策 | 后果 |
|---|---|---|
| `ExpressionBuilder.cs:301` | `Type RecordFieldType = reader.GetFieldType(Ordinal)` | 该列真实类型运行时才知道 |
| `ExpressionBuilder.cs:302` + `IsColumnNullable` | 从运行时 `GetSchemaTable()` 读 `AllowDBNull` | 是否生成 `IsDBNull` 分支运行时定 |
| `ExpressionBuilder.cs:379-380` | `GetSafeIntermediateType(...)` + `typeMapMethod.TryGetValue(...)` | 决定嵌入哪个 `GetInt32/GetString/GetValue` `MethodInfo` |
| `ExpressionBuilder.cs:386-400` | `byte[]` / 无符号 / 常规三路 | 调读取方法的形式运行时定 |
| `ExpressionBuilder.cs:422-448` `GetConversionExpression` | 按 `(SourceType, TargetType)` 运行时对选 直赋/string→Parse/Convert/ToBoolean/byte[]/强转 | 转换分支运行时定 |
| `ExpressionBuilder.cs:495` | `UnderlyingType.IsEnum` | enum vs 数值 Parse 运行时定 |
| `ExpressionBuilder.cs:87/149/210/224/244` | 按 `typeof(T)` 反射 `GetConstructors/GetProperties/GetProperty` | 嵌套/Flat 成员展开运行时定 |

> 也就是说：**同一 `T`、不同 provider（不同 `GetFieldType` 结果）、不同 SQL 投影（不同列集/schema），会现场生成出不同的委托**。缓存 key（`dynamicDelegates`，`ExpressionBuilder.cs:14/25-31`）= 类型 FullName + schema（列名:类型:可空）串，key 变即重新造。

### 1.3 AOT 下真正的问题是什么（v4 按实测校正）

- **「Compile 不可行」是伪命题，解释执行是代价**：`Expression.Compile()` 在 `RuntimeFeature.IsDynamicCodeSupported == false` 时走解释器，委托照常能 Invoke，但没有 JIT 出的原生路径、有逐行解释开销，每个新 schema 首次仍要现场造树。这是性能/延迟损失，不是硬阻断。
- **v4 实测证据**：`Select<User>().Include(Profile).ToListAsync()`（含 enum、可空列、`byte[] Avator`→ 触发 `RecordFieldToBytes`/`RecordFieldBytesDeserializer` 两个 helper）在 `PublishAot=true` 下运行正常 ⇒
  - `BuildDeserializer<T>` 首次执行即运行 `ExpressionBuilder` 的 cctor（`ReflectMethod.cs:39-46` 的 7 个 `GetMethod(nameof(...))!`），未抛 NRE ⇒ **这 7 个 helper 在当前 AOT 产物中存活**；
  - 「库内反射目标会被裁掉」在**本次发布配置下未发生**。v3 把此列为高置信运行时风险是**过度推断**，已降级。
- **裁剪风险的真实边界在消费方应用侧**：真正处于裁剪边界的是**应用自身声明的类型**——实体 `User`/`UserProfile` 的属性与构造器、`JsonContext` source-gen 的类型。它们靠 `BuildDeserializer<T>`/`BuildFunc<T>` 泛型 `T` 上的 `[DynamicallyAccessedMembers(PublicConstructors|PublicProperties)]`（`ExpressionBuilder.cs:19/75`）沿 `typeof(T)` 流动保留；若消费方新增一个**仅被运行时反射、无静态引用、无 DAM 流经**的嵌套/动态类型，裁剪才会在查询那刻静默缺失。
- **遗留开放项（不武断）**：helper 存活的确切机制（库被整体保留 或 被裁剪但按可达保留）需「反编译裁剪后 IL / ILC 中间产物」实证，见 §6。

### 1.4 定性：架构使然 + 已搁置

- 根因是**「一个入口，运行时按 reader 才展开路径」**，这本身是「兼容任意 provider/任意投影」的设计选择。
- 生成器「按 `T` 预生成固定反序列化代码」的方向（`CreateDeserializeMethod`）正是想**消除**这条运行时链——但因「看不到运行时 reader、对不上动态 schema」被作者**判定废弃**。
- 于是运行时保留 `ExpressionBuilder` 现场造树方案 → AOT 下落入「解释执行 + 运行时分支无法静态分析」。

---

## 2. 其余运行时动态点（与 §1 同根模式，可独立治理）

其余几处也是「运行时按实例/类型现场造 `Expression`」，同一类机制，但**决策输入是已固定的实体/参数类型**（非未知 schema），理论上可用生成/预编译消除：

| 文件 | 行 | 运行时输入 | 现保护 | 评估 |
|---|---|---|---|---|
| `Cache/DbParameterReader.cs` | 218 | 参数 `Type` + SQL 提取列 | `[DynamicDependency]`+`IL2026` 抑制；类型带 DAM | P1 |
| `Cache/DbParameterReader.cs` | 265 | 参数 `Type` | 类型带 DAM | P1 |
| `Utils/ResolveHelper.cs` | 225/238 | `Type` + 成员名 | 无 DAM | P1 |
| `Utils/ObjectExtensions.cs` | 17/35/54/75 | `Type` + `PropertyInfo` | 无 DAM | P1 |
| `Extension/MethodCallExtensions.cs` | 61/73 | 动态方法调用 | `[Obsolete]` | P2 |

- 运行时全反射建表路径：`Cache/TableContext.cs:106-152`（`ScanProperty` 标 `[RequiresUnreferencedCode]`），靠 StaticContext 兜，否则 `NotSupportedException`。
- `static readonly MethodInfo` 反射一次（`ExpressionBuilder.ReflectMethod.cs` 等）风险低。

---

## 3. 🟠 P1：注册桥接 / 裁剪保留面护栏

1. **`LightOrmTableContextBase` 孤儿**：`AddTable` 无调用点（`TableContextGenerator.cs:11-37` 的 `ModuleInitializer` 方案被注释停用）。
2. **`AOTSupported` 依赖显式 `SetTableContext`**（`ExpressionSqlBuilder.cs:74`）：漏配则 `IsAOTRuntime` 恒 false → 反射兜底。
3. **无「裁剪保留面」护栏**：运行时分支 + 抑制掩盖下，缺一个「AOT 下显式拦截不支持路径 + 编译期诊断升级」的兜底。

---

## 4. 🟠 P1：Provider 与 ODP.NET

- `SupportedFrameworks.props` 已设 `IsAotCompatible=true`，但驱动库无 AOT 注解：
  - `OracleCommand` 四个 size setter 内部 `GetType().GetProperty(...).SetMethod` → AOT 后 NRE；修复=消费端 `<TrimmerRootAssembly Include="Oracle.ManagedDataAccess" />`（已验证）。
  - 残余 IL3000 / IL3053；治本 `buildTransitive` 自动注入未做。
- `test/AotTest/AotOracleTest` 已改造成**真实 LightORM 主链路 NativeAOT 冒烟工程**（`PublishAot=true` + `UseOracle` + `SetTableContext<TableContext>` + `Select<User>().Include(Profile)` + JsonSerializer source-gen），v4 实测发布运行正常。

---

## 5. 建议的整改优先级

> 反序列化（§1）作者已放弃生成器方向且不再新增方案；本文不再列「造静态反序列化器」。

| 优先级 | 动作 | 对应 | 收益 |
|---|---|---|---|
| **P1-1** | 在映射边界加「AOT 护栏」：`BuildDeserializer<T>` 若 `IsAOTRuntime` 则显式抛 `LightOrmException`（明示无静态方案），并把 `LightOrmAotAnalyzer` 对结果映射调用升级为 **Error** | §1 | 「运行时分支 + 解释执行」的窄场景**前置、体面失败**而非查询当下才崩 |
| **P1-2** | 逐点补 DAM 收窄运行时反射保留面；对「嵌套实体/Flat/动态列类型转换」这类抑制处的保留面做核查 | §1.3 | 降低「静默裁错、查询时才崩」概率 |
| **P1-3** | 真实 AOT 冒烟回归工程（引用 LightORM+生成器+增删查改，含 Oracle root），纳入 CI | §4 | 暴露 IL/裁剪/NRE 回归 |
| **P1-4** | 决策 `DbParameterReader`/`ObjectExtensions`/`ResolveHelper` 造树点的替代或补 DAM | §2 | 收窄反序列化外的运行时动态面 |
| **P1-5** | 接通/明确停用 `LightOrmTableContextBase`；「`PublishAot` 未 `SetTableContext`」做成编译诊断 | §3 | 不依赖调用方自觉 |
| **P2** | Oracle NuGet 包 `buildTransitive` 自动根；AotAnalyzer 覆盖 SelectProvider/匿名 | §4/§5 | 收尾 |

---

## 附：证据文件速查（v3）

- 唯一入口 + 运行时展开：`src/LightORM/SqlExecutor/ExpressionBuilder.cs`（`BuildDeserializer` 17-56、`BuildFunc` 73-285、分支决策 293-577）
- 运行时分支断点：`GetFieldType`(301)、`AllowDBNull`(302/325)、`typeMapMethod`(380)、`GetConversionExpression`(417-456)、`IsEnum`(495)
- 18 个映射调用点：`SqlExecutor/ExecuteResult.cs`、`Extensions/ExecuteResultExtensions.cs`、`Models/MultipleResult.cs`、`Repository/LightOrmQuery.cs:27`、`Extensions/SqlExecutorExtensions.cs:326-333`
- 边界 DAM：`ExpressionBuilder.cs:19/75`；抑制：`ExpressionBuilder.cs:141`、`Cache/TableContext.cs:101-105`
- 废弃生成产物：`LightOrmTableContextGenerator/TableContextGenerator.CreateDeserializeMethod.cs`、`TableContextGenerator.cs:154-166`
- AOT 开关：`AssemblyInfo.cs:54-82`、`ExpressionSql/ExpressionSqlBuilder.cs:72-76`
