# 场景：接入 Nicokobo Forge

适用：内容 Mod 使用 Forge 注册物品、效果、机器、工坊或周目数据，或把重复的游戏接入迁到已有公共 API。只在项目已经依赖 Forge 或本次需求需要它时使用，不为普通工具插件强加前置。

## 确认当前契约

先定位实际框架仓库和调用项目，读取框架的 `README.md`、`docs/SCOPE.md`、`docs/API_BOUNDARIES.md` 与相关 API 声明；能力进度再查 `docs/FORGE_PROGRESS.md`。名称和能力可能随版本变化，下表只是定位入口，每次仍核对当前签名、快照与调用者。

| 需求 | 优先核对 |
| --- | --- |
| 物品、节点、设施、模组及效果 | `ForgeApi`、`ForgeItemApi`、`ForgeEffectApi`、`ForgeContentApi`；owner、稳定 ID、工厂、随机资格及逐项应用状态 |
| 读档、过夜和日结 | `ForgeLifecycleApi`；阶段、执行顺序、订阅身份与释放；事件上下文和原生句柄不跨事件缓存 |
| 模组准入、属性和成长 | `ForgeModuleApi`；实际机器类型、原生重算顺序、成功工作证据 |
| 自定义机器与配方 | `ForgeMachineRegistrationApi`、`ForgeMachineRuntimeApi`；模板、实时槽位和批次事务，见 [机器](machines-and-recipes.md) |
| 电量、液体及生产价值 | `ForgePowerApi`、`ForgeLiquidApi`、`ForgeProductionValueApi`；实际单位、当前机器电耗、逐批价值与读回 |
| 名称、效果说明及来源行 | `ForgePresentationApi`；仅作用于所属且已应用的物品，见 [物品](custom-items.md) |
| 工坊独立进度链 | `ForgeWorkshopApi.RegisterChain`；展示快照与执行回调分开，见 [工坊](workshop-progression.md) |
| 开局身份和周目数据 | `ForgeStartApi`、`ForgeRunDataApi`；身份认领、内存暂存与实际保存分开，见 [开局](custom-starts.md)、[存档](save-state.md) |
| 已有周目另存新槽 | `ForgeRunForkApi.RegisterState / IsForking`；纯身份转换、会话重绑与保存回读，见 [存档](save-state.md) |
| 库存观察与放置预检 | `ForgeInventoryApi`、`ForgeInventoryPlacementApi`；快照和预检不预留、不提交，见 [库存](inventory-and-trade.md) |
| 单件搬运与倒液 | `ForgeItemTransferApi.Move`、`ForgeLiquidTransferApi.Pour`；实际转移量、不确定结果和各自适配条件 |
| 隐藏库存与完整物品图 | `ForgeInventoryEndpointApi`、`ForgeSaveGraphApi`、`ForgeListInventoryApi`；固定拓扑、完整特征和真实容量 |
| 准入、临时锁与业务事务 | `ForgeInventoryAdmissionApi`、`ForgeInventoryFreezeApi`、`ForgeRunPurchaseApi`、`IForgeBatchTransaction`；各自限定的所有权与补偿范围 |
| 多源供电与耗电设备 | `ForgeExternalPowerApi`、`ForgePowerApi`、`IForgePowerTransaction`；真实收据、拒绝与补偿，见 [机器](machines-and-recipes.md) |
| 配方手册与自动化目录 | `GuideSnapshot`、`ForgeRecipeGuideApi`、`ForgeMachineAutomationApi`；只读声明，不增加加工次数 |
| 对话、来访、电话与供应商 | `ForgeDialogueApi`、`ForgeClientVisitApi`、`ForgePhoneApi`、`ForgeSupplierApi`；见 [角色](custom-characters.md) |
| 图像、阅读、物品检查与存档回读 | `ForgeAssetsApi`、`ForgeReadingApi`、`ForgeItemInspectionApi`、`ForgeSaveReadbackApi`；按本次用途读对应契约 |

## 分工与注册时机

内容 Mod 持有物品定义、图像、数值、配方、解锁条件、奖励与存档格式；Forge 持有通用注册、能力门控、原生适配、事件分派和已有资源事务。通过公共 API 接入，不复制内部注册器或为了取旧私有成员而加反射兼容层。确需新能力时在框架中实现通用契约，再更新调用者；不要把单个 Mod 的玩法塞进框架。

原生内容声明在目录初始化前提交，通常放在项目约定的早期注册阶段；工厂在目录应用或实际创建时才触碰所需原生对象。`Accepted` 只表示声明暂存，逐项 `Applied` 才表示目录接入；两者都不能证明工厂产物与玩法有效。不要把直接写原生目录的“初始化后追加”照搬到 Forge 注册，也不要假定迟到注册立即生效。

追加配方在目标机器声明已存在后，使用 `RegisterAdditionalRecipes`；按加载顺序选择一次性的合法回调，例如项目已采用的 `OnLateInitializeMelon`。缺目标时报告具体依赖和注册状态，不改成逐帧搜索。注册后不修改调用者持有的列表来更新运行时配置，应通过当前支持的公共更新路径发布。

订阅回调保留可释放句柄或原委托，ID 归属正确 owner；初始化失败或功能退出时只释放自己的订阅。共享事件中的库存列表可能只捕获一次，但其中对象仍可变化，提交交易前重新校验实时状态。

## 能力与验收边界

动作前检查对应的 `ForgeCapabilities.Current`、目录快照和具体 API 的可用状态，只停用依赖失败能力的功能。优先按所用 API 查询实际门控，不假设总能力等同于每个窄接口。核对实际安装产物，不能从开发目录的新 API 推断游戏已具备该能力；接口表只是查找入口，不承诺所有构建均可用。

`InventoryTransfer` 总能力仍可能为 false，但当前有独立的单件 `Move` 和倒液 `Pour` 契约。按具体接口核对数量、`Indeterminate` 与恢复状态；不能将它们串起来宣称跨库存整体原子事务。确实只有观察/预检能力的环境才停在计划阶段。

`ForgeRunDataApi.Stage` 只比较后暂存所属 JSON，不是保存或跨物品事务；`ForgeStartApi` 认领身份也不生成完整开局。工坊条件和奖励声明只供展示，内容方负责交易及收据。涉及运行中业务状态参与机器批次时使用现有事务接入，不把先 `Stage`、后 `Move` 当作共同提交。

检查覆盖 owner/ID 冲突、注册时机、逐项应用和依赖缺失；有原生执行请求时再检查实例创建、玩法资源变化与重载。框架公共 API 变更按 [统一构建](build-and-verification.md) 更新所有调用者。
