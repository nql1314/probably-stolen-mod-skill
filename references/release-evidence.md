# 正式版检查基线与证据

检查日期：2026-10-28（Asia/Shanghai）。本次做只读静态分析、技能整理与最小模板编译，未运行游戏。

## 当前安装

- 游戏：`F:\SteamLibrary\steamapps\common\Probably Stolen`。
- Steam manifest：`F:\SteamLibrary\steamapps\appmanifest_4348910.acf`，AppID `4348910`、buildid `26263409`；不要把用户标识等无关字段抄入资料。
- `app.info`：Questing Goose Studio / Probably Stolen。
- `Mods` 在检查时为空；本次按文件扩展名检查安装目录未发现附带 Markdown／源码工程／教程。文档通过开发者仓库获得。
- `Probably Stolen_Data/Managed/Assembly-CSharp.dll`：4,572,424 bytes，MVID `02b354fd-9876-4ddf-8920-1a9ea4b27603`，Unity Mono / CIL / AnyCPU；不是 Demo 的 IL2CPP 包装程序集。引用 `netstandard 2.1.0.0`、`0Harmony 2.4.2.0`、`Newtonsoft.Json 13.0.0.0`。
- REA `inspect_managed_artifact` Evidence ID：`ev_896c538e40e2ac66a032c05c39bc1936671f20419cdd49c638bb74d60590152b`。元数据 coverage complete；这不等于运行行为验证。以下方法体证据来自 ILSpy 重建，不是 REA 运行观察。

## 开发者资料

