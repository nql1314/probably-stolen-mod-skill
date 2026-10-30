# 正式版官方开发流程

依据：2026-10-28 本地 Build 26263409 与开发者 `probably-stolen-modding` 的 1.0 文档；已补充同提交的[官方中文指南](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md)。具体证据见 [检查基线](release-evidence.md)。以下是当前构建契约，不是所有历史版本的承诺。

## 工程与包

源码放在工作区，游戏安装目录只用作引用来源。默认 `netstandard2.1`；C# 语言版本可以较新，但不要据此调用仅现代 .NET 才有的运行库接口。按实际使用引用 `Assembly-CSharp`、Unity 模块、`0Harmony`、`Newtonsoft.Json` 等游戏自带 DLL，`Private=false`。不用旧 `Il2Cpp*` 类型或 `Il2CppInterop`。

模板构建示例（只编译）：

```powershell
dotnet build .\OfficialStarter.csproj -c Release '-p:GameDir=F:\SteamLibrary\steamapps\common\Probably Stolen'
```

游戏包结构：

```text
Mods/<包目录>/
  manifest.xml
  MyMod.dll
  Assemblies/         按需放自带依赖，并逐项列入 manifest
  Textures/
    Items/
    Paper/
  Sounds/
  Localization/
  LootTables/
  Books/
  preview.png         可选
```

本地加载器枚举 `Mods` 的直接子目录，不把根目录散放 DLL 当成官方包。每个包必须有 `IMod` 实现；纯贴图／JSON 包也需要入口 DLL。目录名与 ID 可不同，ID 才是启用、信任和数据的核心身份。

manifest 子元素区分大小写：`ID`、`Name`、`Author`、`ModVersion`、`Description`、`GameVersions/Version`、`Prerequisites/Mod`、`Assemblies/Assembly`、`Capabilities/Capability`。`ID` 允许 ASCII 字母、数字、`_-.`，不以点开头，最长 64；`Assemblies` 不得空，路径必须是包内相对 `.dll`，不能包含 `..`；`Capabilities` 必须存在，允许空。

被其他 DLL 引用的库，文件名应与程序集简单名一致。当前 allowlist 和 `ResolveModDependency` 使用文件名去扩展名匹配程序集引用；旧包的 `Forge-版本号.dll` 命名不能直接当作新依赖库名称。版本放到 manifest／发布包名，依赖 DLL 保持实际程序集名。

`GameVersions` 供 UI 比较 `Application.version`，不是 Steam Build 匹配或强制兼容保证。`Prerequisites` 当前仅在初始化后检查启用状态并报警，没有版本范围、自动订阅或拓扑排序。实际初始化发生在目录发现期间；之后才按保存顺序排序资源加载列表。

## 生命周期

当前加载链：发现本地／已安装工坊包 → manifest／启用／信任检查 → `Assembly.LoadFrom` → 找到非抽象 `IMod` 类并无参构造 → `Harmony("mod." + ID).PatchAll(assembly)` → `Init(manifest)` → `OnEnable()` → 加入 `ModLoader.Mods` → 排序、依赖提示 → 资源加载。

`IMod` 精确签名：

```csharp
void Init(ModManifest manifest);
void OnEnable();
void OnDisable();
```

使用可实例化的公开入口和无参构造。一个 DLL 有多个入口时只使用第一个；一个 manifest 列出多个有入口的 DLL 会生成多个 `LoadedMod`，共用 ID 和 Harmony owner，容易使资源、配置与清理产生歧义。一个包一个入口是本项目的设计约束，不是加载器禁止多入口。

没有 `IMod` 的依赖 DLL 会在检查入口后返回，不会走自动 `PatchAll`。若 Forge／内容库保留 HarmonyPatch 类，需要单入口明确安装这些库的必要补丁，管理所属 owner、失败撤销和退出清理；或者把补丁编进入口程序集。不能既重复安装入口补丁，又漏装库里的补丁。该方案仍需普通扫描与游戏内验证。

`Init`／`OnEnable` 只做配置定义、注册订阅等早期操作。自己的 `LoadedMod` 还未加入列表，不能靠 `GetModFolder` 或查列表完成自身初始化。资源回调按 ID 筛选自身；注册时保留具名委托，以便退出时退订。

启停设置写入 `modOrder` 并标记重启；退出时对启用实例执行 `OnDisable` 和所属 Harmony 清理。初始化失败会撤销该 Harmony owner 的补丁，但不会自动撤销入口此前完成的任意副作用：多步初始化应自行清理已订阅事件／已创建对象。

