## ADDED Requirements

### Requirement: LED 组件基本结构
系统 SHALL 在窗口左侧提供一个 130px 宽的 LED 灯柱区域，包含三个圆形指示灯：红灯（顶部）、黄灯（中部）、绿灯（底部）。每个灯 SHALL 有外圈、内圈、高光三层渲染以模拟真实 LED 质感。

#### Scenario: LED 组件初始状态
- **WHEN** 应用启动，未收到任何 hook 事件
- **THEN** 三个灯均处于全灭状态（模式 0）

### Requirement: 20 种灯效模式支持
系统 SHALL 实现 0-19 共 20 种灯效模式，每种模式的视觉效果和时序 SHALL 与 antigravity-task-led 项目的 Arduino 实现对齐。

#### Scenario: 模式 0 全灭
- **WHEN** 灯效模式设为 0
- **THEN** 红灯和绿灯均熄灭，亮度为 0

#### Scenario: 模式 1 同闪
- **WHEN** 灯效模式设为 1
- **THEN** 红灯和绿灯以 500ms 间隔同步闪烁（亮 500ms / 灭 500ms）

#### Scenario: 模式 7 警车交替快闪
- **WHEN** 灯效模式设为 7
- **THEN** 红灯和绿灯交替点亮，红亮时绿灭、绿亮时红灭，间隔 300ms

#### Scenario: 模式 8 心跳双闪
- **WHEN** 灯效模式设为 8
- **THEN** 双灯执行心跳节律：亮 80ms → 灭 100ms → 亮 80ms → 灭 600ms，循环

#### Scenario: 模式 9 SOS 求救信号
- **WHEN** 灯效模式设为 9
- **THEN** 双灯执行 SOS 摩尔斯码：三短(200ms) 三长(600ms) 三短(200ms)，循环间隔 1s

#### Scenario: 模式 10 交替呼吸灯
- **WHEN** 灯效模式设为 10
- **THEN** 红绿灯亮度呈反向线性交替渐变，8ms 步进，1023 级精度

#### Scenario: 模式 11 萤火虫混沌呼吸
- **WHEN** 灯效模式设为 11
- **THEN** 绿灯按 3000ms 周期正弦变化，红灯按 2200ms 周期正弦变化，双通道独立

#### Scenario: 模式 12 心电波模拟
- **WHEN** 灯效模式设为 12
- **THEN** 红灯模拟 ECG 波形（P 波、QRS 峰、T 波），1.2 秒周期；绿灯在 QRS 峰时同步暴闪

#### Scenario: 模式 16 太极阴阳呼吸
- **WHEN** 灯效模式设为 16
- **THEN** 红绿灯以三阶正弦 y=sin³(x) 对立消长，3 秒周期，顶点有滞留感

#### Scenario: 模式 18 HELLO 摩尔斯码广播
- **WHEN** 灯效模式设为 18
- **THEN** 双灯执行 H-E-L-L-O 摩尔斯码广播

#### Scenario: 模式 19 雷达扫描锁定
- **WHEN** 灯效模式设为 19
- **THEN** 绿灯 3 秒正弦扫描 → 红灯 1 秒高频锁定闪 → 双灯 0.5 秒全亮确认

### Requirement: 灯效 PWM 平滑度
所有涉及渐变的灯效 SHALL 实现至少 256 级亮度精度，渲染帧率不低于 30fps。呼吸、正弦等平滑灯效 SHALL 无明显阶梯感。

#### Scenario: 呼叫灯效渲染
- **WHEN** 模式 10（呼吸灯）运行中
- **THEN** 亮度变化平滑无阶梯，视觉上为连续渐变

### Requirement: 灯效模式可编程切换
系统 SHALL 提供接口接受外部命令切换灯效模式（模式编号 0-18）。切换 SHALL 立即生效，无需重启。

#### Scenario: 运行时切换灯效
- **WHEN** 当前灯效为模式 10，外部发送命令切换到模式 16
- **THEN** 灯效立即从呼吸灯切换到太极呼吸，无延迟或闪烁异常

### Requirement: 黄灯独立控制
黄灯（中间灯）SHALL 支持独立的亮灭控制，用于 THINKING 状态指示。黄灯 SHALL 支持常亮和闪烁两种状态。

#### Scenario: THINKING 状态黄灯闪烁
- **WHEN** Claude Code hook 发送 THINKING 事件
- **THEN** 黄灯以 500ms 间隔闪烁，红灯和绿灯不受影响
