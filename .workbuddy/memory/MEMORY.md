# LightORM 项目长期备忘

## 构建环境（关键）

WorkBuddy bash 环境缺 `APPDATA`/`PROGRAMDATA`/`ProgramFiles` 环境变量，导致 dotnet restore 报 `Value cannot be null. (Parameter 'path1')`。运行 dotnet 命令前必须：

```bash
export APPDATA="C:\Users\Marvel\AppData\Roaming" PROGRAMDATA="C:\ProgramData" && env "ProgramFiles=C:\Program Files" "ProgramFiles(x86)=C:\Program Files (x86)" dotnet <命令>
```

NuGet 本地源：`E:\GitRepositories\LocalNuget`（用户 NuGet.Config 里的 "local" 源），全局包目录 `E:\Nuget\.nuget\packages`。

补充（2026-09-07）：bash 沙箱还会吞掉 dotnet.exe 的 stdout（exit 0 但无任何输出）；dotnet 命令优先用 PowerShell 工具执行（同样需补 APPDATA/PROGRAMDATA）。反编译用 `C:\Users\Marvel\.dotnet\tools\ilspycmd.exe`。

补充（2026-09-10）：PowerShell 工具偶尔完全无输出（连 `Write-Output` 都不回显），此时改用 bash + 文件重定向，稳定可用：`cd /e/GitRepositories/LightORM && export APPDATA="C:\Users\Marvel\AppData\Roaming" PROGRAMDATA="C:\ProgramData" && env "ProgramFiles=C:\Program Files" "ProgramFiles(x86)=C:\Program Files (x86)" dotnet test ... > out.txt 2>&1; echo "EXIT=$?"`，再用 `grep -a` 读（中文日志需 `-a`）。`--no-build` 前必须确保**目标项目自己**刚 build 过——共享项目 LightORMTest.dll 在各自 bin 下是独立副本，只 build 别的 provider 项目不会刷新它，`--no-build` 会跑到旧代码。

补充（2026-09-15）：**bash 工具的 PATH 会丢失** —— 初始 PATH 缺 `/usr/bin`，`ls`/`grep`/`head`/`dirname` 全报 command not found，且 shell 状态在两次调用之间不保留。每条 bash 命令前都要加 `export PATH="/usr/bin:/bin:$PATH"`。另：**不要在此环境用 `git stash`** —— `git stash push` 在本仓库会失败并留下残缺的 `refs/stash`（`stash show/drop` 报 "not a stash-like commit"），且使其后 `git status`/`git log`/`git diff` 因 submodule `src/GenShared`（gitdir `.git/modules/src/GenShared` 缺失）报 `fatal: not a git repository`；清理残留引用用 `git update-ref -d refs/stash`。该 submodule 的本地完整副本在 `E:\GitRepositories\MT.Generators.Shared`。另外此环境对 `.git` 的可见性可疑 → **该判断已作废**。2026-09-15 17:45 用 PowerShell（独立于 bash 沙箱）`Test-Path` 复核，确认仓库是**真实损坏**，不是隔离视图。要点见 2026-09-15 日志：① 缺失 4 commit + 5 tree + 103 blob，**只影响 09-14/15 的近期本地提交**，历史分支（36 个 ref、2 个 pack `verify-pack` 通过）完好；② `refs/heads/master` 的 `3ef2783b` 在 GitHub 同名存在，`git fetch github` 可恢复；③ `.git/modules/src/GenShared` 是**被外部工具删到回收站**（17:24:52:xx），可还原；④ 回收站里**确实有** `.git\objects` 条目（被前一次的判断误否，见"恢复经验"）。git 本身从不使用回收站（不用 Shell API）⇒ 删除来自外部 Git GUI，不是 git 命令。
**排查经验**：判断"对象是否真缺失"必须用**第二个独立工具**（PowerShell `Test-Path`）交叉验证，别只信 bash 沙箱结论；回收站 `$I` 元数据可给出**精确到秒的删除时间与原始路径**（Python 在沙箱里读不到 `$RECYCLE.BIN`，PowerShell 可以）。**同一仓库同时开着 SourceTree / UGit / VS 的 Git 工具是高危操作，动 git 前先关掉它们。**

