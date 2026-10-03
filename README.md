# Probably Stolen Mod Skill

供 Codex 设计、评审、开发和排查 **Probably Stolen** 内容 Mod 使用的 Skill。它整理游戏接口、加载时机、Harmony 补丁、内容注册、界面、存档与兼容性检查，适用于 MelonLoader 插件、基于 Nicokobo Forge 的内容 Mod 和游戏内置 ModLoader 内容包。

这是开发指导，不是可以放进游戏 `Mods` 目录直接运行的 Mod。具体类型签名和行为须以当前游戏构建、框架契约与安装产物为准；接口检查、编译、安装、加载、玩法和保存重载分别记录。百科内容写作使用 Wiki Skill，框架本身的改造使用 `nicokobo-forge`。

## 仓库内容

- [SKILL.md](SKILL.md)：Skill 入口，说明适用范围、开发与验证流程，并按需求指向场景指南。
- [scenarios/](scenarios/)：按任务查阅；除物品、机器、开局、UI、事件与存档等场景，也包含 [Forge 接入](scenarios/forge-integration.md)、[工坊进度](scenarios/workshop-progression.md) 和 [构建与生效排查](scenarios/build-and-verification.md)。只读取本次需要的指南。

一般开发执行适当的离线检查；场景中的原生验收项不自动启动游戏。用户明确要求执行原生测试时，才读取并使用当前环境的 `probably-stolen-native-test` 技能；仅讨论测试或整理技能不触发。规则见 [原生测试触发规则](scenarios/build-and-verification.md#原生测试触发规则)。

## 接入 Codex

将包含 `SKILL.md` 的目录放入 `$CODEX_HOME/skills/probably-stolen-mod-skill`；未设置 `CODEX_HOME` 时，默认位置为用户目录下的 `.codex/skills/probably-stolen-mod-skill`。请保留 `SKILL.md` 与 `scenarios/` 的相对位置。

在 **Windows PowerShell** 中，从本仓库根目录运行以下命令可建立目录联接。这样修改仓库文件后无需再次复制：

```powershell
$skillsDir = if ($env:CODEX_HOME) { Join-Path $env:CODEX_HOME 'skills' } else { Join-Path $env:USERPROFILE '.codex\skills' }
New-Item -ItemType Directory -Force -Path $skillsDir | Out-Null
New-Item -ItemType Junction -Path (Join-Path $skillsDir 'probably-stolen-mod-skill') -Target (Get-Location).Path
```

如果目标路径已有同名目录，先检查现有接入方式，再更新它；上面的命令不会覆盖现有目录。不使用目录联接时，也可以在目标目录中复制 `SKILL.md` 和整个 `scenarios/`，仓库更新后需重新同步。

接入后开启一个新的 Codex 任务，明确调用，例如：

```text
$probably-stolen-mod-skill 请基于当前游戏版本，为这个 MelonLoader Mod 增加一种可从拾荒获得的新物品，并检查存档与其他 Mod 的兼容性。
```

也可以直接描述 Probably Stolen Mod 开发需求，让 Codex 根据 Skill 描述选择它。提供游戏版本、加载方式、Mod 项目路径和期望行为，有助于核对实际接口。

常用请求也包括“结合现有接口评审这份 Mod 方案”“给已有 Forge 机器追加配方并同步投料准入”“检查改过的数值为什么没有生效”。方案评审、开发、安装与原生测试按各自请求范围执行。
