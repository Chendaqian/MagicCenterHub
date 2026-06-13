## ADDED Requirements

### Requirement: 开机自启
系统 SHALL 支持通过命令行参数 `/register` 将自身注册到 Windows 开机自启（注册表 `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`）。使用 `/unregister` 可取消自启。

#### Scenario: 注册开机自启
- **WHEN** 用户运行 `MagicCenterHub.exe /register`
- **THEN** 注册表中添加自启项，下次开机自动启动

#### Scenario: 取消开机自启
- **WHEN** 用户运行 `MagicCenterHub.exe /unregister`
- **THEN** 注册表中移除自启项

### Requirement: 窗口置顶
系统 SHALL 以 `Topmost` 模式运行，窗口始终显示在其他窗口上方。

#### Scenario: 窗口始终在前
- **WHEN** 用户打开其他应用程序窗口
- **THEN** 系统监控窗口始终保持在最前方

### Requirement: 无边框悬浮窗
窗口 SHALL 使用无边框样式（`WindowStyle="None"`），不可通过标题栏拖动。用户 SHALL 能通过鼠标拖动窗口任意区域移动位置。

#### Scenario: 拖动移动窗口
- **WHEN** 用户在窗口任意位置按住鼠标左键拖动
- **THEN** 窗口跟随鼠标移动

### Requirement: 资源占用控制
应用在空闲状态下的 CPU 占用 SHALL 不超过 1%，内存占用 SHALL 不超过 100MB。硬件信息采集和 UI 刷新 SHALL 使用异步机制避免阻塞 UI 线程。

#### Scenario: 持续运行稳定性
- **WHEN** 应用连续运行 24 小时
- **THEN** 无内存泄漏，CPU 占用稳定在 1% 以下

### Requirement: 系统托盘支持
系统 SHALL 在系统托盘区域显示图标。双击托盘图标可显示/隐藏主窗口。右键托盘图标 SHALL 显示上下文菜单（显示/隐藏、退出）。

#### Scenario: 最小化到托盘
- **WHEN** 用户关闭主窗口
- **THEN** 窗口隐藏，仅保留托盘图标，应用继续后台运行

#### Scenario: 从托盘恢复
- **WHEN** 用户双击托盘图标
- **THEN** 主窗口重新显示

### Requirement: 窗口位置记忆
系统 SHALL 在窗口关闭时将当前 Left、Top 坐标持久化到本地配置文件（JSON 格式，路径为 `%AppData%\MagicCenterHub\settings.json`）。下次启动时 SHALL 恢复到上次保存的位置。

#### Scenario: 首次启动定位到小屏幕
- **WHEN** 应用首次启动（无配置文件）
- **THEN** 系统遍历所有显示器，找到分辨率最接近 1424x280 的屏幕，将窗口居中显示在该屏幕上

#### Scenario: 记住上次位置
- **WHEN** 用户将窗口拖到某个位置后关闭应用
- **THEN** 下次启动时窗口出现在上次关闭时的位置

#### Scenario: 上次位置的显示器已断开
- **WHEN** 上次保存的位置所在的显示器当前不可用
- **THEN** 窗口回落到主显示器可见区域内

### Requirement: 应用单实例运行
系统 SHALL 确保同一时间只有一个实例运行。重复启动 SHALL 激活已有实例的窗口。

#### Scenario: 重复启动
- **WHEN** 用户在应用已运行时再次启动
- **THEN** 已有实例的窗口被激活并置顶，新实例退出
