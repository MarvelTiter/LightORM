# 反序列化器「源生成器方案」产出预览 · 待评估

> 本文只回答一个问题：**把 `ExpressionBuilder` 从「运行时按 `DbDataReader` 实例编译表达式树」改成「编译期按实体类型生成代码」，生成出来的东西长什么样、能做到什么程度、会丢什么。**
>
> 目录里的 `.g.cs` 文件是**生成器的预期产出**（手写等价物），不是手写给用户用的代码。

---

## 1. 先说结论

**可以做到「吞吐 ≈ 现有 Expression 方案、AOT 完全安全、首次开销从毫秒级降到微秒级」，但有明确的能力边界。**

核心矛盾你已经点出来了：生成器只有实体类型 `T`，看不到运行时的 `DbDataReader`。所以想兼容任意 provider，就必须把「类型决策」从编译期推迟到运行期。

**关键洞察是：这个推迟只需要发生一次，而不是每行每列都发生。**

```
编译期（生成器）              运行期一次（Bind）          运行期每行（Read）
─────────────────────      ──────────────────────    ─────────────────────
列集合 / 目标类型            reader.GetName(i)         reader.GetInt32(o)
所有转换分支全部展开   →      → 列序号 ordinal         reader.GetString(o)
（纯静态 C#，JIT 内联）      reader.GetFieldType(i)    无 switch、无装箱
                             → FieldKind（一次）       无委托、无反射
                             结果按 schema 缓存
```

推迟的代价是：每行每列多一次 `FieldKind` 枚举比较（一个 `cmp` + 完美预测的跳转），**不是** `GetFieldType` 调用，**不是** 装箱，**不是** 委托。

---

## 2. 生成代码长什么样

以 `LightORMTest.Models.Product` 为例（完整版见 `01-Product.Deserializer.Fast.g.cs`）：

```csharp
internal sealed class ProductDeserializer : ReaderPlanCache<ProductDeserializer.Plan>
{
    // ① Plan：一次绑定，之后每行复用
    internal sealed class Plan
    {
        public int ProductId = -1;                    // 列序号，-1 = 本次查询没这列
        public FieldKind ProductId_Kind;              // reader.GetFieldType(i) 归一化
        public int ProductName = -1;
        public FieldKind ProductName_Kind;
        // ...
    }

    // ② 入口：对象初始化器 + 静态读取方法
    public static Product Deserialize(IDataReader reader)
    {
        var plan = _cache.Get(reader);                // 一次字典查找
        return new Product
        {
            Id          = Read_Id(reader, plan),
            ProductId   = Read_ProductId(reader, plan),
            ProductName = Read_ProductName(reader, plan)!,
            CreateTime  = Read_CreateTime(reader, plan),   // init-only 也能赋值
            ModifyTime  = Read_ModifyTime(reader, plan),
        };
    }

    // ③ 列绑定：每个 schema 只跑一次，C# 字符串 switch 自动降级成静态字典
    protected override Plan Bind(IDataReader reader)
    {
        var plan = new Plan();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var kind = FieldKindResolver.Resolve(reader.GetFieldType(i));
            switch (reader.GetName(i))
            {
                case "PRODUCT_ID": case "product_id":
                    plan.ProductId = i; plan.ProductId_Kind = kind; break;
                // ...
            }
        }
        return plan;
    }

    // ④ 逐列读取：switch 全展开，无装箱、无委托、无反射
    private static int Read_ProductId(IDataReader r, Plan p)
    {
        int o = p.ProductId;
        if (o < 0 || r.IsDBNull(o)) return default;      // 列缺失 or NULL → 保持默认
        switch (p.ProductId_Kind)
        {
            case FieldKind.Int32:   return r.GetInt32(o);
            case FieldKind.Int64:   return (int)r.GetInt64(o);     // Oracle NUMBER
            case FieldKind.Decimal: return (int)r.GetDecimal(o);   // 达梦 / SqlServer
            case FieldKind.String:  return int.Parse(r.GetString(o), CultureInfo.CurrentCulture);
            default: return (int)Convert.ChangeType(r.GetValue(o), typeof(int), ...);
        }
    }
}
```

