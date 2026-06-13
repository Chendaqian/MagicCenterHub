## 1. 项目初始化与基础架构

- [x] 1.1 创建 .NET 8 WPF 项目 `MagicCenterHub`，配置 net8.0-windows 目标框架
- [x] 1.2 添加 NuGet 依赖：Newtonsoft.Json（硬件数据通过 HWiNFO 共享内存读取，无额外 NuGet 包）
- [x] 1.3 创建项目目录结构：ViewModels/、Services/、Effects/、Models/
- [x] 1.4 实现 App.xaml.cs 单实例逻辑（Mutex）和命令行参数解析（/register、/unregister）

## 2. 应用生命周期 (app-lifecycle)

- [x] 2.1 实现开机自启注册/取消（注册表 HKCU Run）
- [x] 2.2 实现无边框窗口 + Topmost + 拖动移动
- [x] 2.3 实现系统托盘图标（NotifyIcon），双击显示/隐藏，右键菜单（显示/隐藏、退出）
- [x] 2.4 实现关闭按钮最小化到托盘而非退出
- [x] 2.5 实现 Settings 模型和 JSON 持久化（%AppData%\MagicCenterHub\settings.json），含窗口坐标、cpuFrequencyGHz、cpuMaxTempC、gpuMaxTempC、colorThresholds
- [x] 2.6 实现首次启动显示器检测（找最接近 1424x280 的屏幕居中定位）
- [x] 2.7 实现窗口关闭时保存 Left/Top 坐标到配置文件
- [x] 2.8 实现启动时恢复窗口位置 + 校验显示器可用性（不可见则回落主屏幕）

## 3. 硬件信息采集 (hardware-monitor)

- [x] 3.1 定义 HWiNFO 共享内存结构体（HWiNFO_SENSORS_SHARED_MEM、SensorEntry、ReadingEntry）
- [x] 3.2 实现 HwInfoProvider：读取 `Global\HWiNFO_SENS_SM2` 共享内存，解析传感器数据
- [x] 3.3 实现启动检测：检查共享内存是否存在，不存在则弹提示，后台每 5 秒重试
- [x] 3.4 实现从 HWiNFO 数据中筛选 CPU 使用率/温度、GPU 使用率/温度、内存、磁盘、网络、电池指标
- [x] 3.5 实现后台定时采集（1 秒间隔），通过事件推送数据更新
- [x] 3.6 实现 UI 层 N/A 值特殊渲染（灰色文字、进度条归零）

## 4. UI 主界面 (hardware-monitor)

- [x] 4.1 创建 MainWindow.xaml，定义 1424x280 Grid 布局（LED 柱 + 四面板）
- [x] 4.2 实现主背景和面板样式（#2A2A2E / #333338 / #3C3C42 配色、圆角）
- [x] 4.3 实现 CPU 面板（使用率数值 + 进度条 + 温度数值 + 进度条）
- [x] 4.4 实现 GPU 面板（使用率数值 + 进度条 + 温度数值 + 进度条）
- [x] 4.5 实现主机面板（内存占用数值 + 进度条 + 电池功耗数值 + 进度条）
- [x] 4.6 实现磁盘子面板（读取/写入速率两列）
- [x] 4.7 实现网络子面板（上传/下载速率两列）
- [x] 4.8 实现进度条和数值三档颜色编码（绿/琥珀/粉，读取 settings.json 中的 colorThresholds 阈值）
- [x] 4.9 实现图标替代方案（MahApps.Metro.IconPacks 或 SVG Path 替代 Tabler Icons）
- [x] 4.10 绑定 HardwareMonitorService 数据到 ViewModel，实现 INotifyPropertyChanged

## 5. LED 灯效引擎 (led-indicator)

