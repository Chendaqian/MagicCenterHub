using MagicCenterHub.Models;

namespace MagicCenterHub.Services;

/// <summary>
/// 硬件监控服务，定时从 HWiNFO 采集硬件数据
/// </summary>
public class HardwareMonitorService : IDisposable
{
    private readonly HwInfoProvider _hwInfo = new();
    private System.Windows.Threading.DispatcherTimer? _timer;
    private bool _isConnected;

    /// <summary>
    /// 硬件数据更新时触发
    /// </summary>
    public event Action<HardwareData>? DataUpdated;

    /// <summary>
    /// HWiNFO 连接状态变化时触发
    /// </summary>
    public event Action<bool>? ConnectionChanged;

    /// <summary>
    /// 启动定时采集
    /// </summary>
    /// <param name="intervalMs">采集间隔（毫秒），最小 500</param>
    public void Start(int intervalMs = 3000)
    {
        TryConnect();
        _timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(Math.Max(500, intervalMs))
        };
        _timer.Tick += OnTick;
        _timer.Start();
    }

    /// <summary>
    /// 动态更新采集间隔
    /// </summary>
    /// <param name="intervalMs">新的采集间隔（毫秒），最小 500</param>
    public void UpdateInterval(int intervalMs)
    {
        if (_timer != null)
            _timer.Interval = TimeSpan.FromMilliseconds(Math.Max(500, intervalMs));
    }

    private void TryConnect()
    {
        bool wasConnected = _isConnected;
        _isConnected = _hwInfo.TryConnect();
        if (wasConnected != _isConnected)
            ConnectionChanged?.Invoke(_isConnected);
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (!_isConnected)
        {
            TryConnect();
            if (!_isConnected)
                return;
        }

        HardwareData data = _hwInfo.Read();
        DataUpdated?.Invoke(data);
    }

    /// <summary>
    /// 停止采集并释放资源
    /// </summary>
    public void Dispose()
    {
        _timer?.Stop();
        _hwInfo.Dispose();
        GC.SuppressFinalize(this);
    }
}
