## ADDED Requirements

### Requirement: 设置窗口提供左侧标签页导航

设置窗口 SHALL 使用左侧标签页导航承载设置内容，并提供“通用”“窗口位置”“硬件监控”“LED 灯效”“关于”五个标签页，用户 SHALL 能在标签页之间切换。

#### Scenario: 打开设置窗口
- **WHEN** 用户打开设置窗口
- **THEN** 窗口显示左侧标签页导航，并显示五个指定标签页

#### Scenario: 切换标签页
- **WHEN** 用户选择任一标签页
- **THEN** 内容区域切换到该标签页对应的设置项，其他标签页内容不覆盖当前内容

### Requirement: 设置项按职责归入标签页

设置窗口 SHALL 按以下归属显示现有设置项：通用包含开机自启动和窗口置顶；窗口位置包含窗口坐标、取当前和位置预设管理；硬件监控包含轮询间隔、CPU/GPU 温度上限及使用率/温度颜色阈值；LED 灯效包含默认灯效模式和空闲恢复时间。

#### Scenario: 查看通用设置
- **WHEN** 用户选择“通用”标签页
- **THEN** 页面显示开机自启动和窗口置顶，且不显示硬件、位置预设或 LED 灯效设置

#### Scenario: 查看窗口位置设置
- **WHEN** 用户选择“窗口位置”标签页
- **THEN** 页面显示窗口坐标、取当前按钮和位置预设的新增、列表、跳转、删除及排序操作

#### Scenario: 查看硬件监控设置
- **WHEN** 用户选择“硬件监控”标签页
- **THEN** 页面显示轮询间隔、CPU/GPU 温度上限以及两类颜色阈值

#### Scenario: 查看 LED 灯效设置
- **WHEN** 用户选择“LED 灯效”标签页
- **THEN** 页面显示默认灯效模式、空闲恢复时间及其现有说明

### Requirement: 标签页布局不改变设置保存语义

设置窗口 SHALL 在标签页外提供固定可见的“应用”和“保存”操作；用户在一个或多个标签页修改设置后执行保存操作时，系统 SHALL 按现有校验规则保存所有设置字段。

#### Scenario: 跨标签页应用设置
- **WHEN** 用户在两个或多个标签页修改有效值并点击“应用”
- **THEN** 所有有效修改被保存，窗口保持打开，并显示已保存状态

#### Scenario: 跨标签页保存并关闭
- **WHEN** 用户在两个或多个标签页修改有效值并点击“保存”
- **THEN** 所有有效修改被保存，窗口关闭

### Requirement: 位置预设操作保持可用

“窗口位置”标签页 SHALL 保留现有位置预设的保存当前位置、跳转、删除和拖拽排序操作，并在操作后更新预设列表和相关状态。

#### Scenario: 管理位置预设
- **WHEN** 用户在“窗口位置”标签页保存、跳转、删除或拖拽调整一个位置预设
- **THEN** 对应操作成功完成，列表反映最新结果，并保留现有状态提示和托盘菜单刷新行为

### Requirement: 关于页展示版本和项目信息

“关于”标签页 SHALL 显示当前程序版本、项目 GitHub Releases 超链接、作者 GitHub Profile 超链接、当前使用的技术栈以及开源组件信息。项目 Release 链接 SHALL 指向 `https://github.com/ChenDaqian/MagicCenterHub/releases/latest`，作者链接 SHALL 指向 `https://github.com/Chendaqian`。

#### Scenario: 查看关于信息
- **WHEN** 用户选择“关于”标签页
- **THEN** 页面显示当前程序版本、项目 Release 链接、作者主页、.NET 8/WPF、HWiNFO 共享内存、Newtonsoft.Json、System.Text.Encoding.CodePages 等技术栈或组件信息

#### Scenario: 打开项目或作者链接
- **WHEN** 用户点击项目 Release 超链接或作者 GitHub Profile 超链接
- **THEN** 系统使用默认浏览器打开对应 URL

### Requirement: 手动检查 GitHub Release 更新

“关于”标签页 SHALL 提供检查更新按钮。系统 SHALL 查询项目 GitHub 最新 Release，并将其版本与当前程序版本进行比较；发现远程版本更高时 SHALL 提醒用户有新版本并打开 Release 页面，版本相同时或当前版本更高时 SHALL 提示当前无需更新。

#### Scenario: 发现新版本
- **WHEN** 用户点击检查更新且 GitHub 最新 Release 版本高于当前版本
- **THEN** 系统提示发现新版本，并使用默认浏览器打开该 Release 页面

#### Scenario: 已是最新版本
- **WHEN** 用户点击检查更新且 GitHub 最新 Release 版本不高于当前版本
- **THEN** 系统提示当前已是最新版本，无需更新

#### Scenario: 更新检查失败
- **WHEN** 用户点击检查更新且 GitHub API 与 `releases/latest` 页面均请求失败、无有效版本或版本无法解析
- **THEN** 系统提示无法完成更新检查，且不打开无效链接