### 为什么用「对象初始化器 + 静态方法」，而不是 `var p = new T(); p.X = ...`

`Product.CreateTime` 是 `init` 访问器，方法体里直接赋值**编译不过**。对象初始化器是唯一能在生成代码里写 `init` 属性的地方。这个形式顺带还解决了 Flat 聚合属性的只读宿主问题。Expression 方案用 `Expression.MemberInit` 绕过了这个限制，生成器必须走这条路。

---

## 3. 三档生成策略

| 档位 | 生成代码 | 每行每列额外成本 | 代码体积 | AOT |
|---|---|---|---|---|
| **A · 纯静态 GetValue** | `p.X = (int)Convert.ChangeType(r.GetValue(o), typeof(int))` | 装箱 + `ChangeType`，**2~4× 慢** | 最小 | ✅ |
| **B · Compact** | `p.X = DataReaderReaders.ReadInt32(r, o, p.X_Kind)` | 一次静态方法调用（方法体大，JIT 可能不内联） | ~3 行/列 | ✅ |
| **C · Fast（推荐）** | switch 展开在实体自己的 `Read_Xxx` 里 | 一次枚举比较（完美分支预测） | ~25 行/列 | ✅ |

- A 档不推荐，只是标个下限。
- **B 档见 `02-Product.Deserializer.Compact.g.cs`**：代码量小 8 倍，适合列多的大宽表（>30 列）或实体数量很多的项目，避免编译时间爆炸。
- **C 档见 `01-Product.Deserializer.Fast.g.cs`**：性能对标 Expression，但 50 个实体 × 10 列 ≈ 12500 行生成代码。

**可以在生成器里按列数自动选档**（比如 ≤ 20 列用 Fast，超过用 Compact），两者都实现同一份 `ITableEntityInfo<T>.DataReaderDeserializer`，可无缝混用。

---

## 4. 与现有 Expression 方案的能力对照

| 维度 | Expression 现状 | 生成器方案 | 谁更好 |
|---|---|---|---|
| 列类型准确性 | `reader.GetFieldType(o)`，准确 | 同左（Bind 时读一次，同 schema 内恒定） | 平 |
| 列序号 | 逐列 `GetOrdinal` / O(n·m) 遍历 | 一次 `for` + 字符串 switch 全映射 | **生成器** |
| 可空性判断 | 依赖 `GetSchemaTable()["AllowDBNull"]` | 无条件 `IsDBNull` | **生成器**（见下） |
| 缺失列处理 | 不匹配就不生成绑定，属性留默认 | `o < 0 → default`，行为一致 | 平 |
| 类型转换 | `Expression.Convert` / `Parse` 编译期决策 | switch 展开，覆盖等价 | 平 |
| `IsDBNull` 开销 | 仅 `AllowDBNull == true` 时才检查 | 每行每列都检查 | Expression |
| 匿名类型 | ✅ | ❌ 生成不了 | **Expression** |
| `Tuple<...>` | ✅ | ❌（需拦截调用点另做） | **Expression** |
| 标量 `int` / `string` | ✅ | 可以预生成一份 | 平 |
| 嵌套匿名类型 | 显式抛异常 | 不适用 | — |
| JSON 列 | `Deserialize(string, Type)` 反射重载 | `Deserialize<T>` 泛型实参，可接 STJ 源生成 | **生成器** |
| 首次开销 | 表达式树编译 ~1–10 ms | Bind ≈ µs，**零编译** | **生成器** |
| AOT / 裁剪 | ❌ 需 `DynamicallyAccessedMembers` 兜底 | ✅ 天然安全 | **生成器** |
| 自定义 `DbDataReader` 撒谎 | 崩（InvalidCastException） | 落 `default` 分支走 `ChangeType` | **生成器** |

### ⚠️ 一处行为差异，但方向是「生成器更稳」

现有实现（`ExpressionBuilder.GetTargetValueExpression`）：

```csharp
if (AllowDBNull)          // 来自 GetSchemaTable
    TargetValueExpression = Expression.Condition(IsDBNull, Default, ReadValue, ...);
else
    TargetValueExpression = ConvertedRecordFieldExpression;   // ← 不检查 NULL
```