常用事件：

| 需求 | 当前入口 | 决策 |
| --- | --- | --- |
| 自定义物品 | `OnModItemDirectoryInit(ModItemDirectory)` | `dir.Add(id, factory)` 注册工厂；不在菜单阶段操作库存 |
| 启动资源 | `OnModAssetsLoaded(LoadedMod)` | 图片、本地化、声音加载流程结束；单个资源可能失败，仍检查键存在 |
| 周目加载 | `OnGameLoadedInit/Early/Normal/Late` | 在实际调用方确认新局／读档时序后读写周目状态 |
| 访客生成 | `OnGenerateCustomerVeryEarly/Early/Normal/Late/VeryLate(StoreClientManager)` | 调整生成流程；不是新立绘／阵营注册接口 |
| 开关门／睡醒／出入商店 | `OnShutterOpened/Closed*`、`OnGoingSleep*`、`OnWakingUp*`、`OnLeavingStore*`、`OnReturningStore*` | Early/Late 的准确位置需追踪目标方法，不把 GoingSleep 当保存完成事件 |
| 提示、掉落、配置 | `OnCreateTooltip*`、`OnLootTablesLoaded`、`OnLootRolled`、`OnModConfigApplied` | 改目标数据，不全局重复扫描 |

官方没有 `IMod.Update`。确有逐帧需求时使用自己拥有的 Unity 组件并管理生命周期；不要机械复制 Melon 的 Update/GUI 回调。已有官方钩子时优先使用。

## 内容与资源

物品由 `ModItemDirectory.Add(id, Func<GameItem>)` 注册，`ItemDirectory.CreateEmptyItem(id)` 可创建基础物品，`SetSpriteAndShapeFromMod(modId, spriteKey)` 使用官方物品图。使用稳定且避免冲突的物品 ID；迁移不要为了统一命名改已有身份。`ModHelper.SetSource` 能显式标注归属，尤其用于工厂位于共享库的情形。

`ModHelper.SpawnItem` 只创建对象，放入库存是另一动作；`MoveItemToInventory` 等当前返回 `void`，需要检查放置后的实际状态，不能把调用完成当成交付成功。

| 文件位置 | 读取与使用 |
| --- | --- |
| `Textures/*.png,*.jpg` | `LoadedMod.Textures`，键为文件名去扩展名 |
| `Textures/Items/*.png,*.jpg` | `LoadedMod.Sprites`，Point 过滤、100 PPU；不是自动导入任意深层 Assets 目录 |
| `Textures/Paper/*.png,*.jpg` | `PaperSprites` 与 `Textures["Paper/" + key]`，UI 平滑过滤 |
| `Sounds/*.wav,*.ogg,*.mp3` | 异步加载，声音 ID 为 `modId:soundKey`；`PlaySound` 系列调用 |
| `Localization/<locale>.csv` | 无需表头；每行第一个逗号分隔 key/value，`#` 注释、`\n` 换行；不支持跨物理行 CSV 值 |
| `LootTables/*.json` | 文件名排序加载，含 `tables`、`groups`、`patches`；工厂和掉落是两个注册环节 |
| `Books/*.json` | 官方纸张书籍系统；布局按开发者 Paper Books 文档与当前 `PaperBookRegistry` 核对 |

本地化使用 `ModHelper.GetLocalized`，当前 locale → `en` → 原 key。用延迟 `Func<string>` 获取显示文字，避免在资源载入前／语言切换前固化翻译。语言文件名以游戏 locale code 为准，不从操作系统语言猜测。

掉落注册入口：`RegisterLootTable`、`AddLootEntry`、`SetLootWeight` 等；JSON 的条目是 `{id, weight}`，patch 可指定 `table` 或 `group`，带 `add/remove/setWeight/clear`。权重不是百分比。只改所需条目，不默认清空整张表。

其他官方覆盖包括人脉升级、开局、天赋、难度、夜间地点／事件和鉴定物品，见[内容注册 API](official-content-apis.md)；窗口、纸质手册、机器面板和游戏选项见[UI 与纸质手册](official-ui-and-books.md)。资源就绪、完整钩子选取、来源标注和配置回调见[资源与钩子](official-resources-and-hooks.md)。

开发者 1.0 文档未承诺支持：新顾客立绘／阵营、重构内置 UI、改变店铺布局／核心经营循环、Underground Exchange 新交易、新可检视物品（与鉴定练习物品不同）。这些需求需单独追踪实现并评估 Harmony 路径；不能因有 ModHelper 就称全部可原生接入。

