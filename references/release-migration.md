# Demo → 正式版迁移参考

以下保留 2026-10-28 整理的接口约束与迁移建议，目标是正式版官方加载器；当时的静态证据见 [检查基线](release-evidence.md)。

项目当前实现与验收边界以 `D:\workzone\probably-stolen\docs\CONTENT_MIGRATION.md`、各 `mods-official/<目录>/README.md` 和实际源码为准。正式版已使用根目录 `Build-Mods.ps1`；新增功能应扩展现有项目，先核对当前包结构与 Forge 服务接口。本页的阶段划分和依赖方案是迁移时的参考，不作为最新进度。

## 总体选择

旧组合是 Demo IL2CPP、net6、MelonLoader、Il2CppInterop 与 Forge。正式版是 Unity Mono、官方 `IMod` 和 Managed 程序集。不能只换 GameDir、去掉命名空间或复制原 DLL；必须重新编译并重新核对目标方法。

建议保留可复用玩法规则、稳定 ID、资源与存档 schema，将加载和游戏集成层换成官方接口。Forge 按职责削减：官方已覆盖的加载、配置、资源、钩子、网络升级、开局与周目数据优先接官方；机器批次事务、液体／电量结算、物流协议、内容所有权和必要存档迁移仍逐项评估保留。

## 最先解决的包依赖问题

**不能假定“独立 Forge Mod + 在其他 manifest 填 Prerequisites”就能工作。**

当前 `ModScanner.Pass.Run` 逐一检查程序集引用，非游戏引用必须出现在当前 manifest 的 `Assemblies` 名单。`CheckEnableCore` 和 `IsWellFormed` 又要求名单中的文件实际存在；`Prerequisites` 不参与该 allowlist。目录加载期间已经执行 `Init/OnEnable`，保存顺序稍后才应用。以上使旧版共享 Forge 包与内容包的静态引用模式存在直接阻碍。

依赖 DLL 文件名还需匹配程序集简单名：扫描器和 resolver 用文件名去扩展名处理依赖。旧版将版本号放进每个 DLL 文件名的发布习惯不宜沿用；包版本在 manifest／压缩包名表达。

优先做小型验证后再确定结构，候选方案：

1. **能独立使用官方 API 的简单 Mod，解除 Forge 依赖。** 最少的维护负担，也适合最早试点。
2. **强耦合机核系列先考虑单包组合。** 一个 `IMod` 入口编排必要 Forge 库和内容模块；库不再各带 `IMod` 入口，所有 DLL 在同一 manifest 列出且实际随包提供。一个公共所有者适用于包级扫描，内部仍显式保留内容原 owner 和存档身份。加载器不会自动 PatchAll 无 IMod 的库；这些库的必要补丁由入口显式安装／撤销，或编入入口程序集。这是设计候选，尚无加载验收。
3. **多个独立 Mod 继续共享框架的方案留待实证。** 与开发者确认官方对共享依赖的支持，或改为适当的纯源码／包内组件。不要给每个包塞同名不同版本 Forge DLL 后假定 Mono 会隔离；`Assembly.LoadFrom` 的程序集身份与多个全局静态状态需要专门验证。也不要改名程序集就自动认为状态可以共享。

不通过修改／绕过加载器扫描规则解决框架架构。此前一个包多个入口 DLL 的“合集”也不要照搬：同 manifest 会产生多个 LoadedMod，共享 Harmony owner，失败清理与资源归属需要重新设计。

## 分阶段推进

| 阶段 | 交付目标 | 验证／进入下一阶段的条件 |
| --- | --- | --- |
| A：构建基线 | 新建 `mods-official` 或独立正式版工程，隔离旧 `Directory.Build.props/targets`；引用 Managed | 最小 IMod 编译；无 Melon/Il2Cpp 引用；不自动部署 |
| B：最小试点 | 一个不改存档的小功能，例如 Speed Up 的单一倍率功能；官方配置和事件 | 编译后，经明确测试请求，普通激活、重启、事件与效果通过 |
| C：Forge 最小能力 | 单件物品注册、来源、图标、一段周目数据，以及必要的包内依赖 | 实际生成、新局／读档、保存后新进程重载；先确认包边界和 scanner |
| D：机器内容 | 选一组简单物品／配方，再迁食品加工、模组矩阵、合成扩展 | 交易／材料／液体／电量守恒与已有配置、资源相符 |
| E：自动化物流 | 正式版机器物流协议、网络、UID、库存和电量生命周期 | 多机器联动、失败路径、过夜、重载与断线重连状态正确 |
| F：复杂开局／剧情 | Definitely Stolen、机械飞升等剧情与奖励状态 | 新局身份、读档、一次性奖励、周目分叉和存档 schema 验证 |
| G：旧档与分发 | 旧档转换策略、正式版包／Workshop 与下载站元数据 | 转换至独立槽验证；旧源档保留；发布渠道明确区分 Demo 与正式版 |

