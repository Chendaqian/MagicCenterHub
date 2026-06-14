# MagicCenterHub 🚦

<div align="center">
    <img width="463" height="364" alt="PixPin_2026-06-13_19-40-09" src="https://github.com/user-attachments/assets/ff76e92f-a932-449c-ae15-31b27944fd02" />
</div>

Windows 桌面系统监控悬浮窗，实时显示硬件指标 + Claude Code hook LED 灯效指示。

[English](README.md)

[![.NET 8](https://img.shields.io/badge/.NET-8-blue)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF-purple)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
[![Windows](https://img.shields.io/badge/Platform-Windows-0078d4)](https://www.microsoft.com/windows)
[![GitHub Release](https://img.shields.io/github/v/release/ChenDaqian/MagicCenterHub?label=Release)](https://github.com/ChenDaqian/MagicCenterHub/releases/latest)
[![Build Status](https://img.shields.io/github/actions/workflow/status/ChenDaqian/MagicCenterHub/publish.yml?label=Build)](https://github.com/ChenDaqian/MagicCenterHub/actions/workflows/publish.yml)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
[![GitHub Stars](https://img.shields.io/github/stars/ChenDaqian/MagicCenterHub?style=flat)](https://github.com/ChenDaqian/MagicCenterHub/stargazers)
[![GitHub Downloads](https://img.shields.io/github/downloads/ChenDaqian/MagicCenterHub/total?style=flat)](https://github.com/ChenDaqian/MagicCenterHub/releases/latest)
[![GitHub Last Commit](https://img.shields.io/github/last-commit/ChenDaqian/MagicCenterHub?style=flat)](https://github.com/ChenDaqian/MagicCenterHub/commits/master)

## 功能特性

### 硬件监控
- **CPU** — 使用率、温度
- **GPU** — 使用率、温度
- **内存** — 已用/总量、使用率百分比
- **电池** — 功耗
- **磁盘** — 读取/写入速率
- **网络** — 上传/下载速率

### LED 灯效指示
三色 LED（红/黄/绿）实时反映 Claude Code 工具调用状态，支持 20 种灯效模式：

| 编号 | 模式 | 说明 |
|------|------|------|
| 0 | 全灭 | 三灯熄灭 |
| 1 | 同闪 | 三灯同频闪烁 |
| 2 | 绿闪 | 绿灯闪烁 |
| 3 | 红闪 | 红灯闪烁 |
| 4 | 黄闪 | 黄灯闪烁 |
| 5 | 绿亮 | 绿灯常亮 |
| 6 | 红亮 | 红灯常亮 |
| 7 | 黄亮 | 黄灯常亮 |
| 8 | 全亮 | 三灯常亮 |
| 9 | 警车 | 红绿交替 + 黄灯独立快闪 |
| 10 | 心跳 | 模拟心脏律动 |
| 11 | SOS | 摩尔斯求救码 |
| 12 | 呼吸 | 三色轮转呼吸灯 |
| 13 | 萤火虫 | 三灯独立混沌呼吸 |
| 14 | 心电 | ECG 波形模拟 |
| 15 | 跑马 | 三色追逐爆闪 |
| 16 | 爆闪 | 绿→黄→红依次爆闪 |
| 17 | 太极 | 阴阳消长呼吸 |
| 18 | HELLO | 摩尔斯码广播 |
| 19 | 雷达 | 扫描与锁定 |

### Hook 事件映射

| Claude Code 事件 | LED 模式 | 通知 |
|------------------|----------|------|
| PreToolUse | 2 - 绿闪 | — |
| UserPromptSubmit | 2 - 绿闪 | — |
| PermissionRequest | 4 - 黄闪 | ⏳ 需要权限确认 |
| PostToolUseFailure | 3 - 红闪 | ❌ 工具执行失败 |
| Stop | 5 - 绿亮 | ✅ 任务已完成 |
| SessionStart | 17 - 太极 | — |
| SessionEnd | 17 - 太极 | — |

> 映射在 `hooks/claude-code-settings-example.json` 中配置，应用本身不硬编码事件映射。

## 技术栈

- **语言**：C# 7.3 (.NET 8, net8.0-windows)
- **UI**：WPF
- **硬件数据源**：HWiNFO64 共享内存 (`Global\HWiNFO_SENS_SM2`)
- **序列化**：Newtonsoft.Json
- **编码**：System.Text.Encoding.CodePages (GBK)

## 项目结构

```
src/MagicCenterHub/
├── App.xaml / App.xaml.cs              # 入口：单实例、托盘图标、开机自启
├── MainWindow.xaml / .cs               # 主窗口：1428x284 布局、LED 动画循环
├── LedTestWindow.xaml / .cs            # LED 灯效测试窗口
├── SettingsWindow.xaml / .cs           # 设置窗口
├── Utils/
│   ├── PercentToWidthConverter.cs      # 进度条百分比→宽度转换器
│   └── IconHelper.cs                   # 图标生成工具
├── Models/
│   ├── Settings.cs                     # 配置模型
│   ├── HardwareData.cs                 # 硬件数据模型
│   └── LedMode.cs                      # 20 种灯效枚举
├── Services/
│   ├── SettingsService.cs              # JSON 配置持久化
│   ├── HwInfoProvider.cs               # HWiNFO 共享内存读取器
│   ├── HardwareMonitorService.cs       # 1 秒定时采集
│   └── NamedPipeListenerService.cs     # 命名管道 hook 监听
├── Effects/
│   ├── ILedEffect.cs                   # 灯效接口
│   ├── LedEffects.cs                   # 20 种灯效实现
│   └── LedEffectFactory.cs             # 工厂模式创建灯效
├── ViewModels/
│   └── MainViewModel.cs                # 数据绑定 + 三档颜色
├── Resources/
│   ├── MagicCenterHub.ico              # 托盘图标
│   └── Claude.png                      # 通知图标
└── hooks/
    ├── send-hook.ps1                   # Hook 脚本：LED 控制 + BurntToast 通知
    ├── claude-code-settings-example.json  # Claude Code hooks 配置示例
    └── sound-test.ps1                  # 音效测试工具
```

## 配置

配置文件路径：`%AppData%\MagicCenterHub\settings.json`

| 字段 | 说明 | 默认值 |
|------|------|--------|
| WindowLeft | 窗口 X 坐标（-1 = 自动定位） | -1 |
| WindowTop | 窗口 Y 坐标（-1 = 自动定位） | -1 |
| WindowTopMost | 窗口置顶 | true |
| PollIntervalMs | HWiNFO 采集间隔（毫秒） | 3000 |
| CpuMaxTempC | CPU 温度上限 | 100 |
| GpuMaxTempC | GPU 温度上限 | 95 |
| ColorThresholds.UsageGreen | 使用率绿色阈值 | 50 |
| ColorThresholds.UsageYellow | 使用率黄色阈值 | 80 |
| ColorThresholds.TempGreen | 温度绿色阈值 | 60 |
| ColorThresholds.TempYellow | 温度黄色阈值 | 80 |
| PositionPresets | 窗口位置预设列表 | [] |

## 前置要求

1. **HWiNFO64** — 运行并启用 Shared Memory Support
2. **管理员权限** — 访问全局共享内存需要管理员权限

## 命名管道协议

- 管道名：`\\.\pipe\ClaudeCodeMagicCenterHub`
- 消息格式：`{"ledMode": N}`（N = 0-19，灯效模式编号）
- 测试命令：`powershell -File send-hook.ps1 -LedMode 2`

> 事件到灯效编号的映射在 `hooks/claude-code-settings-example.json` 中配置。

## 许可证

MIT License