## 三种持久化

| 数据 | 入口 | 范围 |
| --- | --- | --- |
| 玩家设置 | `ModHelper.GetConfig(id)` / `ModConfig.Define` | `<persistentDataPath>/ModConfigs/<id>.json`；跨槽共享，支持 bool/int/float/string/enum 与原生设置面板 |
| 周目状态 | `Set/GetModData*` | `PlayerStore.modData` 字符串字典，实际键 `modId + ":" + key`；随游戏保存，新局独立 |
| 物品状态 | 物品 tags | 跟随物品保存／移动，不用一个全局配置值代替所有实例状态 |

`SetModData` 只改内存，调用后退出而未保存不能保证持久化。schema、迁移版本、业务幂等标记由 Mod 自己维护；避免把未知 schema 当空新局。旧 Forge 直接字典键与官方自动前缀必须显式映射，见迁移方案。

包内读取优先 `ReadModFileText/Bytes`；独立持久文件用 `WriteModDataFileText/Bytes` 等，落在 `<persistentDataPath>/ModData/<safe-id>`。这些 helpers 不要求直接文件能力；路径为包内／数据目录内相对路径。不要把设置写进 Workshop 内容目录，否则更新可能覆盖。此处 persistentDataPath 未通过本次启动实测，不能凭 Demo 日志决定真实存档目录。

## 扫描、启用与诊断

六种额外能力：`files`、`network`、`process`、`dynamic-code`、`native`、`reflection-external`。配置、官方文件 helper、物品、普通游戏钩子与游戏类型 Harmony 通常不需要它们；直接系统文件、联网、进程、动态代码、原生调用或外部／计算型反射需按扫描结果申报。第三方依赖也会扫描，不把空 `Capabilities` 当免检查声明。

扫描会拒绝：未列出的程序集依赖、自带游戏／Harmony/Cecil/MonoMod 副本、隐藏可执行资源、不透明载荷、无法解码的方法／特性、全局无 owner 撤补丁、反射或补丁加载器／信任核心等。旧混淆产物需要重新验证可读性，不能默认兼容。

用户授权游戏测试后，在官方 UI 激活／Review and enable 并重启，查看本次日志中的 `Initialized`、`audit`、`LoadError` 与功能效果。诊断命令为 `mod-scan <id或包目录>`、`mod-trust`、`mod-trust-list`。本次仅静态分析，未执行这些游戏命令。

Developer mode 是本机、每 Mod 的开发便利功能，会放行部分普通激活不会接受的构建；不自动启用，不能作为发布验收证据。必须用普通玩家激活规则验证最终包。

## Harmony 与诊断

入口程序集的自动 `PatchAll` 已在生命周期说明；不再手动补一次。选定真实类型、方法和重载后，再决定 Prefix、Postfix、Finalizer 或 Transpiler。`Prefix` 返回 `false` 会跳过原方法；Finalizer 会影响异常处理；Transpiler 依赖目标 IL，游戏更新后需重新核对。不要为了隐藏报错而吞掉异常，不使用无 owner 的全局 `UnpatchAll`。

先按日志区分：不出现在列表时查 manifest、DLL 和 `IMod`；列出但不生效时查激活与重启；出现“未加载”时查本次扫描／信任原因；资源缺失查键、目录和加载时机。没有新增扫描能力的普通补丁也可能因目标签名变化失败，不自动归因于信任。日志和 F8/F9 用法见[资源与钩子](official-resources-and-hooks.md)。

## 打包与发布

在 `D:\workzone\probably-stolen` 用户明确要求打包时，从根目录使用 `Build-Mods.ps1 -Mod <目录>`；本地只出包使用 `-NoInstall`，默认模式还会安装，执行前核对当前 `AGENTS.md` 和脚本参数。在其他工程沿用其已核实的打包入口。包只含 manifest、入口 DLL、必要依赖与资源，不附带游戏 DLL，不调用旧 Demo 发布脚本。

本地与 Workshop 使用同一种包，根部都是 manifest；同 ID 的本地包优先于 Workshop 包，来源变化可能要求重新批准。正式版 Workshop AppID `4348910`。

开发者文档的发布路径是 SteamCMD `workshop_build_item`（游戏内无上传按钮）。本地发布脚本、VDF 身份、可见性和结果核对见[Steam 工坊发布](official-workshop.md)。发布需明确请求，不因编译／打包自动登录或上传。
