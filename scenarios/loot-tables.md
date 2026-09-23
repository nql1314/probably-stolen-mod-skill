# 场景：扩充或调整掉落表

适用：让自定义物品进入拾荒、垃圾堆或奖励掉落；调整某张表的条目和权重；监听抽取结果。先找实际生成路径引用的表或组 ID，再决定修改表、组还是生成结果。

## 选择入口

| 需求与加载方式 | 候选入口 | 边界 |
| --- | --- | --- |
| 游戏内置 ModLoader 内容包 | `ModLootFileJson` 的 `tables`、`groups`、`patches`；条目为 `id`、`weight` | 先核对当前构建的文件发现规则和包结构；裸 MelonLoader DLL 不会因此自动加载 JSON |
| MelonLoader 插件运行时调整 | `LootRegistry.RegisterTable` / `AddEntry` / `RemoveEntry` / `SetWeight`，组对应 `RegisterGroup` / `AddGroupEntry` / `RemoveGroupEntry` / `SetGroupWeight` | 在表和物品目录可用后注册；确认是否会因场景重建重放或重复写入 |
| 观察抽取 | `ModHook.OnLootRolled(Action<LootRollContext>)` | 上下文有 `tableId`、`groupId`、`itemId`；修改 `itemId` 能否改变实际奖励需另行验证 |

当前构建还有 `ModHook.OnLootTablesLoaded` 可作为加载完成的候选时机，以及 `TableMaster.SpawnFromTable`、`TableGroupMaster.SpawnFromTableGroup` 作为实际生成路径。使用 `LootRegistry` 前核对当前托管程序集签名；不要按旧版已移除的 `TableMaster.lootTables` / `TableGroupMaster.tableGroups` 字段直接写入。

## 写入与冲突

1. 为自定义表、组和物品使用稳定且隔离的 ID。先确认条目 ID 能由物品目录解析，组条目指向现有表；物品创建与获得路径见 [物品示例](custom-items.md)。
2. 权重必须是有限、非负且符合游戏实际抽取语义的数。区分“权重”与“概率”，不要声称把权重设为 20 就等于 20%。
3. 扩充原版表优先定点 `AddEntry` 或 `SetWeight`；`ClearTable`、`ClearGroup` 和完整 `Register*` 会影响同表已有条目。与其他 Mod 改同一表时，实际操作顺序会改变最终分布，记录冲突和加载顺序。
4. 注册通常是加载阶段动作，避免在每次抽奖回调里改表。抽取回调只做轻量观测，统计或日志需限制频率；不要在热路径重建全部表。

## 验证

核对加载日志、表/组 ID、注册后的条目与权重，并走一次实际掉落路径。分别试验未装其他 Mod、共改同表、场景重载和缺失物品 ID；观察实际物品落点与保存结果。`dump.cs` 只能确认接口声明，无法证明加载顺序或掉落分布。
