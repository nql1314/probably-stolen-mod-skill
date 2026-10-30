# 正式版内容注册 API

依据：[官方中文指南](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md)。以下提取注册入口、状态语义和易错点；参数重载以目标 `Assembly-CSharp.dll` 为准，文档整理不等于运行验证。

## 入口选择

| 内容 | 注册 | 查询／清理 |
| --- | --- | --- |
| 物品工厂 | `OnModItemDirectoryInit` → `dir.Add(id, factory)` | `SpawnItem`；与掉落注册分开 |
| 人脉升级 | `AddNetworkUpgrade` | `IsNetworkUpgradeUnlocked`、`GetNetworkUpgrade`、`RemoveNetworkUpgrades` |
| 起始方案 | `AddStartingScenario` | `GetStartingScenarioId`、`IsStartingScenario`、`RemoveStartingScenarios` |
| 起始天赋 | `AddStartingPerk` | `IsStartingPerkActive`、`GetStartingPerkIds`、`RemoveStartingPerks` |
| 难度 | `AddDifficulty`／`ModifyDifficulty` | `GetDifficultyId`、`GetDifficultyRules`、`IsDifficultyAtLeast`、`RemoveDifficulties` |
| 夜间地点 | `AddAfterhourLocation` | `GetAfterhourLocation`、`GetCurrentAfterhourLocationId`、`RemoveAfterhourLocations` |
| 夜间事件 | `AddAfterhourEvent` | `Start/EndAfterhourEvent`、`GetActiveAfterhourEventIds`、`RemoveAfterhourEvents` |
| 鉴定类型 | `AddAppraisalItem` | `GetAppraiserRank`、`RemoveAppraisalItems` |

除物品目录工厂外，上述定义可在 `Init/OnEnable` 注册，资源通过键延迟解析；动作中才访问已载入的商店。新 ID 加 manifest ID 前缀；迁移已有内容时保留其原 ID。`Remove*` 注销当前定义不代表清除存档，也不承诺热卸载；事件仍需对称退订。

## 掉落表

