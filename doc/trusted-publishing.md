# NuGet Trusted Publishing 配置模板

用 GitHub Actions 的 OIDC 换取 **1 小时有效的临时 API Key**，仓库里不再保存长期 NuGet 密钥。
适用于任意 .NET 仓库；换成别的项目只需要改包路径和 .NET 版本。

> 只支持 GitHub Actions 与 GitLab CI。Gitee / Jenkins 等仍需长期 API Key。

---

## 一、GitHub 侧（每个仓库都要做）

### 1. 建 Environment（可选，但建议）

`Settings → Environments → New environment`，名字随意（本文用 `nuget`）。

- 想发布前人工审批 → 勾 **Required reviewers**（注意：只有 `publish` job 会等审批，PR 构建不受影响）
- 想限制只有主干能发布 → **Deployment branches** 选 `master`
- 用了环境级 secret 才需要在这里加 `NUGET_USER`

**不建环境也可以**，那就把下面 YAML 里的 `environment:` 整行删掉，同时 nuget.org 策略里的 Environment 留空。

### 2. 放 workflow 文件

保存为 `.github/workflows/push_nuget.yml`。**文件名必须与 nuget.org 策略里填的 Workflow File 完全一致**（改文件名 = 改策略）。

替换三个占位符：

| 占位符 | 换成 |
|---|---|
| `YOUR_NUGET_USERNAME` | nuget.org 的**个人资料用户名**（不是邮箱、不是显示名）。用户名不是秘密，直接写字符串即可；也可以用 `${{ secrets.NUGET_USER }}` |
| `nuget`（`environment:` 处） | 你建的环境名；没建环境就删掉这一行 |
| `8.0.x` | 你项目用的 SDK 版本 |

```yaml
name: publish

on:
  push:
    branches: [ "master" ]
  pull_request:
    branches: [ "master" ]

jobs:
  build:
    runs-on: ubuntu-latest

    # 写了 permissions 就等于把未列出的全部置为 none，checkout 需要 contents: read
    permissions:
      contents: read

    steps:
    - name: Checkout
      uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x

    - name: Restore
      run: dotnet restore
    - name: Build
      run: dotnet build --configuration Release --no-restore
    - name: Pack
      run: dotnet pack --configuration Release --no-build --output ./nupkgs

    - name: Upload packages
      uses: actions/upload-artifact@v4
      with:
        name: nupkgs
        path: ./nupkgs/*.nupkg
        if-no-files-found: error

  publish:
    needs: build
    # PR 上不发布：同仓库 PR 的 OIDC 是能对上策略的，这个 if 是安全边界，别删
    if: github.event_name == 'push'
    runs-on: ubuntu-latest

    # 必须与 nuget.org 策略里的 Environment 完全一致（策略里留空则删掉此行）
    environment: nuget

    permissions:
      contents: read      # 同上：声明 permissions 会清空默认值
      id-token: write     # 换临时 API Key 必需，缺了 NuGet/login 直接 403

    steps:
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x

    - name: Download packages
      uses: actions/download-artifact@v4
      with:
        name: nupkgs
        path: nupkgs

    # 临时 Key 只有 1 小时有效，且一个 OIDC token 只能换一个 key：
    # 这里 login 一次，随后把所有包一起推完，不要循环调用 login。
    - name: NuGet login (OIDC → temp API key)
      id: login
      uses: NuGet/login@v1
      with:
        user: YOUR_NUGET_USERNAME

    - name: Push to NuGet
      run: |
        dotnet nuget push nupkgs/*.nupkg --api-key ${{ steps.login.outputs.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json --skip-duplicate
```

### 3. 不想拆两个 job？

可以合并成一个（省掉 artifact 中转，PR 也不再多一次 upload）。改动是把 `environment` / `permissions` 移到 `build` job 上，删掉 upload、download 两步，并给 **login 和 push 两步都加** `if: github.event_name == 'push'`。

代价：`environment` 是 job 级静态声明，一旦给环境配了审批，**PR 构建也会卡审批**。所以「要审批」和「要单 job」只能二选一。

---

## 二、nuget.org 侧（每个仓库一条策略）

`nuget.org → 用户名菜单 → Trusted Publishing → Add a new trusted publishing policy`

| 字段 | 填什么 | 注意 |
|---|---|---|
| Policy owner | 你个人，或包所属组织 | 策略只作用于该 owner 名下的包；离开组织会让策略失效 |
| Repository Owner | GitHub 用户/组织名 | 大小写不敏感 |
| Repository | 仓库名 | |
| Workflow File | `push_nuget.yml` | **只写文件名**，不要带 `.github/workflows/` |
| Environment | 与 workflow 的 `environment:` 一致，或留空 | 两边必须同时有或同时没有，否则 `no matching policy` |
| Scopes（包 glob） | 该仓库发布的所有包 ID，如 `MyLib` 和 `MyLib.*` | 一条策略可加多条；界面只允许一条时用 `*` |

要点：

- **策略按 workflow 文件计**。同一仓库再加一个 `release.yml` 手动发布，需要再建一条策略。
- **私有仓库**新建策略是"临时激活 7 天"，期间至少成功 login 一次才会永久绑定；过期可在页面上续 7 天。
- 策略是**每次发布前校验**的，改了 workflow 文件名或环境名，记得同步改策略。

---

## 三、从「长期 API Key」迁移的最小改动

已有的 API Key 工作流，只改 4 处：

1. job 上加 `permissions: { contents: read, id-token: write }`
2. job 上加 `environment: nuget`（可选）
3. push 前加一步 `NuGet/login@v1`（`id: login`）
4. `--api-key ${{ secrets.NUGET_API_KEY }}` → `--api-key ${{ steps.login.outputs.NUGET_API_KEY }}`

**先别删旧的 `NUGET_API_KEY`**，等第一次 OIDC 发布成功、包在 nuget.org 上可见了再删。删了是不可逆的一步。

---

## 四、排错

| 现象 | 原因 | 处理 |
|---|---|---|
| `NuGet/login` 403 | job 没有 `id-token: write` | 补权限；注意写了 `permissions` 会清空默认值 |
| `no matching policy`（403） | 仓库名 / workflow 文件名 / environment 三者之一对不上 | 逐项核对；文件名只写名字不带路径 |
| 用户名无效 | 填了邮箱或显示名 | 换成 nuget.org 个人资料里的用户名 |
| `401` / 无权限推送 | 包不属于策略的 owner | 检查 Policy owner 与包所有权 |
| 推送时 key 过期 | login 之后隔太久 | 把 login 紧挨着 push |
| 发布成功但版本没更新 | 版本已存在，`--skip-duplicate` 静默跳过 | 确认 `Version` 属性；这是预期行为，job 仍显示成功 |
| 循环 login 报错 | 一个 OIDC token 只能换一个 key | 只 login 一次，循环里只做 push |

---

## 五、多仓库 / 复用

- **每仓库一条策略**，无法跨仓库共享；环境名与 secret 也都是仓库级的。
- 用户名不是敏感信息，**直接写死在 `user:`** 可以省掉每个仓库配 secret 的麻烦。
- `reusable workflow`（`workflow_call`）看着能复用，但策略里的 Workflow File 匹配的是**调用方**还是**被调用方**的文件尚未验证（GitHub OIDC 里 `workflow_ref` 与 `job_workflow_ref` 是两个 claim），上生产前先用一个测试包验证。
- 一次成功的 login 就够了，不必真的发布出包。
