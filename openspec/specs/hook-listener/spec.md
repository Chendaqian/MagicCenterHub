## ADDED Requirements

### Requirement: 命名管道服务器
系统 SHALL 在启动时创建命名管道服务器，管道名为 `\\.\pipe\ClaudeCodeMagicCenterHub`。管道 SHALL 支持多个客户端并发连接。

#### Scenario: 管道服务器启动
- **WHEN** 应用启动
- **THEN** 命名管道 `\\.\pipe\ClaudeCodeMagicCenterHub` 创建成功，等待客户端连接

#### Scenario: 外部 hook 连接
- **WHEN** Claude Code hook 脚本通过命名管道发送消息
- **THEN** 管道服务器接受连接并解析消息

### Requirement: 灯效模式接收
系统 SHALL 解析来自命名管道的 JSON 消息，读取 `ledMode` 字段并切换 LED 灯效。

消息格式:
```json
{
  "ledMode": 17
}
```

- `ledMode`：灯效模式编号 0-19（必填），对应 `LedMode` 枚举

#### Scenario: 收到灯效切换命令
- **WHEN** 管道收到 `{"ledMode": 2}`
- **THEN** LED 灯效切换为模式 2（绿闪）

#### Scenario: 收到太极灯效
- **WHEN** 管道收到 `{"ledMode": 17}`
- **THEN** LED 灯效切换为模式 17（太极呼吸）

### Requirement: Hook 脚本集成
系统 SHALL 提供 PowerShell hook 脚本 `send-hook.ps1`，支持通过 `-LedMode` 参数发送灯效命令。

#### Scenario: hook 脚本发送灯效
- **WHEN** Claude Code 触发 PreToolUse 事件，hook 配置执行 `send-hook.ps1 -LedMode 2`
- **THEN** 脚本通过命名管道发送 `{"ledMode": 2}`，应用切换为绿闪

### Requirement: Claude Code Hook 配置示例
系统 SHALL 提供 `hooks/claude-code-settings-example.json`，展示 Claude Code hooks 的完整配置：

| Claude Code 事件 | LED 模式 | 说明 |
|------------------|----------|------|
| PreToolUse | 2 - 绿闪 | 工具调用中 |
| UserPromptSubmit | 2 - 绿闪 | 用户提交 |
| PermissionRequest | 4 - 黄闪 | 等待权限确认 |
| PostToolUseFailure | 3 - 红闪 | 工具执行失败 |
| Stop | 5 - 绿亮 | 任务完成 |
| SessionStart | 17 - 太极 | 会话开始 |
| SessionEnd | 17 - 太极 | 会话结束 |
| Notification | — | BurntToast 通知（通过 stdin 接收通知内容） |

#### Scenario: 配置示例
- **WHEN** 用户查看项目文档
- **THEN** 提供完整的 Claude Code hooks 配置示例和 PowerShell 调用脚本

#### Scenario: 通知事件处理
- **WHEN** Claude Code 触发 Notification 事件（如任务完成、需要权限确认等）
- **THEN** hook 脚本通过 stdin 接收 JSON 格式的通知内容，使用 BurntToast 弹出 Windows 通知

### Requirement: 默认灯效与空闲恢复
系统 SHALL 支持配置默认 LED 灯效模式和空闲自动恢复功能。

配置项：
- `DefaultLedMode`：默认灯效模式编号 0-19（默认 17 - 太极）
- `LedIdleRestoreSeconds`：空闲恢复时间（秒），0 表示不自动恢复（默认 0）

#### Scenario: 启动时应用默认灯效
- **WHEN** 应用启动
- **THEN** LED 灯效自动设置为 `DefaultLedMode` 配置的模式

#### Scenario: 空闲自动恢复
- **WHEN** `LedIdleRestoreSeconds` 配置为 30，LED 灯效切换为模式 2（绿闪）
- **AND** 30 秒内未收到新的灯效切换命令
- **THEN** LED 灯效自动恢复为 `DefaultLedMode` 配置的模式

#### Scenario: 空闲恢复被中断
- **WHEN** 空闲恢复定时器运行期间收到新的灯效切换命令
- **THEN** 定时器重置，重新开始计时

#### Scenario: 空闲恢复禁用
- **WHEN** `LedIdleRestoreSeconds` 配置为 0
- **THEN** LED 灯效保持在最后一个设置的模式，不自动恢复

### Requirement: 管道通信容错
系统 SHALL 处理管道通信中的异常情况，包括客户端断开、消息格式错误、管道被占用等。

#### Scenario: 客户端异常断开
- **WHEN** hook 脚本发送消息后立即断开连接
- **THEN** 服务器正常处理已接收的消息，不影响后续连接

#### Scenario: JSON 格式错误
- **WHEN** 管道收到非 JSON 格式的数据
- **THEN** 忽略该消息，LED 状态不变，管道继续监听

#### Scenario: ledMode 超出范围
- **WHEN** 管道收到 `{"ledMode": 99}`
- **THEN** 忽略该消息，LED 状态不变
