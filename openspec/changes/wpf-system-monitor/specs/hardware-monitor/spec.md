## ADDED Requirements

### Requirement: CPU 使用率与温度采集
系统 SHALL 以不低于每秒 1 次的频率从 HWiNFO 共享内存采集 CPU 总体使用率和 CPU 温度。CPU 使用率 SHALL 以百分比显示（0-100%），温度 SHALL 以摄氏度显示。

#### Scenario: 正常采集 CPU 数据
- **WHEN** 应用运行且 HWiNFO64 共享内存可访问
- **THEN** UI 显示 CPU 使用率百分比（如 "29.6%"）和温度（如 "45.0°C"），进度条同步更新

#### Scenario: HWiNFO64 未运行
- **WHEN** HWiNFO 共享内存不存在
- **THEN** CPU 数据显示灰色 "N/A"，进度条为 0%

### Requirement: GPU 使用率与温度采集
系统 SHALL 从 HWiNFO 共享内存采集 GPU 使用率和 GPU 温度。支持 NVIDIA、AMD、Intel 集成显卡。

#### Scenario: 正常采集 GPU 数据
- **WHEN** HWiNFO64 正在运行且检测到 GPU 传感器
- **THEN** UI 显示 GPU 使用率百分比和温度，进度条同步更新

#### Scenario: 仅有集成显卡
- **WHEN** 系统仅有 Intel 集成显卡（如 Core Ultra 358H）
- **THEN** HWiNFO 仍能采集并显示 iGPU 使用率和温度

### Requirement: 内存占用显示
系统 SHALL 从 HWiNFO 共享内存读取当前已使用的物理内存大小（GB）和总内存占比。精度保留一位小数。

#### Scenario: 正常显示内存
- **WHEN** 系统有 64GB 内存，已使用 30.5GB
- **THEN** UI 显示 "30.5 GB"，进度条约 47.7%

### Requirement: 电池功耗显示
系统 SHALL 从 HWiNFO 共享内存读取当前电池放电功率（瓦特）。当无电池或 HWiNFO 未检测到电池传感器时，显示 "0.00 W"。

#### Scenario: 笔记本有电池
- **WHEN** 系统有电池且正在放电
- **THEN** UI 显示当前功耗（如 "15.3 W"）

#### Scenario: 台式机无电池
- **WHEN** 系统无电池
- **THEN** UI 显示 "0.00 W"，进度条为 0%

### Requirement: 磁盘读写速率显示
系统 SHALL 从 HWiNFO 共享内存读取磁盘读取速率和写入速率。单位自动选择 KB/s 或 MB/s。

#### Scenario: 磁盘有读写活动
- **WHEN** 磁盘正在写入数据
- **THEN** UI 显示读取和写入速率（如 "0.00 KB/s" / "4.07 MB/s"）

### Requirement: 网络上下行速率显示
系统 SHALL 从 HWiNFO 共享内存读取网络上传速率和下载速率。单位自动选择 KB/s 或 MB/s。

#### Scenario: 网络有数据传输
- **WHEN** 系统有活跃的网络连接
- **THEN** UI 显示上传和下载速率（如 "0.24 KB/s" / "87.7 KB/s"）

### Requirement: UI 布局与设计稿一致
系统 SHALL 按照 `system_monitor_v3.html` 设计稿的布局渲染。窗口尺寸为 1424x280 像素。背景色、面板色、字体、圆角等视觉元素 SHALL 与设计稿一致。

#### Scenario: 窗口首次显示
- **WHEN** 应用启动
- **THEN** 显示 1424x280 的无边框窗口，包含左侧 LED 灯柱和右侧四个面板，配色与设计稿一致

### Requirement: 用户硬件参数配置
系统 SHALL 在配置文件 `%AppData%\MagicCenterHub\settings.json` 中支持用户配置以下硬件参数：

```json
{
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

- `cpuFrequencyGHz`: CPU 主频（GHz），用于信息展示
- `cpuMaxTempC`: CPU 最高温度（°C），决定温度进度条的满量程
- `gpuMaxTempC`: GPU 最高温度（°C），决定温度进度条的满量程
- `colorThresholds`: 颜色切换阈值

#### Scenario: 用户配置 CPU 主频
- **WHEN** 用户在 settings.json 中设置 `"cpuFrequencyGHz": 5.0`
- **THEN** UI 信息展示区域显示 CPU 主频为 5.0GHz

#### Scenario: 用户配置 CPU 最高温度
- **WHEN** 用户设置 `"cpuMaxTempC": 95`，当前 CPU 温度 60°C
- **THEN** 温度进度条显示为 63.2%（60/95），颜色根据阈值判断

### Requirement: 进度条三档颜色编码
进度条和数值颜色 SHALL 根据当前值与阈值的比较动态变化，三档颜色：
- 绿色 (#6FCF6F): 低于 `Green` 阈值
- 琥珀/黄色 (#F0A840): 介于 `Green` 和 `Yellow` 阈值之间
- 粉/红色 (#F06080): 高于 `Yellow` 阈值

CPU/GPU 使用率使用 `usageGreen` / `usageYellow` 阈值（默认 50%/80%）。
CPU/GPU 温度使用 `tempGreen` / `tempYellow` 阈值（默认 60°C/80°C）。
内存使用率使用 `usageGreen` / `usageYellow` 阈值。

#### Scenario: CPU 使用率低
- **WHEN** CPU 使用率 < usageGreen（默认 50%）
- **THEN** 进度条填充色和数值均为绿色 #6FCF6F

#### Scenario: CPU 使用率中等
- **WHEN** CPU 使用率 >= usageGreen 且 < usageYellow（默认 50%-80%）
- **THEN** 进度条填充色和数值均为琥珀色 #F0A840

#### Scenario: CPU 使用率高
- **WHEN** CPU 使用率 >= usageYellow（默认 80%）
- **THEN** 进度条填充色和数值均为粉色 #F06080

#### Scenario: GPU 温度高
- **WHEN** GPU 温度 >= tempYellow（默认 80°C）
- **THEN** GPU 温度进度条和数值变为粉色，设计稿中 60.5°C 显示琥珀色即为此规则

#### Scenario: 用户自定义阈值
- **WHEN** 用户修改 settings.json 中的 `colorThresholds`
- **THEN** 所有进度条和数值颜色按新阈值判断

### Requirement: HWiNFO 启动检测
系统 SHALL 在启动时检查 HWiNFO64 共享内存 `Global\HWiNFO_SENS_SM2` 是否存在。若不存在，SHALL 弹出提示并每 5 秒重试。

#### Scenario: HWiNFO64 正在运行
- **WHEN** 用户已安装并运行 HWiNFO64，共享内存 `Global\HWiNFO_SENS_SM2` 可访问
- **THEN** 所有指标从 HWiNFO 共享内存读取，包括 CPU/GPU 温度、使用率、功耗等，数据完整

#### Scenario: HWiNFO64 未运行
- **WHEN** 应用启动时共享内存不存在
- **THEN** 弹出提示"请先安装并运行 HWiNFO64"，窗口正常显示但所有数据为 N/A，后台每 5 秒重试检测

#### Scenario: HWiNFO64 中途启动
- **WHEN** 应用已在运行且数据为 N/A，用户此时启动了 HWiNFO64
- **THEN** 下次重试检测到共享内存后，自动开始采集，数据从 N/A 切换为实际值
