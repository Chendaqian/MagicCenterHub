## Why

需要一款 Windows 桌面系统监控工具，以悬浮窗形式常驻桌面，实时显示 CPU/GPU/内存/磁盘/网络等硬件指标，并通过 LED 灯效组件接收 Claude Code hook 事件，直观反映 AI 工作状态。当前没有轻量级的、将硬件监控与 AI 状态指示合二为一的桌面工具。

## What Changes

- 新建 WPF 桌面应用程序，支持开机自启、窗口置顶
- 实现硬件信息采集模块：CPU 使用率/温度、GPU 使用率/温度、内存占用、电池功耗、磁盘读写速率、网络上下行速率
- 实现红绿灯 LED 组件（20 种灯效模式），参照 antigravity-task-led 项目的 Arduino FSM 逻辑移植到 WPF 动画
- 实现命名管道监听服务，接收外部 Claude Code hook 调用，将 hook 事件映射到对应灯效
- UI 布局严格按照 `system_monitor_v3.html` 设计稿实现：左侧 LED 灯柱 + 右侧四个面板（CPU/GPU/主机/磁盘网络）

## Capabilities

### New Capabilities
- `hardware-monitor`: 系统硬件信息采集与展示，包括 CPU/GPU/内存/磁盘/网络的实时数据采集和 UI 渲染
- `led-indicator`: 红绿双色 LED 灯效组件，支持 0-19 共 20 种灯语模式的 WPF 动画实现
- `hook-listener`: 命名管道监听服务，接收 Claude Code hook 事件并映射到 LED 状态
- `app-lifecycle`: 应用生命周期管理，包括开机自启、窗口置顶、托盘图标等
- `settings-window`: 图形化设置界面，支持配置开机自启、窗口位置、采集间隔、温度上限、颜色阈值等参数
- `led-test-window`: LED 灯效测试窗口，提供 20 个按钮快速切换各灯效模式，便于开发调试

### Modified Capabilities

（无已有能力需要修改）

## Impact

- 新增 WPF 项目，目标框架待定（.NET Framework 4.5 或 .NET 8）
- 依赖 HWiNFO64 共享内存读取硬件传感器数据（用户需安装运行 HWiNFO64）
- 需要实现与 Claude Code hook 的集成协议（命名管道 JSON 消息格式）
- 设计稿引用了 Tabler Icons 字体图标，WPF 端需替换为等效的图标方案