阶段顺序是依赖建议；不表示现在执行安装、原生测试或发布。官方模板的编译仅证明最小入口可构建。

## 代码层映射

| 旧层 | 正式版方向 |
| --- | --- |
| `MelonMod` 与初始化／退出回调 | `IMod`；官方事件；必要时自行管理 MonoBehaviour |
| `Il2CppSystem.Collections`／Il2Cpp 对象转换 | 普通托管集合和真实游戏类型；按签名修改，不做盲目文本替换 |
| Il2Cpp detour、原生地址与偏移 | 删除旧实现；优先官方钩子，否则重新追踪托管方法 |
| MelonPreferences 与 Mod Manager 设置页 | 官方 `ModConfig` 和现有选项 API，显式迁移有效旧值 |
| Forge 物品工厂／注册 | 官方目录事件可承接基础注册；高级内容逻辑按需保留 |
| 自制资源发现 | 优先官方资源目录；已有嵌入图像也可暂留，核对 scanner 与名称 |
| Forge 网络升级／开局 | 对照官方 `AddNetworkUpgrade`、`AddStartingScenario/Perk`，保留逻辑身份 |
| 周目字符串数据 | 接 `PlayerStore.modData`／官方 helpers，明确原 key 与新前缀映射 |
| System.Text.Json 与 source-generated context | 先确认 netstandard2.1 依赖闭包及 scanner，再决定保留或转游戏内 Newtonsoft；字段／枚举／null／字典格式必须核对 |
| 旧混淆发布脚本 | 新正式版打包入口，验证最终扫描；不沿用 Il2Cpp 排除规则当验收 |

不要依照旧技能的 `ForgeNativeApi` 名称规划重构。当前 Forge 0.6.53 源码有 `ForgeItemApi`、`ForgeEffectApi`、`ForgeMachineRegistrationApi`、`ForgeMachineRuntimeApi`、`ForgeMachineAutomationApi`、`ForgeExternalPowerApi`、`ForgeRunDataApi` 等；逐项核对真实公共契约。

## 存档是独立迁移工程

- 游戏正式版是否直接读 Demo 存档、其兼容窗口如何，本次未实测。不能以两版都出现 `PlayerStore.modData` 就承诺可互通。
- 先保留原物品 ID、owner、配方 ID、开局 ID、UID 和 run/slot 语义。正式版原生 ID 冲突需明确映射，不能静默覆盖。
- 旧 Forge 直接写字典键（如物流 `Content.Owner + ".runtime_v5"`）；官方 `SetModData(owner,key)` 会写成 `owner:key`。需要在正确周目读取旧键，校验 schema／身份，再写新键并标记转换，不能把原键原样传入 helper 后多加一层前缀。
- 机器、库存、物流边和电量属于同一业务状态；转换要保持关联，不能只迁总量或显示字段。旧 `System.Text.Json` 与新序列化库的输出差异需要样本回读。
- `Stage`／`SetModData` 不保证写盘；先验证在游戏实际保存时序之前提交，再验证保存后的新进程读取。失败时不能写“迁移已完成”。
- 旧 Definitely Stolen／机械飞升的奖励收据、schema、run fork 规则保留，避免重复奖励或把另存新槽误判成新周目。
- Forge 的缺失物品恢复只适用于可确认的提供者移除，不能把正式版接口失配误判成卸载并退款／删除物品。
- 真正执行不可逆存档转换时才制定必要备份和回退，使用独立槽／可丢弃样本；本次不复制或修改玩家存档。

## 独立处理的工具 Mod

**Mod Manager / Bootstrap**：原加载顺序和 Melon 程序集扫描接管逻辑停止迁移；正式版已有启用、顺序展示、配置、扫描、信任与工坊。下载、版本检查、社区统计等剩余产品需求分别评估，并说明其网络／文件／进程能力。不再以官方加载器补丁重建旧 Bootstrap。

**Auto Backup**：先核对正式版 `SaveFiles` 槽位、历史轮换、索引、云存档与保存成功条件。正式版 `PlayerStore.SaveGame` 已有历史轮换／恢复调用，先判断剩余需求，不因代码短而作为首批试点。

**分发与网站**：新包改为目录 + manifest，现有“DLL 直接放 Mods 根目录”说明需要另任务更新。Nexus、QQ、腾讯文档、Mod Manager 详情库和官网应明确记录游戏分支／加载器／Forge 版本，不能覆盖旧文件制造兼容错觉。本次不发送消息或更新线上记录。

## 旧技能处理

原仓库根 `SKILL.md` 已由正式版技能替换，采用 `$probably-stolen-official-mods` 新名称接入。本机旧名称没有已安装目录可卸载；不另保留第二个活跃开发入口。无入口引用的旧场景参考已清理，研究方法与报告继续供查证。其他独立用途技能、旧 Mod 源码、旧发布包、玩家内容和 Demo Forge 仓库仍各自维护。