仓库：[dragonpan2/probably-stolen-modding](https://github.com/dragonpan2/probably-stolen-modding)。本次读取提交 `accceead2d7b63bc1746bcb053e6ab41bea75d2a`，提交说明 `1.0 doc update`，日期 2026-10-28。

- [README 固定版本](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/README.md)：netstandard2.1、引用、manifest、自动安装脚本的副作用。
- [ModdingGuide 固定版本](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.md)：官方支持边界、资源、内容、配置、存档、扫描和 SteamCMD 发布。
- [ModdingGuide 中文固定版本](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md)：2026-10-28 按用户指定链接追加整理；GitHub 返回同提交，文件 blob 为 `b13e5089c6715d94f092db15c817a4d13afb32ff`，没有用网页访问日期冒充版本。

先查主题章节，不全文搬运。文档代码中的示例版本不能当成本机 Application.version。本次未读取／执行 `create-mod.bat`，其自动部署副作用由 README 描述。

### 中文指南的技能映射

| 指南主题 | 维护位置 |
| --- | --- |
| 示例工程、存档责任、支持边界、能力扫描、Harmony、加载诊断 | [官方开发流程](official-workflow.md)与[迁移方案](release-migration.md) |
| ModLog、开发工具、物品／顾客来源、资源、本地化、钩子、保存数据、模组配置 | [资源与钩子](official-resources-and-hooks.md) |
| 掉落表、人脉升级、起始方案／天赋、难度、夜间地点／事件、鉴定物品 | [内容注册 API](official-content-apis.md) |
| 自定义 UI、纸质手册、机器窗口、游戏选项 | [UI 与纸质手册](official-ui-and-books.md) |
| 工坊包、SteamCMD、条目 ID、首次发布／更新 | [Steam 工坊发布](official-workshop.md) |

中文指南新增主题是文档证据，未在本次重新反编译全部 API、运行游戏或上传工坊；它不扩大下方历史静态证据的验证范围。本地发布封装还只读核对了 `D:\workzone\probably-stolen\Publish-Steam.cmd`、`Publish-Steam.ps1` 与 `docs/STEAM_PUBLISHING.md`，操作前应读取其当前版本。

## 本地方法证据

选定类型的 ILSpy 重建保存在：`D:\workzone\probably-stolen\output\modding\release-26263409\decompiled`，处于仓库已忽略的 `output` 中，不分发游戏源码或 DLL。它们仅是本构建快照，更新后不可直接作为当前事实。

| 文件／符号 | 本次确认内容 |
| --- | --- |
| `IMod.cs` | Init/OnEnable/OnDisable 三个接口 |
| `ModLoader.cs` / Awake、LoadAllMods、LoadModDll | 目录发现；自动 PatchAll 在 Init 前；LoadedMod 在 OnEnable 后加入；资源随后加载 |
| 同上 / ParseManifestCore | XML 元素、ID、Assemblies 和 Capabilities 校验 |
| 同上 / ApplySavedOrder、ValidatePrerequisites | 排序发生在初始化后；依赖检查仅警告 |
| 同上 / CheckEnableCore、EvaluateTrust、ResolveModDependency | 依赖文件要求、扫描与信任、按已列路径解析依赖 |
| 同上 / LoadTextures、LoadLocalization、LoadSoundsCoroutine | 资源路径、非递归搜索、CSV 解析和资源完成事件 |
| `ModScanner.cs` / Pass.Run | 当前 manifest 依赖 allowlist、游戏／第三方程序集检查 |
| 同上 / CheckPatchAttributes、Hard 调用 | loader-target、unpatch-all、不可读代码和隐藏资源等拒绝条件 |
| `ModHook.cs` | 事件签名与逐个处理器异常记录；未穷举所有 Fire 调用者 |
| `ModHelper.cs` / ModDataKey、ModDataStore、SetModData | owner:key 前缀；只写 PlayerStore 字典；没有立即存盘 |
| `PlayerStore.cs` / modData、SaveGame、WriteSlot | modData 标注 ES3Serializable；游戏保存与历史轮换；不承诺跨版本存档兼容 |
| `ModConfig.cs` | ModConfigs 路径、Define 重载与配置持久化 |
| `ModItemDirectory.cs`、`ItemDirectory.cs` | 物品目录初始化事件与 CreateEmptyItem |
| `ModLootFile.cs` 与四个 JSON 类型 | LootTables 文件、排序、tables/groups/patches 结构 |
| `ModUIManager.cs` / VersionMatchesGame | UI 用 Application.version 比较兼容标签 |
| `ModWorkshop.cs` | 正式版 AppID、订阅安装目录加载；上传步骤依据开发者文档 |
| `ModSecurityDebug.cs` | mod-scan、mod-trust、mod-trust-list 命令 |

ILSpy 本机入口 `ilspycmd.exe` 被环境指向旧的 .NET 8 runtime；本次用已安装的 `C:\Program Files\dotnet\dotnet.exe` 运行 `.dotnet\tools\.store\ilspycmd\11.0.0.9375\ilspycmd\11.0.0.9375\tools\net10.0\any\ilspycmd.dll`。未升级工具或改全局环境。可用 `-t <类型> <程序集>` 重新生成聚焦快照；此命令反编译文件，不启动游戏。

## 旧工程迁移依据

位于 `D:\workzone\probably-stolen\mods-melonloader`：

- `Directory.Build.props/targets`：net6/Demo/MelonLoader/Il2Cpp 引用与统一 Forge 依赖。
- `speed-up/Plugin.cs`：多种 Melon 生命周期回调。
- `mod-manager-bootstrap/Plugin.cs`：MelonLoader 排序／程序集扫描接管。
- `automated-logistics/Plugin.cs`：Forge 最低版本和构建门控；`Integration/AutomatedLogisticsRuntime.cs`：runtime_v5 与网络状态。
- `aug-mechanical-ascension/Persistence/RunStateCodec.cs`、`RunForkMigration.cs`：schema 与周目迁移。
- `Build-MechcoreProtocol.ps1`：GameAssembly 检查、旧解析目录与混淆发布结构。

位于 `D:\workzone\nicokobo-forge`：`README.md`（0.6.53）、`docs/SCOPE.md`、`docs/API_BOUNDARIES.md`、`Directory.Build.props`、`src/Nicokobo.Forge/Nicokobo.Forge.csproj` 以及 `src/Nicokobo.Forge/Diagnostics/BuildProbe.cs`、`src/Nicokobo.Forge/Runtime/Il2CppHookCompatibility.cs`、`src/Nicokobo.Forge/ForgeRunDataApi.cs` 等源码。不以旧技能名词代替源码。

## 验证界限

已确认：本地程序集元数据、选定方法静态控制流、开发者文档与包格式、旧工程依赖结构。

已执行验证：技能 `quick_validate.py` 通过（Windows Python 用 `-X utf8` 读取中文）；`assets/official-starter/OfficialStarter.csproj` Release 编译通过，0 警告、0 错误。输出仅含自身 DLL/PDB/deps 与 manifest，未复制游戏程序集。构建无安装步骤，正式版 Mods 目录未写入。

未确认：游戏内普通扫描／激活、所有 hook 的实际调用时序、模板运行、资源显示、游戏真实保存目录、旧档读取与迁移、多个独立包共享 Forge 的装载、混淆产物扫描以及 Workshop 上传。本次不把这些记为已通过，也不执行旧 Mod 的整体编译迁移。
