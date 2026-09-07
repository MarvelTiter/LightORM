# LightORM 项目长期备忘

## 构建环境（关键）

WorkBuddy bash 环境缺 `APPDATA`/`PROGRAMDATA`/`ProgramFiles` 环境变量，导致 dotnet restore 报 `Value cannot be null. (Parameter 'path1')`。运行 dotnet 命令前必须：

```bash
export APPDATA="C:\Users\Marvel\AppData\Roaming" PROGRAMDATA="C:\ProgramData" && env "ProgramFiles=C:\Program Files" "ProgramFiles(x86)=C:\Program Files (x86)" dotnet <命令>
```

NuGet 本地源：`E:\GitRepositories\LocalNuget`（用户 NuGet.Config 里的 "local" 源），全局包目录 `E:\Nuget\.nuget\packages`。

补充（2026-09-07）：bash 沙箱还会吞掉 dotnet.exe 的 stdout（exit 0 但无任何输出）；dotnet 命令优先用 PowerShell 工具执行（同样需补 APPDATA/PROGRAMDATA）。反编译用 `C:\Users\Marvel\.dotnet\tools\ilspycmd.exe`。

## 项目约定

- LightORM 多目标框架：net462;netstandard2.0;net8.0;net10.0；版本号集中管理在 `src/versions.props`。
- 源生成器：`LightOrmTableContextGenerator`（`[LightORMTableContext]`）+ `LightOrmExtensionGenerator`；使用生成器的项目必须 `<ImplicitUsings>enable</ImplicitUsings>`，否则生成代码级联报 CS0535/CS0738。
- `AddLightOrm` 扩展在 namespace `LightORM`（IoCExtensions.cs）。
- DatabaseUtils（WPF Blazor Hybrid）→ DatabaseUtils.Avalonia（net10.0 + MVVM Toolkit）迁移进行中：阶段一（基础设施）+ 阶段二（纯逻辑层）已完成，2026-09-06。剩余：阶段三 ViewModel、阶段四 View、阶段五回归。迁移映射与计划见 2026-09-06 日志。
