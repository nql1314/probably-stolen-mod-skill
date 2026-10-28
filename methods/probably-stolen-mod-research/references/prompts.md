# 分阶段任务提示词

这些提示词用于用户手动选择模型、新建会话后粘贴。它们不是已经创建的任务。先将 `<…>` 替换为实际值。模型不支持 `$技能名` 时，直接要求读取完整文件路径即可，不依赖自动发现或旧会话。

默认方法入口：
`D:\workzone\probably-stolen-mod-skill\methods\probably-stolen-mod-research\SKILL.md`

默认批次目录可选：
`D:\workzone\probably-stolen-mod-skill\packages\mod-research\<批次名>`

## A：低成本模型盘点

```text
读取 D:\workzone\probably-stolen-mod-skill\methods\probably-stolen-mod-research\SKILL.md，执行 A 阶段。
输入：D:\workzone\probably-stolen\mods
知识库：D:\workzone\probably-stolen-mod-skill
批次目录：<完整批次路径>

只盘点和编排，不开展逐 Mod 深度分析、不改开发 Skill、不运行游戏。
检查归档内部组件，将发布材料与唯一分析单元分别列账；处理合集、多个版本、随包依赖、纯资源包和无法判定身份。
覆盖所有输入材料，明确非 Mod 排除项；不要用文件数代替 Mod 数。
生成 run.md、inventory.md 与可独立执行的 tasks 任务单，选出 3–5 个不同类型的试点对象。
交付范围、数量口径、试点任务路径和下一阶段入口；不启动其他模型。
```

## B：低成本模型完成一份任务单

```text
读取 D:\workzone\probably-stolen-mod-skill\methods\probably-stolen-mod-research\SKILL.md，执行 B 阶段。
任务单：<完整任务单路径>
批次目录：<完整批次路径>

只分析任务单指定的单元，按 references/analysis.md 保存报告和必要证据。
沿核心功能的入口、状态变化、副作用、失败和保存/清理路径追踪，不只概括类名。
每个实现结论都给可回查的位置；分开写直接事实、文档声明、推断和未知。
不要把源项目较新实现归给旧发布包，不要把框架能力归给调用者。
不改全局清单、成品 Skill 或 Mod，不运行包内程序，不安装或原生测试。
遇到混淆或工具不足时保留已完成部分与精确补证问题，不能虚构内部机制。
最后返回单元 ID、状态、报告位置、候选主题与未决项，供协调者更新清单。
```

## C0：高级模型复核试点

```text
读取 D:\workzone\probably-stolen-mod-skill\methods\probably-stolen-mod-research\SKILL.md，执行试点复核。
批次目录：<完整批次路径>
试点报告：<完整路径列表>

回查决定性证据，检查报告能否支持后续模式比较，而不是只看文档是否完整。
标出缺少的行为链、推断冒充事实、版本错配、重复来源及无法定位的结论。
通过的报告记复核结果，问题报告给出精确补证任务。
如果模板确实缺少必要字段，提出具体修订建议；不要擅自改研究方法或扩展研究范围。
交付是否适合继续全量、需要补证的对象及下一批建议，不改成品 Skill、不派发任务。
```

## C：高级模型按主题归纳

```text
读取 D:\workzone\probably-stolen-mod-skill\methods\probably-stolen-mod-research\SKILL.md，执行 C 阶段。
批次目录：<完整批次路径>
主题：<例如存档与状态生命周期>
指定输入报告/索引：<路径列表>

读取相关报告，按 references/synthesis.md 回查拟推荐结论的关键证据。
比较同类问题的不同方案，保留适用前提、代价、反例和来源相关性，不按名称套设计模式。
生成主题比较、模式卡和 optimization-backlog.md 中本主题的优化候选；维护此主题索引及相应复核结果。
报告说不清的地方退回精确问题，不能凭常识补成观察事实。
本轮只归纳，不改成品开发 Skill、不改 Mod、不执行原生测试、不自动派发任务。
最后说明哪些模式可入库、哪些待补证、哪些无需采纳，列出下一阶段所需文件。
```

## D：高级模型正式入库

```text
读取 D:\workzone\probably-stolen-mod-skill\methods\probably-stolen-mod-research\SKILL.md，执行 D 阶段。
批次目录：<完整批次路径>
允许入库的模式卡：<明确文件列表或已审核主题索引>
知识库：D:\workzone\probably-stolen-mod-skill

将已审且可入库的结论合并到开发 Skill 的相关场景，必要时调整根 SKILL.md 路由。
保留工作区已有修改和删除，不以旧内容自动恢复文件；记录与本次入库冲突的目标并继续可独立完成的部分。
成品按场景给出条件、步骤、代价和验证路径，去掉原始 Mod 身份与调查材料路径。
在研究目录维护 integration-map.md，保留结论到证据的映射。
检查本次链接与内容、运行可用的格式校验，区分既有缺陷与本次改动。
不修改 Mod、不编译游戏项目、不打包安装、不运行原生测试。
交付修改文件、已入库模式、未入库理由和实际检查结果。
```

## E：协调者清点与跨会话交接

```text
读取 D:\workzone\probably-stolen-mod-skill\methods\probably-stolen-mod-research\SKILL.md，执行 E 阶段。
批次目录：<完整批次路径>
本次返回的报告/复核/入库结果：<路径列表>

根据文件中实际结果更新 inventory.md，核对输入材料和分析单元各自的数量关系。
核对无报告、部分、阻塞、退回及未处理的入库项，不把有文件等同于完成。
生成或更新 handoff.md：当前进度、待核实问题、原有工作区改动、下一任务和精确输入输出路径。
若研究档案位于被 Git 忽略的 packages，明确跨机器交接必须另外提供它。
只整理交接，不补写不存在的结论、不启动下一批模型、不恢复或删除用户文件。
```
