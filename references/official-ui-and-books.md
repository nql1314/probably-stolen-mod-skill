# 正式版 UI、纸质手册与机器窗口

依据：[官方中文指南](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md)。按当前任务选择一种入口；这里描述文档契约，具体签名和运行效果仍须核对目标构建。

## 窗口选择

| 需求 | 官方入口 |
| --- | --- |
| Mod 自身参数 | `ModConfig.Define` 的原生配置面板，见[资源与钩子](official-resources-and-hooks.md) |
| 普通运行时面板／列表 | `CustomUIManager.Window` / `CustomUIBuilder` |
| 指南、名片、可阅读文档 | `Books/*.json` / `PaperUI` / `CreatePaper` |
| 机器、容器、工作台内容 | `MachineHelper.CreateMachineryWindow` |
| 属于游戏选项的设置／热键 | `AddGameOption*` |

## 自定义窗口

对应官方[自定义 UI 章节](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#自定义-ui)。不需要自建 Unity 工程或预制件；先确认 `CustomUIManager.Instance` 存在，使用 Mod 精灵图时等资源就绪。

- `Window(id, title, layer)` 的 ID 自行加 modId 前缀；同 ID 会替换现有窗口。`Show()` 返回可见的 `CustomUIWindow`，`GetWindow()` 可取隐藏窗口；`Close()` 销毁并注销，`Hide()` 仅隐藏。
- 场景层为 `LAYER_PAPER/FRONT/REAR/META`，主菜单用 `LAYER_MENU`，跨场景覆盖层为 `LAYER_OVERLAY`。场景窗口会随场景销毁，需要时在载入钩子重建；不存在的层会警告并回退覆盖层。
- 用 `BeginRow/Column/Panel/Grid/Scroll/ScrollFlexible` 与 `End()` 成对布局；标签页为 `BeginTabs/BeginTab/End/EndTabs`。控件可设置 id，后续通过 `window.Get(id)` 更新，不重建整个窗口。
- `AddInput` 的 `onChanged` 在提交／失焦触发，逐字搜索用 `onTyping`。保留输入框，只用 `CurrentContainer`、`ClearContainer` 和该容器上的构建器重建结果区域，避免丢失焦点。
- `SetBool` 等 setter 默认静默，确要触发回调时用 `notify:true`；`AddImage/SetSprite` 接原版键或 `modId:spriteName`。外观可用 `UseMetaPrefabs/UseMachineryStyle`。
- `SetCloseOnEscape` 可能与游戏全局 Escape 处理同时运行，不能仅凭编译断言输入无冲突。关闭／禁用时只清理自己的窗口。

## 纸质手册

对应官方[纸质手册章节](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#纸质手册)。优先把布局放 `Books/<bookId>.json`，行为放 C#；即使只有书籍资源也仍需要 manifest 和 `IMod` DLL。

编辑器入口为载入商店后 F9 > Special > Modding Panel > Paper Editor。它会自动写入本地 Mod 的 Books 文件，运行中重载 JSON；执行编辑／游戏操作需在用户任务范围内。做成品前用 Check 查看溢出、重复 ID 和缺图，截图遵循项目图片预算；不要仅为读指南而启动编辑器。

| 数据 | 主要字段与约束 |
| --- | --- |
| 手册 | `id/title/background/width/height/designWidth/designHeight/chrome/templates/pages/item` |
| 页面 | `name/template/slots/elements`，chrome 为共享外框 |
| 元素 | `type=text/image/button/flow/line`，`id/x/y/w/h/visible/raycast`；需要运行时更新的元素设稳定 id |
| 文字 | `text/loc/translations/style/fontSize/wrap/autoHeight/preview`；loc 优先于 text，preview 仅供编辑器 |
| 图片／按钮 | `sprite/fit/scale`、`action/hoverSprite`；`close/zoom/flip_left/flip_right` 为内置动作 |
| 文档物品 | `id/name/nameLoc/description/descriptionLoc/sprite/value/important`；阅读时打开手册 |

布局坐标默认基于背景原图，左上为原点、y 向下；`width/height` 是显示尺寸，`designWidth/designHeight` 是坐标空间。默认图常以两倍显示，不能把显示像素直接填入设计坐标。拉伸背景的 `border` 顺序是 `[左,下,右,上]`。

美术放 `Textures/Paper`，本 Mod 可用文件名键，跨 Mod 用 `modId:key`；原版物品用 `atlas/sprite`，深色选项外框用 `meta_...`。F9 的 Paper Sprite Picker 可查现有纸张美术。文字样式如 `body/title/caption/page_number/meta_body` 会按语言选择字体，优先沿用。

动作在每次构建／重建时绑定：`ModHook.OnPaperOpened(fullId, ui)` 中筛选 `modId:bookId`，再用 `ui.OnAction/OnPageShown/Get/SetText/SetSprite`；也可用 `ModHelper.OnPaperOpened(modId, bookId, callback)` 定向订阅，清理方式按当前签名核对，不臆造退订 API。不要只在启动时保存一次旧 UI 句柄。

操作入口为 `OpenPaper/ClosePaper/IsPaperOpen/GetOpenPaper/SpawnPaperItem/GetPaperBookIds/RemovePaperBooks`。动态页面用 `CreatePaper(...).AddPage(...).Show()`，自建数据可 `ParsePaperBook/RegisterPaperBook`。控制台可查 `paper-list/open/close/reload/edit/item`；实际命令名均带 `paper-` 前缀。

## 机器窗口与存档

对应官方[机器窗口章节](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#机器窗口)。用 `CreateMachineryWindow` 组合 `PowerPanel/NotePanel/IconPanel/Panel`，再 `item.SetContentWindow(window)`；电池槽用 `SetupBatterySlot` 绑定。

控件包括 `MachineryButton/RadioButton/RadioGroup/RadioList/ProgressBar/Slider/RangeSlider/Tabs/Text`，布局用 `MachineryStack.Vertical/Horizontal`。读取存储用 `GetBodyStorage(item,i)` 或面板 role；批次加工、物流和结算仍由 Mod 自己实现，窗口不会自动提供机器业务。

**面板在定义数组中的顺序同时决定库存存档顺序。** 已发布后保留既有下标，新增面板追加；只想改变视觉排列时传 `displayOrder`，不重排持久化数组。否则旧档物品可能进入错误库存。

## 游戏选项与热键

对应官方[游戏选项章节](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#游戏选项)。`AddGameOptionSection` 新建分区，`AddGameOptionToggle/Slider/Dropdown/KeyBind` 注册条目，关闭时 `RemoveGameOptions(modId)`；单项可 `RemoveGameOption`。Mod 自身参数优先使用 ModConfig，不必都加入游戏总选项。

标签、提示和选项列表用函数求值以跟随语言；getter 在打开窗口读取，setter 仅在“应用”提交，值的持久化由 Mod 决定。内部 ID 为 `modId:optionId`，重复注册会替换；必要时 `GameOptionId` 取得完整 ID。

按键捕获只接受键盘，Escape 取消并拒绝已占用的绑定。自行轮询热键时先检查 `KeyBindHelper.SuppressHotkeys`，防止改键过程触发功能；注册键位本身不会添加 `IMod.Update` 回调。
