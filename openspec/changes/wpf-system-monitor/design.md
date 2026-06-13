## Context

需要开发一款 WPF 桌面系统监控悬浮窗应用。设计稿 `system_monitor_v3.html` 定义了 1424x280px 的横向布局：左侧 130px 红绿灯柱 + 右侧 CPU/GPU/主机/磁盘网络四个面板。参考项目 antigravity-task-led 提供了 20 种 LED 灯效的 Arduino FSM 实现，需要移植到 WPF 动画层。

当前项目 MagicCenterHub 的技术栈为 C# / .NET，需确定目标框架版本。

## Goals / Non-Goals

**Goals:**
- 实时显示 CPU 使用率/温度、GPU 使用率/温度、内存占用、电池功耗、磁盘读写速率、网络上下行速率
- 实现 20 种 LED 灯效模式，视觉效果与 Arduino PWM 版本对齐
- 通过命名管道接收 Claude Code hook 事件，自动切换灯效
- 开机自启、窗口置顶、无边框悬浮窗
- 低资源占用，后台持续运行

**Non-Goals:**
- 不做历史数据存储或图表趋势
- 不做多显示器联动（但支持首次启动时自动定位到小屏幕 1424x280）
- 不做远程推送（与原项目的 MQTT/巴法云无关）
- 不做用户配置 UI（配置通过文件或后期扩展）

## Decisions

### D1: 目标框架选择 .NET 8

**选择**: .NET 8 (net8.0-windows)

**理由**:
- .NET 8 是 LTS 版本，支持到 2026 年 11 月
- 性能显著优于 .NET Framework 4.5，尤其是 GC 和硬件信息采集的高频率调用
- WPF 在 .NET 8 中完全支持，且可使用 CommunityToolkit.Mvvm 等现代库
- HWiNFO64 共享内存读取无需额外 NuGet 包

**备选方案**:
- .NET Framework 4.5: 兼容性好但性能差，缺少现代 C# 语法特性，且微软已停止更新

### D2: 硬件信息采集 — HWiNFO64 Shared Memory

**方案**: HWiNFO64 Shared Memory（唯一数据源）

HWiNFO 是硬件支持最全面的监控工具，传感器数据库更新快，对 Intel Core Ultra 系列（358H/388H）有良好支持。它通过内存映射文件 `Global\HWiNFO_SENS_SM2` 暴露所有传感器数据，我们的程序只需读共享内存，无需管理员权限。

- 读取方式：`MemoryMappedFile.OpenExisting("Global\HWiNFO_SENS_SM2")` → 解析 `HWiNFO_SENSORS_SHARED_MEM` 结构体
- 数据结构：Header（传感器数量/轮询次数）+ SensorEntry[]（名称/类型）+ ReadingEntry[]（值/单位/标签）
- 优势：零开销、实时、覆盖 CPU/GPU 温度/使用率/功耗/风扇转速等全部指标

**启动检测**: 应用启动时检查共享内存 `Global\HWiNFO_SENS_SM2` 是否存在：
- 存在 → 正常启动，开始采集
- 不存在 → 弹出提示"请先安装并运行 HWiNFO64"，窗口显示但所有数据为 N/A，后台每 5 秒重试检测

**不使用降级方案的原因**: LHM 对 Intel Core Ultra 支持不完整（LiteMonitor#448），WMI/PerfCounter 无法获取温度，降级意义不大。统一要求 HWiNFO 更简单可靠。

### D3: LED 灯效使用 CompositionTarget.Rendering + 状态机

**选择**: 基于 `CompositionTarget.Rendering` 事件的非阻塞有限状态机

**理由**:
- 与 Arduino `millis()` 模式一致，便于移植 19 种灯效逻辑
- `CompositionTarget.Rendering` 以显示器刷新率（~60fps）触发，适合平滑动画
- 状态机模式：每种灯效定义自己的时序数组和状态变量，主循环根据 elapsed 时间推进状态
- WPF 的 `Ellipse.Fill` 使用 `SolidColorBrush`，通过修改 `Color.A` (alpha) 实现 PWM 效果

**备选方案**:
- Storyboard/DoubleAnimation: 难以实现复杂时序（如 SOS 摩尔斯码、ECG 波形）
- DispatcherTimer: 精度不如 CompositionTarget，且需要多个 Timer 管理不同灯效

### D4: 命名管道通信协议

**选择**: `NamedPipeServerStream` + JSON 消息格式