补充（2026-09-15 18:20，**git 对象损坏已恢复**，方法可复用）：
- **不要用回收站记录里的"名字"判断内容**：`$I` 里的原始路径在控制台会被截断（只显示到 `…\.git\objects\`），据此筛选会得出"回收站里没有 objects"的错误结论。**直接用内容判定**。
- **内容级恢复法**（`%TEMP%\recover_git_objects.ps1`，PowerShell，因为沙箱禁止 Python 读 `$RECYCLE.BIN`）：枚举 `$R*` 文件 → 按 zlib 头筛（`0x78`+`01/5E/9C/DA`）→ `DeflateStream` 跳 2 字节头解压 → 以 `blob `/`tree `/`commit ` 开头即为 git 对象 → 对**解压后内容**求 SHA-1 → 目标 `.git\objects\aa\bbbb…` 不存在则复制（**只增不改，天然安全**）。本次一次恢复 **382 个对象**（178 blob/163 tree/41 commit）。
- **坏 ref 会阻断 `git fetch`**（`fatal: bad object <ref>` + `did not send all necessary objects`）。处理：把坏 ref 文件**临时移出 `refs/`**（如移到 `.git/refs-disabled/`，可逆不删数据）再 fetch，取回后用 `git update-ref` 重建。
- **本地丢失的 commit 可能躺在远端的非分支 ref 上**：`refs/heads/master` 的 `3ef2783b` 在 GitHub 同名分支；`refs/heads/测试用例` 的 `321adb36` 只在 **`refs/pull/42/head`**（PR 头）。取 PR 头：`git fetch github +refs/pull/42/head:refs/remotes/github/pr-42`。
- **本环境 ssh 被沙箱策略拒绝**（`C:\Users\Marvel\.ssh\*` blocked），submodule 重克隆需让用户在本地终端执行或用本地副本 `E:\GitRepositories\MT.Generators.Shared`。
- **bash 沙箱对 `.git` 的视图会滞后**：unsandboxed 侧（PowerShell / escalate 过的命令）新建的目录，sandboxed 侧 `ls`/`git` 看不到；反之 sandboxed 看不到 ≠ 磁盘上没有。涉及 `.git` 下新建/删除，一律用 PowerShell 交叉验证。
- **submodule gitdir 重建步骤**（`.git/modules/src/GenShared` 被删时）：`git submodule update --init` 会因工作区目录非空而失败（`destination path already exists and is not an empty directory`），需手动克隆到 gitdir，再 `git --git-dir=<gitdir> config core.worktree ../../../../src/GenShared`，最后 `git -C src/GenShared checkout <index gitlink 的 sha>`。

跑测试的命令：`dotnet test test/LightORMTest.Sqlite/LightORMTest.Sqlite.csproj --filter "FullyQualifiedName~Update|FullyQualifiedName~Json" -v n`；测试输出里会带 `SQL:` + `参数:` 调试块，定位生成 SQL 问题非常有效。SqlGenerate 目录下的用例只 `Console.WriteLine` 不连库，PostgreSQL 项目也能本地跑（`--logger "console;verbosity=detailed"` 才看得到 SQL）。

## 项目约定

- LightORM 多目标框架：net462;netstandard2.0;net8.0;net10.0；版本号集中管理在 `src/versions.props`。
- 源生成器：`LightOrmTableContextGenerator`（`[LightORMTableContext]`）+ `LightOrmExtensionGenerator`；使用生成器的项目必须 `<ImplicitUsings>enable</ImplicitUsings>`，否则生成代码级联报 CS0535/CS0738。
- `AddLightOrm` 扩展在 namespace `LightORM`（IoCExtensions.cs）。
- DatabaseUtils（WPF Blazor Hybrid）→ DatabaseUtils.Avalonia（net10.0 + MVVM Toolkit）迁移进行中：阶段一（基础设施）+ 阶段二（纯逻辑层）已完成，2026-09-06。剩余：阶段三 ViewModel、阶段四 View、阶段五回归。迁移映射与计划见 2026-09-06 日志。

## JSON 列与方言约定（2026-09-11）

### 架构（方案 A：序列化下沉到方言 binder）
- 框架**不再预序列化** json 参数：`DbParameterValue` 携带**原始 CLR 值**，序列化与驱动类型化统一下沉到 `IDatabaseParameterBinder.BindParameter(parameter, column, value)`。
- `src/LightORM/Utils/JsonParameterHelper.cs`：`Serialize(object?)`（走 `ExpressionSqlOptions.Instance.Value.GetJsonHandler()`）+ `IsCompositeJsonValue(Type|object)`（标量白名单 string/char/数值/Guid/DateTime/TimeSpan/byte[]/Uri/Version/object，其余视为复合）。
- `CustomDatabaseAdapter` 实现 `IDatabaseParameterBinder` 并给出**默认 json 绑定**（json 列 → `Serialize` + `DbType.String`），所有方言（含第三方）继承即得。
- `InsertBuilder` / `UpdateBuilder` / `BatchSqlInfoExtensions` 中的 json 预序列化已全部移除，json 列与普通列同路。
- **缓存约束**：`Resolve` 的 cache key 只有 `(SqlAction, Hash, Depth)`、**不含值** → SQL 侧的"形状分支"必须由**表达式结构**（成员类型）决定，不能用运行时值。

### 各方言"按 JSON 解析文本参数"的写法（否则双重编码）
| 方言 | 路径更新值参数 | 备注 |
|---|---|---|
| PG / KingbaseES | `@p::JSONB`（binder 另声明 jsonb） | 恒等 cast |
| SQLite | `JSON(@p)` | `json_set` 对 TEXT 按字面量处理，必须包装 |
| SqlServer | 复合 `JSON_QUERY(@p)` / 标量传**原生值** | 无 `JSON()`；标量文本抛 Msg 13609；需生成期分流 |
| MySQL | `CAST(?p AS JSON)` | **5.7.44 实测 17/17 通过**；整列**必须** `col = ?p`（见下"NULL 陷阱"） |
| **Dameng（DM8）** | `CAST(:p AS JSON)` | 实测对字符串/对象/数组/数字/布尔/null **一律正确**，无需分流；binder 用框架默认 |
| Oracle | 未处理 | 用户明确跳过（Oracle 库本身不支持 json 列） |

### MySQL / MySQL 特有陷阱
- **`JSON_SET` 首参为 NULL 时整体返回 NULL** → MySQL 下整列更新**不能**写成 `JSON_SET(col,'$',val)`：列一旦被 `SetNull`/`Set(col,null)` 置空，值就再也写不回去（静默丢失，不报错）。整列一律 `col = ?p`（JSON 文本写 JSON 列时 MySQL 自动解析）。
- `SqlFn.JsonSet(路径, 值)` 的值在 MySQL 侧是**原始 CLR 值**（字符串 → JSON 字符串、数字 → JSON 数字），与 PG 需自行传 JSON 文本（`"\"NewName\""`）的约定不同，跨库用例按 `DbType` 分支。
- MySQL `ON DUPLICATE KEY UPDATE` 受影响行数按"插入 1 / 更新 2"计 → 断言需 `DbType == MySql` 分支（`ExecutionTest.cs` 既有约定）。
- 本机 MySQL 是 **5.7.44**：无 CTE（`WITH ... AS` 直接语法错误）、无窗口函数、CUBE/GROUPING SETS 抛 `NotSupportedException`（`MySqlTableOptions.Version` 默认 null → `over8=false`）。MySQL 全量 144/155，失败的 11 个全部由此而来，与 json 无关。

### 其他
- **例外**：`SqlFn.JsonSet(path, value)` 走 `XxxMethodResolver.JsonSet`，值参数是**用户原始值、不序列化**（不经 HandleJsonColumn）→ 不能加 JSON 包装；PG 侧需用户自己传 JSON 文本（测试里手写 `"\"NewName\""`）。
- json 列**整列**更新（`Set(j => j.Data, obj)` / `UpdateColumns`）：各方言统一 `col = @p`，读回反序列化，对称。
- **达梦**：列类型默认 `JSON`（原生，长度 >320000 走 CLOB）；`JSONBackend.Binary` 在本机 DM8 **不可用**（`JSONB_VALUE` 不存在、`JSONB_SET` 拒绝 `'$.a'`），用默认 `Text`。
- **达梦实例可用**：`localhost:5236`（LIGHTORM_TEST/LIGHTORM_TEST/DAMENG），`LightORMTest.Dameng` 可实跑；多测试类并发建连时 DM 会报 `6001 每个套接字地址只允许使用一次`（环境/连接数问题，非代码 bug），稍等重跑即恢复。
- 定位 json 问题的高效手段：先写**方言语义探针**（Python `sqlite3` / `sqlcmd` / DmProvider `DmCommand`）验证各 JSON 函数对各类参数的行为，再改框架，比反复改 C# 跑测试快得多。