- [x] 5.1 定义 LedMode 枚举（0-19 共 20 种模式）
- [x] 5.2 定义 ILedEffect 接口（Update(deltaMs)、Reset、GetCurrentColor）
- [x] 5.3 实现基础模式 0-8（全灭、同闪、绿闪、红闪、黄闪、绿亮、红亮、黄亮、三灯常亮）
- [x] 5.4 实现模式 9 警车交替快闪（红绿反相交替 300ms + 黄灯独立快闪 150ms）
- [x] 5.5 实现模式 10 心跳双闪（时序数组 {80,100,80,600} 状态机）
- [x] 5.6 实现模式 11 SOS 摩尔斯码（20 步时序数组状态机）
- [x] 5.7 实现模式 12 三色轮转呼吸灯（红绿反向渐变 + 黄灯 120° 相位偏移）
- [x] 5.8 实现模式 13 萤火虫混沌呼吸（三灯独立正弦波，周期不同产生非对称律动）
- [x] 5.9 实现模式 14 心电波模拟（ECG 波形：P 波、QRS 峰、T 波，黄灯 P/T 波柔和发光）
- [x] 5.10 实现模式 15 三相正弦追逐（绿 sin、红 cos、黄 sin+120°，3s 周期）
- [x] 5.11 实现模式 16 急救爆闪追击（绿→黄→红各闪 3 下，80ms 亮/80ms 灭）
- [x] 5.12 实现模式 17 太极阴阳呼吸（三阶正弦 y=sin³(x)，红绿对立消长，黄灯独立）
- [x] 5.13 实现模式 18 HELLO 摩尔斯广播（H-E-L-L-O 编码）
- [x] 5.14 实现模式 19 雷达扫描锁定（3s 扫描 → 1s 锁定 → 0.5s 全亮）

## 6. LED UI 组件 (led-indicator)

- [x] 6.1 创建 LedIndicator.xaml 用户控件（三个 Ellipse：红/黄/绿，各三层渲染）
- [x] 6.2 实现 CompositionTarget.Rendering 驱动的动画循环
- [x] 6.3 实现 LedEffectFactory，根据模式编号创建对应 ILedEffect 实例
- [x] 6.4 实现黄灯独立控制（常亮/闪烁两种状态）
- [x] 6.5 集成到 MainWindow.xaml 左侧 130px 灯柱区域

## 7. 命名管道监听 (hook-listener)

- [x] 7.1 创建 NamedPipeListenerService，监听 `\\.\pipe\ClaudeCodeMagicCenterHub`
- [x] 7.2 实现 JSON 消息解析（ledMode 字段，0-19 数字编号）
- [x] 7.3 实现灯效模式切换（收到 ledMode 后直接设置对应 LedMode）
- [x] 7.4 实现未知编号忽略和格式错误容错
- [x] 7.5 实现客户端断开后自动重新监听

## 8. 设置界面 (settings-window)

- [x] 8.1 创建 SettingsWindow.xaml 设置窗口（420x660 布局，深色主题）
- [x] 8.2 实现开机自启开关（注册表 HKCU Run 读写）
- [x] 8.3 实现窗口置顶开关
- [x] 8.4 实现窗口位置手动输入和"取当前"按钮
- [x] 8.5 实现采集间隔、CPU/GPU 温度上限配置
- [x] 8.6 实现使用率和温度的颜色阈值配置（绿/黄上限）
- [x] 8.7 实现保存功能（写入 settings.json + 回调通知主窗口）

## 9. LED 灯效测试窗口 (led-test-window)

- [x] 9.1 创建 LedTestWindow.xaml 测试窗口（500x460 布局）
- [x] 9.2 实现 20 个灯效模式按钮（0-19），点击即切换对应灯效
- [x] 9.3 实现状态文本显示当前灯效编号和名称
- [x] 9.4 集成到托盘右键菜单（"LED 测试"选项）

## 10. 集成与收尾

- [x] 10.1 将硬件采集服务、LED 引擎、管道监听服务集成到主窗口生命周期
- [x] 10.2 实现窗口 IsVisible 控制（不可见时暂停 LED 渲染循环节省 CPU）
- [x] 10.3 创建 Claude Code hook 配置示例脚本和 settings.json 示例
- [x] 10.4 编写项目 README（使用说明、hook 配置、灯效模式说明）
- [ ] 10.5 验证所有 20 种灯效视觉效果正确（需手动运行测试）
- [ ] 10.6 验证开机自启、托盘、单实例等功能正常（需手动运行测试）
- [ ] 10.7 验证设置界面所有配置项保存/加载正常（需手动运行测试）