**消息格式**:
```json
{
  "event": "PreToolUse" | "PostToolUse" | "PostToolUseFailure" | "UserPromptSubmit" | "PermissionRequest" | "Notification" | "Stop",
  "tool": "Bash",
  "success": true,
  "timestamp": "2026-06-09T10:00:00Z"
}
```

**状态映射**:

| Hook 事件 | LED 状态 | 默认灯效 |
|-----------|---------|---------|
| PreToolUse / UserPromptSubmit | THINKING | 10 (呼吸灯) |
| PermissionRequest / Notification | WAITING_USER | 3 (红灯闪) |
| PostToolUseFailure | TOOL_ERROR | 5 (红灯常亮) |
| Stop | TASK_COMPLETE | 16 (太极呼吸) |

**理由**:
- 命名管道是 Windows 本地进程间通信的标准方式，延迟低
- JSON 格式易于扩展，Claude Code hook 脚本只需 `echo` JSON 到管道
- 管道名: `\\.\pipe\ClaudeCodeMagicCenterHub`

### D5: 窗口样式与布局

**选择**: 无边框 `WindowStyle="None"` + `AllowsTransparency="True"` + `Topmost="True"`

**布局结构**:
```
+--Grid (1424x280)---------------------------+
| LED柱  |  CPU面板  | GPU面板 | 主机面板 | 磁盘网络 |
| 130px  |  flex:1   | flex:1  | flex:1.1 | flex:1.2 |
+--------+-----------+---------+----------+---------+
```

- 背景色: `#2A2A2E` (主背景), `#333338` (面板), `#3C3C42` (子面板)
- 字体: `Courier New` (等宽，系统自带)
- 图标: 使用 Segoe Fluent Icons 或引入 MahApps.Metro.IconPacks 替代 Tabler Icons
- 圆角: 使用 `Clip` 或 `Border.CornerRadius`

### D6: 开机自启实现

**选择**: 注册表 `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`

**理由**:
- 不需要管理员权限
- 用户级自启，不影响其他用户
- 通过 `/register` 和 `/unregister` 命令行参数控制

### D7: 窗口位置持久化与多显示器定位

**选择**: JSON 配置文件 + Screen API 显示器检测

**存储**: `%AppData%\MagicCenterHub\settings.json`
```json
{
  "windowLeft": 100,
  "windowTop": 200,
  "cpuFrequencyGHz": 3.6,
  "cpuMaxTempC": 100,
  "gpuMaxTempC": 95,
  "colorThresholds": {
    "usageGreen": 50,
    "usageYellow": 80,
    "tempGreen": 60,
    "tempYellow": 80
  }
}
```

硬件参数说明：
- `cpuFrequencyGHz`: 用户的 CPU 主频，仅用于展示
- `cpuMaxTempC` / `gpuMaxTempC`: 温度进度条的满量程，不同 CPU 的 TjMax 不同（如 Intel 100°C、AMD 95°C）
- `colorThresholds`: 三档颜色切换阈值，usage 类指标（CPU/GPU 使用率、内存）和 temp 类指标（温度）分别控制

**首次启动定位策略**:
- 遍历 `System.Windows.Forms.Screen.AllScreens`
- 找到工作区尺寸最接近 1424x280 的屏幕（按宽度优先匹配）
- 窗口居中显示在该屏幕工作区内

**位置恢复策略**:
- 读取配置中的 Left/Top，校验是否在当前任一显示器的可见范围内
- 若不可见（显示器断开），回落到主屏幕居中
- 窗口关闭时（OnClosing）保存当前坐标

**理由**:
- JSON 配置文件简单，与项目已有的 Newtonsoft.Json 依赖一致
- `%AppData%` 是 Windows 标准的用户数据存储位置
- 显示器检测用 WinForms Screen API（WPF 无等效 API，需引用 System.Windows.Forms）

## Risks / Trade-offs

- **[HWiNFO64 需要用户自行安装并运行]** → 启动时检测共享内存，不存在则弹提示，后台每 5 秒重试；UI 空数据区域显示 N/A
- **[共享内存结构体可能随 HWiNFO 版本变化]** → 解析时做版本校验，结构不匹配时提示用户更新 HWiNFO
- **[CompositionTarget.Rendering 在窗口不可见时仍触发]** → 使用 `IsVisible` 属性暂停渲染循环，节省 CPU
- **[20 种灯效的代码量较大]** → 将每种灯效封装为独立的 `ILedEffect` 实现类，通过工厂模式切换
- **[设计稿使用 Tabler Icons]** → WPF 原生不支持，需替换为 MahApps.Metro.IconPacks 或自定义 SVG Path
