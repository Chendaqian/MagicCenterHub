## ADDED Requirements

### Requirement: 命名管道服务器
系统 SHALL 在启动时创建命名管道服务器，管道名为 `\\.\pipe\ClaudeCodeMagicCenterHub`。管道 SHALL 支持多个客户端并发连接。

#### Scenario: 管道服务器启动
- **WHEN** 应用启动
- **THEN** 命名管道 `\\.\pipe\ClaudeCodeMagicCenterHub` 创建成功，等待客户端连接

#### Scenario: 外部 hook 连接
- **WHEN** Claude Code hook 脚本通过命名管道发送消息
- **THEN** 管道服务器接受连接并解析消息

### Requirement: Hook 事件解析
系统 SHALL 解析来自命名管道的 JSON 消息，识别 `event` 字段并映射到 LED 状态。

消息格式:
```json
{
  "event": "string",
  "tool": "string (optional)",
  "success": "boolean (optional)"
}
```

#### Scenario: 收到 PreToolUse 事件
- **WHEN** 管道收到 `{"event": "PreToolUse", "tool": "Bash"}`
- **THEN** LED 状态切换为 THINKING，黄灯闪烁

#### Scenario: 收到 PermissionRequest 事件
- **WHEN** 管道收到 `{"event": "PermissionRequest"}`
- **THEN** LED 状态切换为 WAITING_USER，红灯常亮

#### Scenario: 收到 PostToolUseFailure 事件
- **WHEN** 管道收到 `{"event": "PostToolUseFailure", "tool": "Bash", "success": false}`
- **THEN** LED 状态切换为 TOOL_ERROR，红灯闪烁（模式 3）

#### Scenario: 收到 Stop 事件
- **WHEN** 管道收到 `{"event": "Stop"}`
- **THEN** LED 状态切换为 TASK_COMPLETE，绿灯常亮（模式 16）

### Requirement: 事件到灯效映射表
系统 SHALL 维护以下默认映射关系，映射 SHALL 可通过配置文件覆盖：

| Hook 事件 | LED 状态 | 灯效模式 |
|-----------|---------|---------|
| PreToolUse | THINKING | 黄灯闪烁 |
| UserPromptSubmit | THINKING | 黄灯闪烁 |
| PermissionRequest | WAITING_USER | 3 (红灯闪) |
| Notification | WAITING_USER | 3 (红灯闪) |
| PostToolUseFailure | TOOL_ERROR | 5 (红灯常亮) |
| Stop | TASK_COMPLETE | 16 (太极呼吸) |

#### Scenario: 未知事件
- **WHEN** 管道收到 `{"event": "UnknownEvent"}`
- **THEN** 忽略该事件，LED 状态不变

### Requirement: 管道通信容错
系统 SHALL 处理管道通信中的异常情况，包括客户端断开、消息格式错误、管道被占用等。

#### Scenario: 客户端异常断开
- **WHEN** hook 脚本发送消息后立即断开连接
- **THEN** 服务器正常处理已接收的消息，不影响后续连接

#### Scenario: JSON 格式错误
- **WHEN** 管道收到非 JSON 格式的数据
- **THEN** 记录错误日志，LED 状态不变，管道继续监听

### Requirement: Claude Code Hook 脚本集成
系统 SHALL 提供 hook 调用示例脚本，展示如何在 Claude Code 的 `settings.json` 中配置 hooks 调用命名管道。

#### Scenario: 配置示例
- **WHEN** 用户查看项目文档
- **THEN** 提供完整的 `settings.json` hooks 配置示例和 PowerShell 调用脚本