即：**只有 schema 报 `AllowDBNull = true` 才生成 `IsDBNull` 检查**。而 `LEFT JOIN` 右表全 NULL 的行，部分 provider（尤其 MySQL）会报 `AllowDBNull = false` 却实际返回 NULL —— 此时 `GetInt32` 直接抛异常。生成器方案无条件检查，天然避开这个坑。

代价是每行每列多一次 `IsDBNull` 虚调用。可以做「Bind 阶段读一次 `GetSchemaTable` 的 AllowDBNull 存进 Plan」来优化，但：
1. `GetSchemaTable()` 在部分 provider 上很慢甚至返回 null（SQLite、部分达梦驱动）；
2. 省下的只是一次虚调用，还引入上面那个正确性风险。

**建议：不做这个优化，保持无条件检查。**

---

## 5. 还没被覆盖的场景（必须双轨回退）

`BuildDeserializer<T>` 现有 **16 个调用点**（`ExecuteResult` / `MultipleResult` / `LightOrmQuery` / `SqlExecutorExtensions`），其中大量是匿名类型、Tuple、标量。生成器只看得到具名实体，所以必须保留回退：

```csharp
public static Func<IDataReader, T> BuildDeserializer<T>(DbDataReader reader)
{
    // 轨道 1：生成器产物
    var generated = Generated.Get<T>() ?? (StaticContext?.GetTableInfo(typeof(T)) as ITableEntityInfo<T>)?.DataReaderDeserializer;
    if (generated is not null) return generated;

    // 轨道 2：回退 Expression（匿名类型 / Tuple / 标量 / 未注册实体）
    if (ExpressionBuilder.IsAOTRuntime) throw new LightOrmException(...);
    return ExpressionBuilder.BuildDeserializer<T>(reader);
}
```

完整代码见 `05-Integration.ExpressionBuilder.cs`。

好消息：生成的 `Deserializer` 签名就是 `Func<IDataReader, T>`，和 `ITableEntityInfo<T>.DataReaderDeserializer` 完全一致 —— **接口已经就位，接入零包装、零委托适配**。

**AOT 下轨道 2 会失效**，需要靠项目里已有的 `LightOrmAotAnalyzer` 在编译期报诊断，把「AOT 模式下用了匿名类型查询」变成编译错误而不是运行崩溃。

---

## 6. 风险与未决问题

1. **命名策略覆盖不完整**：只生成 `原样 / 全大写 / 全小写` 三种 case，覆盖 Oracle（全大写）、MySQL（原样）、PostgreSQL（全小写）。驼峰/蛇形中间态（`"Product_Id"`）匹配不到 → 静默留默认值。建议加一个全局 `INamingPolicy` 钩子在 `Bind` 末尾兜底，或者对未命中列报诊断。
2. **JSON 列的 AOT**：现在走 `RecordFieldStringDeserializer(string, Type)` 反射重载。生成器版写死 `Deserialize<T>` 后可以接 `JsonTypeInfo<T>`，但这要求用户注册 `JsonSerializerContext`，是**独立的改造项**，不在本次范围内。
3. **Plan 缓存 key 的构造成本**：每次查询一次（`FieldCount` + `GetName` + `GetFieldType` 各一遍）。比现在的 `GenerateCacheKey`（遍历 `GetSchemaTable().Rows`）便宜一个数量级，但仍存在。可以考虑用 `ArrayPool` 或直接哈希成 `long` 避免字符串分配。
4. **`GetFieldType` 撒谎的 provider**：落 `default` 分支走 `Convert.ChangeType`，结果正确但慢。可选优化：首次慢路径成功后**回写修正 Plan 的 Kind**（下次同 schema 直接走快路径）。
5. **多目标框架**：`net462 / netstandard2.0` 下 `is null or DBNull` 这类模式是纯语法糖，无运行时要求，安全。但要注意 `Array.Empty<T>()`、`ValueTuple` 在 net462 上的包依赖（项目应已处理）。
6. **生成代码量 / 编译时间**：Fast 档 50 实体 × 10 列 ≈ 12500 行。建议按列数自动选档，或提供 `[LightDeserializer(Mode = ...)]` 让用户按实体指定。

