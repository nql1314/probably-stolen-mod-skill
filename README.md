# Probably Stolen Mod Skill

供 Codex 开发、维护和排查 **Probably Stolen** Mod 使用的 Skill。它整理游戏接口、加载时机、Harmony 补丁、内容注册、界面、存档与兼容性检查的工作方法，适用于 MelonLoader 插件和游戏内置 ModLoader 内容包。

这是开发指导，不是可以放进游戏 `Mods` 目录直接运行的 Mod。具体类型签名和行为须以你正在使用的游戏构建为准；离线 dump、编译通过和游戏内生效是不同的验证阶段。百科内容写作请使用单独的 Wiki Skill。

## 仓库内容

- [SKILL.md](SKILL.md)：Skill 入口，说明适用范围、开发与验证流程，并按需求指向场景指南。
- [scenarios/](scenarios/)：按任务划分的指南，涵盖物品、掉落、角色、开局、UI、交易、机器、事件、商店进度、声音、存档等场景。它们提供接口线索和检查项，不是可直接套用的源码。
- `packages/`：现有 Mod 与分析资料，可用于查找实现和兼容性线索；接入 Codex Skill 时不要求复制此目录。

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
