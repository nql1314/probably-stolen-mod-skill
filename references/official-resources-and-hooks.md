# 正式版资源、钩子与配置

依据：[官方中文指南](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md)。这是按任务整理的文档用法；本机静态证据与未验证项见[检查基线](release-evidence.md)。资源目录与 manifest 规则沿用[开发流程](official-workflow.md)。

## 日志与开发入口

- 用 `new ModLog(manifest)` 保存日志实例，再调用 `Log/Warning/Error`，前缀来自 manifest 名称。不要把实例用法误写成任意静态调用。
- F8 显示 Mod、加载器和异常日志；原版普通信息需在 F9 > Command > Toggle Verbose Log 开启。F9 主面板分 Store、Command、Spawn、Special，Modding Panel 与 Marketing Panel 是从 Special 打开的独立窗口。
- 注册自有调试按钮用 `ModHelper.AddDebugFunction(modId, category, name, action, tall)`，关闭时 `RemoveDebugFunctions(modId)`。分类常量为 `DEBUG_CATEGORY_STORE/COMMAND/SPAWN/SPECIAL/MODDING/MARKETING`；受限构建可能没有 F9。
- 下文控制台／编辑器操作仅供用户要求相应游戏操作时使用；整理资料与编译本身不启动游戏。

## 钩子选择

在 `OnEnable` 订阅具名处理器，在 `OnDisable` 退订。`VeryEarly/Early/Normal/Late/VeryLate` 表示层级，不保证任意管理器在每个时刻都可用；准确调用位置按目标构建核对。处理器异常会被隔离记录，仍须修复。

| 需求 | `ModHook` 事件 | 参数／约束 |
| --- | --- | --- |
| 注册物品 | `OnModItemDirectoryInit` | `ModItemDirectory`；`Add(id, factory)`，保留稳定物品 ID |
| 顾客生成 | `OnGenerateCustomerVeryEarly/Early/Normal/Late/VeryLate` | `StoreClientManager`；不是立绘／阵营注册 |
| 读档与新局载入 | `OnGameLoadedInit/Early/Normal/Late` | 无参；需要周目数据时从已载入阶段读取 |
| 仅新局奖励 | `OnNewGameStarted` | `string scenarioId`；方案初始物品和天赋物品放置之后，读档不触发 |
| 开关门、睡醒、出入商店 | `OnShutterOpened/Closed*`、`OnGoingSleep*`、`OnWakingUp*`、`OnLeavingStore*`、`OnReturningStore*` | `*` 为 Early/Late，无参；睡眠钩子不自动等于存盘成功 |
| 夜间维护 | `OnHandlingNightlyServicesEarly/Late` | 无参 |
| 发明家售卖货品 | `OnPlaceInventorInventoryItemEarly/Late` | `List<GameItem>` 玩家物品；用 `PlayerStore.Instance.AddDirectSellingItemToTable(itemId)` 加入货品 |
| 普通／开发提示框 | `OnCreateTooltipEarly/Late`、`OnCreateDevTooltipEarly/Late` | `RichTextBuilder, GameItem` |
| 本 Mod 的资源完成 | `OnModAssetsLoaded` | `LoadedMod`；按 `Manifest.ID` 过滤并检查所需资源键 |
| 最终掉落表／一次掷取 | `OnLootTablesLoaded`、`OnLootRolled` | 无参／`LootRollContext`；详见内容参考 |
| 配置应用 | `OnModConfigApplied` | `string modId`；只处理自己的 ID |
| 人脉购买完成 | `OnNetworkUpgradeUnlocked` | `string upgradeId`；效果执行之后 |
| 应用难度 | `OnDifficultyApplied` | `string difficultyId`；新局与读档均可能发生 |
| 夜间到达／离开 | `OnAfterhourArriveEarly/Late`、`OnAfterhourLeaveEarly/Late` | `string locationId` |
| 夜间事件开始／结束 | `OnAfterhourEventStarted/Ended` | `string locationId, string eventId` |
| 手册面板构建／重建 | `OnPaperOpened` | `string fullId, PaperUI ui`；此处绑定动作，详见 UI 参考 |

