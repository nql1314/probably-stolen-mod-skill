# 正式版 Steam 工坊发布

依据：[官方中文指南的 Steam 工坊章节](https://github.com/dragonpan2/probably-stolen-modding/blob/accceead2d7b63bc1746bcb053e6ab41bea75d2a/ModdingGuide.zh-CN.md#steam-创意工坊)与本地 `D:\workzone\probably-stolen\docs\STEAM_PUBLISHING.md`、`Publish-Steam.ps1`（2026-10-28 只读核对）。文档整理未执行真实登录或上传。

## 上传入口与范围

游戏没有上传按钮，Steam 个人工坊列表用于查看已发布作品。正式版通过 Valve SteamCMD 发布，AppID 为 `4348910`；Demo／playtest 不支持此工坊。上传的内容是一个根部有 manifest 的 Mod 目录，不是 ZIP。

本机仓库已有 `Publish-Steam.cmd`（双击交互选择）／`Publish-Steam.ps1`。有人询问“在哪上传”时先提供这个入口；要求发布某个 Mod 时再核对目标并执行。普通编译、打包、浏览指南和整理技能均不隐含上传授权。

执行前核对当前仓库说明和参数。需要 PowerShell 7、对应 .NET SDK、游戏 Managed 引用以及拥有游戏和目标条目权限的 Steam 账号；SteamCMD 默认放 `tools/steamcmd/steamcmd.exe`，也可在 PATH 或用 `-SteamCmdPath` 指定。密码和 Steam Guard 通过工具交互，不写入脚本或聊天。

## 使用现有发布脚本

在 `D:\workzone\probably-stolen` 的 PowerShell 7 执行，示例 Mod 名替换为 `mods-official/` 下的实际目录：

```powershell
# 只编译、打包并生成待上传 VDF
.\Publish-Steam.ps1 -Mod speed-up -PrepareOnly

# 首次创建；脚本默认隐藏
.\Publish-Steam.ps1 -Mod speed-up -NewItem -SteamUser YOUR_LOGIN

# 已有条目先绑定实际 ID，避免新建重复项
.\Publish-Steam.ps1 -Mod speed-up -PublishedFileId 1234567890 -SteamUser YOUR_LOGIN

# 后续更新会复用保存的条目 ID 与登录名
.\Publish-Steam.ps1 -Mod speed-up

# 任务要求公开时使用；本命令也会重新构建并上传
.\Publish-Steam.ps1 -Mod speed-up -Visibility Public
```

这些是用法示例，不代表现在执行。省略 `-Mod` 会交互选择；首次交互可输入已有条目 ID 或 `NEW`。发布脚本使用本次全新的构建目录，并以 `-NoInstall` 打包；不安装、不启动游戏。

发布前使 csproj、manifest 版本一致，填写 `change.log` 当前版本段落，或明确传 `-ChangeNote`。详情默认取 manifest，可用源码目录的 `workshop-description.txt` 覆盖；封面用该目录 `preview.png` 或 `-PreviewFile`，必须小于 1 MB。未提供封面的普通更新保留原封面。

## 条目身份与恢复

- `.steam-workshop/<manifest-ID>.vdf` 是持久发布身份，`account.json` 仅保存登录名。它们不是可随意删除的构建缓存；迁移工作区要保留或用实际 ID 重建绑定。
- 首次创建前写入 `.pending`。中断且 ID 仍为 0 时先核对自己的工坊条目，再以实际 `-PublishedFileId` 恢复；不删除标记盲目重试。
- 有非零 ID 后只更新该条目；脚本拒绝改绑。先确认目标条目属于此次 Mod，不能把示例 ID 当真实 ID。
- 普通更新省略 visibility，保留原可见性；首次上传隐藏，公开属于发布范围内单独需要核对的状态。用户已明确要求公开发布时按已有授权完成，不额外制造确认步骤。

## 没有本地脚本的工程

按官方文档配置 `workshopitem` VDF：`appid=4348910`、`publishedfileid=0`（新建）、`contentfolder`（包目录）、可选 `previewfile`、`title/description/changenote`。VDF 路径字符串需正确转义；`visibility` 数值为 0 公开、1 好友、2 隐藏。

```text
steamcmd +login <Steam登录名> +workshop_build_item "C:\path\to\item.vdf" +quit
```

SteamCMD 返回并写回条目 ID 后保留 VDF，后续用该 ID 更新。不要在已有发布脚本的仓库另造一套状态文件或绕过其新建保护。

## 核对结果

本地脚本同时检查退出码、有效条目 ID 和匹配的成功消息；上传确认后核对页面标题、版本说明、封面、协议和可见性。首次发布需在 Steam 网站接受工坊协议。将“上传确认”“已公开”“已订阅并下载”分别如实报告，不能用其中一个代替全部完成。

本地与工坊使用同类包，同 manifest ID 的本地包优先，可能遮住订阅版本；来源变化可能要求普通玩家重新批准。不要为验证订阅而删除用户本地包或修改信任。`Prerequisites` 也不会使 Steam 自动订阅依赖，依赖需在详情中说明。
