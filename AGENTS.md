# CLAUDE.md - MagicCenterHub

## 项目简介

Windows 桌面系统监控悬浮窗，实时显示硬件指标 + Claude Code hook LED 灯效指示。

## 技术栈

- 语言：C# (.NET 8, net8.0-windows)
- UI 框架：WPF
- 硬件数据源：HWiNFO64 共享内存 (`Global\HWiNFO_SENS_SM2`)
- 序列化：Newtonsoft.Json
- 配置路径：`%AppData%\MagicCenterHub\settings.json`

## 配置模型 (Settings)

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| WindowLeft | double | -1 | 窗口 X 坐标（-1 表示自动） |
| WindowTop | double | -1 | 窗口 Y 坐标（-1 表示自动） |
| WindowTopMost | bool | true | 窗口置顶 |
| CpuFrequencyGHz | double | 0 | CPU 频率基准 |
| CpuMaxTempC | double | 100 | CPU 温度上限（进度条满值） |
| GpuMaxTempC | double | 95 | GPU 温度上限（进度条满值） |
| PollIntervalMs | int | 3000 | HWiNFO 采集间隔（毫秒） |
| ColorThresholds | object | 见下 | 三档颜色阈值 |
| PositionPresets | list | [] | 窗口位置预设 |

### ColorThresholds
| 字段 | 默认值 | 说明 |
|------|--------|------|
| UsageGreen | 50 | 使用率绿色阈值 |
| UsageYellow | 80 | 使用率黄色阈值 |
| TempGreen | 60 | 温度绿色阈值 |
| TempYellow | 80 | 温度黄色阈值 |

## 项目结构

```
src/MagicCenterHub/
├── App.xaml / App.xaml.cs              # 入口：单实例、托盘图标、开机自启
├── MainWindow.xaml / .cs               # 主窗口：1428x284 布局、LED 动画循环
├── LedTestWindow.xaml / .cs            # LED 灯效测试窗口（托盘右键打开）
├── SettingsWindow.xaml / .cs           # 设置界面（开机自启、窗口位置、阈值配置）
├── PercentToWidthConverter.cs          # 进度条百分比→宽度转换器
├── IconHelper.cs                       # 图标加载辅助（多尺寸 ICO 支持）
├── Controls/
│   └── MaterialIcons.cs                # Material Design 图标控件
├── Models/
│   ├── Settings.cs                     # 配置模型（窗口位置、阈值、硬件参数）
│   ├── HardwareData.cs                 # 硬件数据模型（带 Valid 标记）
│   └── LedMode.cs                      # 20 种灯效枚举 (0-19)
├── Services/
│   ├── SettingsService.cs              # JSON 配置持久化（%AppData%）
│   ├── HwInfoProvider.cs               # HWiNFO v2 共享内存读取器
│   ├── HardwareMonitorService.cs       # 1 秒定时采集 + 重试检测
│   └── NamedPipeListenerService.cs     # 命名管道 hook 监听
├── Effects/
│   ├── ILedEffect.cs                   # 灯效接口（LedColor 结构体）
│   ├── LedEffects.cs                   # 20 种灯效实现（每种一个类）
│   └── LedEffectFactory.cs             # 工厂模式创建灯效实例
├── ViewModels/
│   └── MainViewModel.cs                # 数据绑定 + 三档颜色 + hook 事件映射
└── Resources/
    └── MagicCenterHub.ico              # 托盘图标（16/32/48/256 多尺寸）
```

## 核心架构

### 硬件数据流
```
HWiNFO64 共享内存 → HwInfoProvider.Read() → HardwareData
  → HardwareMonitorService (1s定时) → MainViewModel.UpdateHardwareData()
  → WPF DataBinding → UI 刷新
```

### LED 灯效数据流
```
命名管道 / Hook 事件 → MainViewModel.HandleHookEvent()
  → SetLedMode(LedMode) → LedEffectFactory.Create()
  → CompositionTarget.Rendering (60fps) → ILedEffect.Update(elapsedMs)
  → LedColor {Green, Red, Yellow} → WPF DataBinding → Ellipse.Fill
```

### 命名管道协议
- 管道名：`\\.\pipe\ClaudeCodeMagicCenterHub`
- 消息格式：`{"ledMode": 17}`
- `ledMode`：灯效编号 0-19（必填）
- 通知由 hook 脚本通过 BurntToast 直接弹出，不经过管道

## 关键设计决策

- **HWiNFO 共享内存**：v2 格式，头部 44 字节含偏移指针，Reading 元素 316 字节，通过 `SensorType` + 标签关键词匹配指标
- **LED 灯效**：CompositionTarget.Rendering + 非阻塞状态机，与 Arduino millis() 模式一致
- **三档颜色**：绿(#6FCF6F) → 琥珀(#F0A840) → 粉(#F06080)，阈值可配置
- **黄灯**：参与灯效模式（模式 1/4/7/8/9/12/13/14/15/16/17/18/19），不独立控制
- **窗口**：无边框置顶、拖动移动、关闭最小化到托盘、位置记忆
