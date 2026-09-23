# 场景：添加声音与播放反馈

适用：给物品操作、机器完成、交易反馈或事件添加提示音，或播放背景音乐与环境音。

## 资源路径

| 加载方式 | 候选入口 | 需要确认 |
| --- | --- | --- |
| 游戏内置 ModLoader 内容包 | `LoadedMod.Sounds`、`ModHook.OnModAssetsLoaded(LoadedMod)` | 当前构建识别的文件格式、目录与命名规则；回调中是否已注册到音频目录 |
| MelonLoader 插件自行加载 | 创建可用 `AudioClip` 后调用 `AudioDirectory.RegisterModClip(identifier, clip)` | 音频解码、Unity 对象创建线程和资源寿命；不能假设内置加载器会扫描插件目录 |

播放前等 `AudioManager.Instance` 和其 `audioDirectory` 就绪，通过 `HasAudioClip` / `GetAudioClip` 或 `CanPlay` 核对 ID。`AudioManager` 提供 `Play`、`Play2`、`Play3`、`PlayMusic`、`PlayAmbient`；其返回值、声道占用、循环和音量规则要在当前构建及实际游戏中确认。使用带命名空间的稳定 ID，避免覆盖原版或其他 Mod 的音频键。

## 性能与释放

音频文件在资源加载阶段解码并缓存；不要在每帧更新或每次鼠标事件中重新读盘、解码、创建 `AudioClip`。高频事件节流，长音频核对内存与流式加载需求。场景切换、停用 Mod 或资源重载时明确谁拥有 `AudioClip`，不要销毁仍由音频目录或正在播放的声道引用的资源；当前声明只看到注册接口，不能假定有注销接口。

## 验证

分别试验正常播放、资源缺失、快速重复触发、换场景与重新加载；确认声音可听、音量符合游戏设置、没有重复叠放或音频资源持续增长。仅有 `LoadedMod.Sounds` 条目或 `RegisterModClip` 调用不能证明实际能播。
