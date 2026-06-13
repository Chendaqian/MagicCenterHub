# MagicCenterHub

A Windows desktop monitoring widget with real-time hardware metrics and Claude Code hook LED indicator.

<img width="300" height="236" style="display: block; margin: 0 auto;" alt="aiLED" src="https://github.com/user-attachments/assets/ff76e92f-a932-449c-ae15-31b27944fd02" />

[中文文档](README.zh-CN.md)

[![.NET 8](https://img.shields.io/badge/.NET-8-blue)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF-purple)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
[![Windows](https://img.shields.io/badge/Platform-Windows-0078d4)](https://www.microsoft.com/windows)
[![GitHub Release](https://img.shields.io/github/v/release/ChenDaqian/MagicCenterHub?label=Release)](https://github.com/ChenDaqian/MagicCenterHub/releases/latest)
[![Build Status](https://img.shields.io/github/actions/workflow/status/ChenDaqian/MagicCenterHub/publish.yml?label=Build)](https://github.com/ChenDaqian/MagicCenterHub/actions/workflows/publish.yml)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
[![GitHub Stars](https://img.shields.io/github/stars/ChenDaqian/MagicCenterHub?style=flat)](https://github.com/ChenDaqian/MagicCenterHub/stargazers)
[![GitHub Downloads](https://img.shields.io/github/downloads/ChenDaqian/MagicCenterHub/total?style=flat)](https://github.com/ChenDaqian/MagicCenterHub/releases/latest)
[![GitHub Last Commit](https://img.shields.io/github/last-commit/ChenDaqian/MagicCenterHub?style=flat)](https://github.com/ChenDaqian/MagicCenterHub/commits/master)

## Features

<img width="1418" height="279" alt="AIHUD" src="https://github.com/user-attachments/assets/9807456f-9da9-4163-afa2-574af535069d" />


### Hardware Monitoring
- **CPU** — Usage, Temperature
- **GPU** — Usage, Temperature
- **Memory** — Used/Total, Usage Percentage
- **Battery** — Power Consumption
- **Disk** — Read/Write Speed
- **Network** — Upload/Download Speed

### LED Effect Indicator
Three-color LED (Red/Yellow/Green) reflects Claude Code tool call status in real-time, supporting 20 effect modes:

| # | Mode | Description |
|---|------|-------------|
| 0 | All Off | All LEDs off |
| 1 | All Flash | All LEDs flash in sync |
| 2 | Green Flash | Green LED flashing |
| 3 | Red Flash | Red LED flashing |
| 4 | Yellow Flash | Yellow LED flashing |
| 5 | Green On | Green LED steady |
| 6 | Red On | Red LED steady |
| 7 | Yellow On | Yellow LED steady |
| 8 | All On | All LEDs steady |
| 9 | Police | Red/Green alternating + Yellow independent flash |
| 10 | Heartbeat | Heart rhythm simulation |
| 11 | SOS | Morse code distress signal |
| 12 | Breathing | Three-color rotating breath |
| 13 | Firefly | Three LEDs independent chaotic breathing |
| 14 | ECG | ECG waveform simulation |
| 15 | Phase Chase | Three-color chasing strobe |
| 16 | Strobe Chase | Green→Yellow→Red sequential strobe |
| 17 | Taichi | Yin-Yang breathing |
| 18 | HELLO | Morse code broadcast |
| 19 | Radar | Scan and lock |

### Hook Event Mapping

| Claude Code Event | LED Mode | Notification |
|-------------------|----------|-------------|
| PreToolUse | 2 - Green Flash | — |
| UserPromptSubmit | 2 - Green Flash | — |
| PermissionRequest | 4 - Yellow Flash | ⏳ 需要权限确认 |
| PostToolUseFailure | 3 - Red Flash | ❌ 工具执行失败 |
| Stop | 5 - Green On | ✅ 任务已完成 |
| SessionStart | 17 - Taichi | — |
| SessionEnd | 17 - Taichi | — |

> Mapping is configured in `hooks/claude-code-settings-example.json`, not hardcoded in the application.

## Tech Stack

- **Language**: C# 7.3 (.NET 8, net8.0-windows)
- **UI**: WPF
- **Hardware Data**: HWiNFO64 Shared Memory (`Global\HWiNFO_SENS_SM2`)
- **Serialization**: Newtonsoft.Json
- **Encoding**: System.Text.Encoding.CodePages (GBK)

## Project Structure

```
src/MagicCenterHub/
├── App.xaml / App.xaml.cs              # Entry: single instance, tray icon, auto-start
├── MainWindow.xaml / .cs               # Main window: 1428x284 layout, LED animation loop
├── LedTestWindow.xaml / .cs            # LED effect test window
├── SettingsWindow.xaml / .cs           # Settings window
├── Utils/
│   ├── PercentToWidthConverter.cs      # Progress bar percentage→width converter
│   └── IconHelper.cs                   # Icon generation utility
├── Models/
│   ├── Settings.cs                     # Configuration model
│   ├── HardwareData.cs                 # Hardware data model
│   └── LedMode.cs                      # 20 LED effect modes enum
├── Services/
│   ├── SettingsService.cs              # JSON config persistence
│   ├── HwInfoProvider.cs               # HWiNFO shared memory reader
│   ├── HardwareMonitorService.cs       # 1-second polling
│   └── NamedPipeListenerService.cs     # Named pipe hook listener
├── Effects/
│   ├── ILedEffect.cs                   # LED effect interface
│   ├── LedEffects.cs                   # 20 LED effect implementations
│   └── LedEffectFactory.cs             # Factory pattern for effects
├── ViewModels/
│   └── MainViewModel.cs                # Data binding + color thresholds
├── Resources/
│   ├── MagicCenterHub.ico              # Tray icon
│   └── Claude.png                      # Notification icon
└── hooks/
    ├── send-hook.ps1                   # Hook script: LED control + BurntToast notification
    ├── claude-code-settings-example.json  # Claude Code hooks configuration example
    └── sound-test.ps1                  # Sound effect test tool
```

## Configuration

Config file path: `%AppData%\MagicCenterHub\settings.json`

| Field | Description | Default |
|-------|-------------|---------|
| WindowLeft | Window X position (-1 = auto) | -1 |
| WindowTop | Window Y position (-1 = auto) | -1 |
| WindowTopMost | Always on top | true |
| PollIntervalMs | HWiNFO polling interval (ms) | 3000 |
| CpuMaxTempC | CPU max temperature | 100 |
| GpuMaxTempC | GPU max temperature | 95 |
| ColorThresholds.UsageGreen | Usage green threshold (%) | 50 |
| ColorThresholds.UsageYellow | Usage yellow threshold (%) | 80 |
| ColorThresholds.TempGreen | Temperature green threshold (°C) | 60 |
| ColorThresholds.TempYellow | Temperature yellow threshold (°C) | 80 |
| PositionPresets | Window position presets list | [] |

## Prerequisites

1. **HWiNFO64** — Running with Shared Memory Support enabled
2. **Administrator privileges** — Required to access global shared memory

## Named Pipe Protocol

- Pipe name: `\\.\pipe\ClaudeCodeMagicCenterHub`
- Message format: `{"ledMode": N}` (N = 0-19, LED effect mode number)
- Test command: `powershell -File send-hook.ps1 -LedMode 2`

> The mapping from Claude Code events to LED mode numbers is configured in `hooks/claude-code-settings-example.json`.

## License

MIT License
