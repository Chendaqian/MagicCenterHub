using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace MagicCenterHub.Services;

/// <summary>
/// 命名管道 hook 消息模型
/// </summary>
public class HookMessage
{
    /// <summary>
    /// LED 灯效模式编号（0-19）
    /// </summary>
    public int LedMode { get; set; }
}

/// <summary>
/// 命名管道监听服务，接收 Claude Code hook 消息
/// </summary>
public class NamedPipeListenerService : IDisposable
{
    private const string PipeName = "ClaudeCodeMagicCenterHub";
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    /// <summary>
    /// 收到 hook 消息时触发
    /// </summary>
    public event Action<HookMessage>? HookMessageReceived;

    /// <summary>
    /// 启动管道监听
    /// </summary>
    public void Start()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _cts = new CancellationTokenSource();
        Task.Run(() => ListenLoop(_cts.Token));
    }

    private async Task ListenLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                // 设置管道安全属性，允许所有用户连接
                PipeSecurity pipeSecurity = new PipeSecurity();
                pipeSecurity.AddAccessRule(new PipeAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    PipeAccessRights.ReadWrite,
                    AccessControlType.Allow));

                await using NamedPipeServerStream server = NamedPipeServerStreamAcl.Create(
                    PipeName,
                    PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    0, 0, pipeSecurity);

                await server.WaitForConnectionAsync(ct);

                using StreamReader reader = new StreamReader(server, System.Text.Encoding.UTF8);
                string? line = await reader.ReadLineAsync(ct);

                if (!string.IsNullOrWhiteSpace(line))
                    ParseMessage(line);

                // 断开连接，准备接受下一个请求
                server.Disconnect();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch // 管道异常，短暂等待后重试
            {
                await Task.Delay(1000, ct);
            }
        }
    }

    private void ParseMessage(string json)
    {
        try
        {
            JObject obj = JObject.Parse(json);

            HookMessage msg = new HookMessage
            {
                LedMode = obj["ledMode"]?.ToObject<int>() ?? 0
            };

            HookMessageReceived?.Invoke(msg);
        }
        catch // JSON 解析失败，忽略
        {
        }
    }

    /// <summary>
    /// 停止监听并释放资源
    /// </summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _isRunning = false;
        GC.SuppressFinalize(this);
    }
}