---

## 7. 建议的落地路径

| 阶段 | 内容 | 收益 |
|---|---|---|
| 1 | 先给「挂了 `[LightTable]` 且在 `TableContext` 里」的实体生成 Fast 档，接入 `ITableEntityInfo<T>.DataReaderDeserializer`，`BuildDeserializer<T>` 改双轨 | AOT 下主链路打通，Expression 继续兜匿名类型 |
| 2 | 补标量（`int`/`string`/`DateTime`…）和 `Tuple<>` 的预生成 | 覆盖 `ExecuteScalar` / 多表 Tuple 查询 |
| 3 | 匿名类型：需要拦截 `Select<T>` 调用点（项目里 `LightOrmExtensionGenerator` / `TypeSetGenerator` 看起来已在做类似的事） | 覆盖 `Select(x => new { ... })`，工作量最大 |
| 4 | AOT 下把 Expression 路径从「反射执行」改成「编译期诊断报错」，交给 `LightOrmAotAnalyzer` | 问题前置到编译期 |

---

## 8. 需要你拍板的点

1. **Fast 档 vs Compact 档**，还是按列数自动选？（我倾向：按列数自动选，≤20 列 Fast）
2. **`IsDBNull` 是否做 `AllowDBNull` 预判优化**？（我倾向：不做，正确性优先）
3. **命名策略钩子**要不要加？还是严格要求靠 `[LightColumn(Name=...)]`？
4. **生成到哪**：塞进现有的 `{Type}TableInfo.g.cs`（作为 `DataReaderDeserializer` 的实现体，替换掉现在 `throw new NotImplementedException()` 那个桩），还是独立文件？（我倾向：塞进现有 `TableInfo`，那里已经有 `CreateDeserializeMethod` 的空壳等着填了）
5. **性能基线**：建议用项目里的 `PerformanceTest` / `test/BenchmarkTest` 跑一遍再定档，我这边的快慢判断是估算，没实测。

---

## 文件清单

| 文件 | 说明 |
|---|---|
| `00-Runtime.DataReaderSupport.cs` | 运行库支撑（`FieldKind` / `FieldKindResolver` / `ReaderSchemaKey` / `DataReaderReaders` / `ReaderPlanCache`），**放主库，非生成产物** |
| `01-Product.Deserializer.Fast.g.cs` | 主示例，Fast 档完整产出 |
| `02-Product.Deserializer.Compact.g.cs` | Compact 档对照 |
| `03-UserFlat.Deserializer.g.cs` | `[LightFlat]` 聚合属性 |
| `04-JsonTestModel.Deserializer.g.cs` | `[LightJsonMap]` JSON 列 |
| `05-Integration.ExpressionBuilder.cs` | `BuildDeserializer<T>` 双轨改造 |
| `_verify/` | 可编译的验证工程（含实体存根，直接 `dotnet build` 即可验证上述语法） |

> **注意**：`LightORM.Utils.JsonHandler`（`04` 用到）目前主库里**不存在**，需要新增 —— 它的职责是把现在的
> `ExpressionBuilder.RecordFieldStringDeserializer(string, Type)` 改成泛型版 `Deserialize<T>(string)`，
> 内部再决定走 `JsonSerializer.Deserialize<T>` 还是用户自定义的 `IJsonHandler`。这是 JSON 列 AOT 化的前置项。

### 自行验证

```
cd doc/aot-preview/_verify && dotnet build
```

`_verify/Stubs.cs` 是为绕开主库依赖写的临时存根（`LightTable` / `LightColumn` / `LightNavigate` / `LightFlat` / `LightJsonMap` / `Role` / `UserRole` / `JsonHandler`），
`Models/` 下是从 `test/LightORMTest/Models/` 原样拷过来的实体。**当前沙箱环境的 NuGet 有问题（restore 阶段报 `Value cannot be null. (Parameter 'path1')`），我没能在这里跑通编译；上面的代码是逐行审校的，但请你本地 build 一次确认。**