## 物品与顾客归属

`SpawnItem(id)` 返回对象，放置另用 `MoveItemToInventory/FrontCounter/BackCounter/DisplayCase/Trash/Afterhours`。这些操作需要已载入的周目；放置后检查实际容器状态。`SetItemOwnerShip(item, isOwn)` 设置玩家／顾客所有权，不等于来源标签。

目录工厂通常自动识别物品来源；工厂位于共享库时显式 `SetSource(item, modId, modName)`，来源标签为 `MOD_ID_TAG/MOD_NAME_TAG`。在顾客生成钩子里用 `manager.AddClient(client)` 加入的顾客自动带来源；其他加入路径在其抵达柜台前调用 `SetClientSource(client, modId)`。这些标签不能代替稳定物品或存档 ID。

## 资源与本地化

- `Init/OnEnable` 早于图片、本地化与声音加载。启动时依赖这些资源的逻辑等 `OnModAssetsLoaded`；不要在 `OnEnable` 通过 `ModLoader.Mods` 查找自己。
- 文件只扫描指定目录的直接子文件。`SetSpriteAndShapeFromMod(manifestId, fileStem)` 使用 `Textures/Items` 的图，透明区域参与物品占格；目录名、文件名键与 manifest ID 不能混用。
- `Textures/Paper` 用于平滑 UI 精灵图，布局常以原图两倍显示。通用 `Textures` 不会自动显示在 UI；引用方式按使用它的 API 核对。
- 声音通过 `PlaySound/PlaySound2/PlaySound3(modId, soundKey)` 接游戏音频通道；`GetSoundIdentifier` 得到 `modId:soundKey`。同一通道的新声音会切断旧声音，需要重叠时选择不同通道，不另建音源绕过玩家设置。
- 本地化文件名使用 `en/fr/de/ru/zh/ja/es/ptBR/ko/it/pl.csv`，中文是 `zh.csv`。UTF-8，无表头；首个逗号分开键和值，支持 `#` 注释、`""` 引号转义和字面 `\n`，不支持跨物理行的值。
- `GetLocalized` 按当前语言 → `en` → 原键解析，占位用 `{0}` 等 `string.Format` 格式。界面标签用延迟函数读取，不在启动时固化翻译。

## 模组配置与周目状态

| 需要 | 使用 | 生效与持久化 |
| --- | --- | --- |
| 跨存档玩家设置 | `GetConfig(modId)`、`Define`、`DefineEnum<T>` | `ModConfigs/<id>.json`，定义返回时已装入保存值 |
| 单局进度／收据 | `Set/GetModData` 及 Int/Float/Bool 版本 | 字符串存储随游戏保存；从 `OnGameLoadedInit` 读取，写内存不等于写盘 |
| 单件物品状态 | `ModifyTag/GetTagReadonly` | 跟随具体物品 |
| 包内只读资料 | `ReadModFileText/Bytes`、`ModFileExists` | 包内相对路径 |
| 不随存档槽的独立文件 | `Read/WriteModDataFileText/Bytes`、`ModDataFileExists`、`DeleteModDataFile`、`ListModDataFiles` | `ModData/<id>`，相对路径；官方 helpers 无需直接 `files` 能力 |

`Define` 的 bool 对应开关；带范围数字对应滑块；无范围数字对应输入；带 `choices` 的 string／枚举对应下拉。`OnChanged` 在应用实际变化、`entry.Set` 或配置重置时触发，载入不触发；因此初始化应直接读取 `Value`。需一次性刷新时过滤 `OnModConfigApplied(modId)`，启动时才读取的参数标记 `requiresRestart`。

配置面板的“恢复默认”先改变控件，按“应用”才生效。只有已激活并执行初始化的 Mod 才有配置项。周目存储使用 `modId:key` 命名空间，禁用不会自动清除；不在启动或读档时无条件 `ClearModData`，结构化数据的 schema 与迁移见[迁移方案](release-migration.md)。
