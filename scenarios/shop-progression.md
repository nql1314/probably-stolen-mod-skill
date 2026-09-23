# 场景：网络升级与商店服务

适用：调整 Wilds Network 升级的前置、费用、重复购买与冷却，或让商店服务解锁、启用、改变费用。先分清“展示为可购买”“完成付款”“升级生效”和“读档后仍生效”，这些可能属于不同调用链。

## 核对当前对象

- `NetworkUpgrade` 提供 `GetUpgradeById`、`IsReady`、`IsHavePrerequisite`、`GetMissingPrerequisite`、`GetCost`、`IsUnlocked`、`Unlock` 与 `IsLockedInDemo`。对象还有 `prerequisite`、`isRepeatable`、`cooldownCurrent` / `cooldownDuration`、`state` 等持久字段。
- `StoreService.FindServiceByID`、`IsActive`、`IsOnCooldown`、`UpdateCost` 可用于定位和观察服务。其 `unlocked`、`introduced`、`isUsing`、冷却与费用是不同状态；当前构建还有 `HealIntroducedAfterLoad`，其实际修复条件需沿读档调用链确认。
- 只读能力优先读取对象和原生判定；想改变购买资格、费用或生效结果时，追踪当前构建的购买入口、扣款和 `Unlock` 调用链。不要把 `Unlock()` 当作已完成付款的证明，也不要仅写 `state` / `unlocked` 就宣称升级或服务已完整生效。

## 实现路径

1. 修改既有升级时，按目标 ID 和实际玩家状态限定范围；优先改专门判定或数据生成点，保持其他升级的前置和 Demo 锁。需要跳过原流程时，逐项补齐扣款、冷却、UI、存档和原版解锁动作。
2. `NetworkUpgradeList.UnlockActions` 可能承载购买后的发物、排程、电话介绍、服务或场景变更。已有扩展在 `NetworkUpgrade.Unlock` 后只补缺失的动作；复刻这类逻辑前检查当前构建是否已有对应原生动作，避免双倍发放。
3. 调整服务时区分“已介绍”“已解锁”“正在使用”“处于冷却”。费用变化应走当前构建的实际计费路径，不能只修改展示字段。夜间扣费或服务生效可追踪 `ModHook.OnHandlingNightlyServicesEarly/Late` 与服务自身调用链。
4. 新增一条完全自定义升级或服务还需要目录、界面、价格、行为与持久化共同支持；仅有公开查询和状态字段不足以保证游戏原生 UI 会列出它。先做窄范围可用性验证，再决定是否补 UI 或私有注册表。

## 验证

用可丢弃存档检查未达前置、余额不足、一次购买、重复点击、跨日冷却和读档；逐项确认钱、升级/服务状态、赠品、角色排期与 UI 一致。与修改原有规则有关的补丁原则见 [规则示例](gameplay-rules.md)。
