# Probably Stolen 正式版 Mod 技能

当前技能为 `$probably-stolen-official-mods`，替换原 `$probably-stolen-mod-skill` 开发入口，面向正式版 Unity Mono 与内置 ModLoader。

- [技能入口](SKILL.md)
- [官方开发流程](references/official-workflow.md)
- [官方中文指南整理：资源、钩子与配置](references/official-resources-and-hooks.md)
- [官方中文指南整理：内容注册 API](references/official-content-apis.md)
- [官方中文指南整理：UI、纸质手册与机器窗口](references/official-ui-and-books.md)
- [Steam 工坊首次发布与更新](references/official-workshop.md)
- [现有 Mod 与 Forge 迁移方案](references/release-migration.md)
- [版本、证据与待验证项](references/release-evidence.md)
- [可编译最小工程](assets/official-starter/OfficialStarter.csproj)

本机通过 `C:\Users\49479\.codex\skills\probably-stolen-official-mods` 目录联接到此仓库。新对话可显式调用：

```text
$probably-stolen-official-mods 请按正式版官方接口，为这个 Mod 制定迁移计划并完成相关编译。
```

旧开发入口及其无入口引用的七份场景参考已移除。`methods` 研究方法和 `packages` 分析数据继续保留；资料中的游戏分支、API 和验证结论以各自注明的版本为准。原生测试、Wiki、工坊资料同步、QQ 目录同步和 Forge 旧版维护使用各自独立技能。

普通编译不安装到游戏。模板产物只进入自身 `bin/Release/netstandard2.1`；创建内容、安装、测试、打包和发布按任务范围分别执行。