对应官方[掉落表章节](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#掉落表)。优先编辑表／表组，只有需要逐次干预结果时才用 `OnLootRolled`。

- 表：`RegisterLootTable`、`AddLootEntry`、`RemoveLootEntry`、`SetLootWeight`、`ClearLootTable`；表组用对应的 `RegisterLootGroup/AddLootGroupEntry/RemoveLootGroupEntry/SetLootGroupWeight/ClearLootGroup`。编辑带来源 modId，可早于表加载；场景重载会重新应用。
- 表条目指向物品，表组条目指向表。`AddLootEntry` 遇到同物品替换权重；注册已有表 ID 会拒绝。新增条目权重必须大于零；用 `SetLootWeight` 设零可保留但不抽取。权重是相对值，概率为权重／总重，不能把数值直接当百分比。
- 表注册不校验物品 ID；工厂未注册会得到未知物品。常见入口有 `junkTable`、`materialTable`、`packedFoodTable`、`dumpingGroundTG`；完整当前 ID 用 `loot-list` 查询。
- `RollLootTable/Group` 返回 ID，`SpawnFromLootTable/Group` 创建对象，`GetLootEntries/GetLootGroupEntries` 返回快照；最终状态在 `OnLootTablesLoaded` 后读取。
- `LootRollContext` 提供 `tableId/groupId/itemId`，直接抽表时 groupId 可空；改 itemId 会影响后续订阅者看到的结果。

数据文件直接放 `LootTables/*.json`：`tables`、`groups`、`patches` 三个顶层列表均可省略；条目使用 `{id, weight}`。文件按字母顺序应用，文件内先 tables → groups → patches；每个 patch 只指定 `table` 或 `group`，内部次序为 clear → add → remove → setWeight。只改所需条目，不默认清空原版表。

游戏内排查入口为 `loot-list [id]` 和 `loot-roll <id> [count]`，结合 `[LootRegistry]` 日志区分表 ID 错误、注册失败和低概率。

## 人脉升级

对应官方[人脉升级章节](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#人脉升级)。`AddNetworkUpgrade` 定义价格、文本函数、`onUnlock`、图标、前置条件，可设置 `tab/subtitle/subtitleLabel/recommended/cooldownDays/alreadyBought`。

- `tab` 决定标签页，用 `SetNetworkTabLabel` 提供标签；`subtitle` 决定页内卡片。复用原版卡片名即可加入原卡片，无需改原界面。
- `onUnlock` 在付款后、每次购买执行；有冷却的条目可重复购买。不会在读档时重放购买回调，持续效果应从 `IsNetworkUpgradeUnlocked` 恢复。
- 已购状态、冷却和当前价格按升级 ID 随局保存；`GetNetworkUpgrade(id)` 读取本局副本。移除定义后已存状态保留；未知前置 ID 按未解锁处理。
- `OnNetworkUpgradeUnlocked` 可观察原版或其他升级，先筛选目标 ID。`OpenWildNetwork/CloseWildNetwork` 需要商店和已载入周目。

## 起始方案、天赋与难度

对应官方[起始方案](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#起始方案)、[起始天赋](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#起始天赋)、[难度](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#难度)。

| 类型 | 必须区分的行为 |
| --- | --- |
| 起始方案 | `startWith` 是说明，发物品和改初始状态要在 `onStart` 实现；仅新局执行，随后才放天赋物品。`OnNewGameStarted` 更晚，不能拿读档钩子重复发开局奖励 |
| 起始天赋 | 注册只记录可选项和选择，Mod 自己实现效果；新局与读档均从 `IsStartingPerkActive` 恢复。`cost` 正／负／零对应消耗／提供／免费；`incompatiblePerks` 控制互斥，`extraPerkSlots` 增加栏位，`mindEditable` 允许局中修改 |
| 难度 | `configure` 编辑 `basedOn` 的规则副本，默认基于 NORMAL；`notes` 仅显示说明，自定义效果在 `OnDifficultyApplied` 实现。`IsDifficultyAtLeast` 按 order 比较，跳序章和无尽是独立开关 |

`ModifyDifficulty` 修改共享定义，后执行者可覆盖先前值，不默认修改全部原版难度。按指南，移除方案提供者后载入列表可能显示“损坏的存档”，不据此认定数据损坏或删除存档；移除天赋保留所选 ID 但无效果；缺失自定义难度会回退普通难度。这些是文档说明，实际兼容仍需指定构建验证。

## 夜间地点与事件

对应官方[夜间地点章节](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#夜间地点)。`AddAfterhourLocation` 返回 `DelegateAfterhourLocation`，也可注册自定义 `AfterhourLocation`；常用字段为 `isDiscovered/isAvailable/onArrive/onLeave/onTick/buildPanel/imageKey/backgroundKey`。

- `isDiscovered=false` 隐藏地点；`isAvailable=false` 显示但禁用。共享外出流程已处理背包和商店窗口，`onArrive/onLeave` 在黑屏期间运行，勿重复接管。
- `buildPanel(CustomUIBuilder)` 自动附返回控件；自备 `panelObject` 时自行提供返回入口，走 `ReturnFromAfterhour()`。`onTick` 仅停留时逐帧执行，不用它反复注册。
- 访问次数与地点计数器用 `GetAfterhourVisits/GetAfterhourCounter/SetAfterhourCounter`；由地点 ID 随局保存。环境声在本 Mod 资源完成后用 `RegisterAfterhourAmbience` 注册。
- `AddAfterhourEvent` 可定义天数、随机日概率、固定 weekdays、地点关闭及 effects；`AfterhourEffect` 的修正键须由实际机制读取，纯文字不会产生效果。多个事件的同名修正相加；开始已有事件会延长。
- 原版 `injury_chance/junk_chance/inventor_sell_price` 等是相对变化；`loot_double_chance` 是百分点增量，`injury_immunity` 是免伤开关，`scav_attempts` 是次数，`commissary_restock_percent_of_max` 按最大库存比例补货。不要把这些键统一当百分数。
- `OnAfterhourArrive/Leave*` 和 `OnAfterhourEventStarted/Ended` 可能来自任何地点，按 ID 过滤。游戏内事件诊断命令为 `afterhour-events/afterhour-event-start/afterhour-event-end`。

## 鉴定类型

对应官方[鉴定物品章节](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#鉴定物品)。`AddAppraisalItem` 需要 `itemId/unlockRank/createRandom/getCorrectValue`，可提供 `isType/displayName/guideItemId`；目录工厂仍单独注册。

随机工厂用 `AppraisalHelper.InitAppraisalItem` 标记物品，并把决定正确价值的随机状态写入物品标签；`getCorrectValue` 根据该状态计算，不能每次估价重新随机。默认类型识别比较 itemId，变体另给 isType。`RANK_JUNIOR/APPRAISER/SENIOR/MASTER` 为 0/1/2/3。

`guideItemId` 在对应晋升发放，初级类型不经晋升解锁，需另安排指南来源；已拥有指南不会再发。注册原版 `hand/ribwich/plasma_fuel` 会替换其定义，移除 Mod 后须重启才恢复原版定义。鉴定类型与可检视物品是不同系统，不据此宣称有后者的注册 API。
