using MagicCenterHub.Effects;
using MagicCenterHub.Models;
using MagicCenterHub.Services;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Threading;

namespace MagicCenterHub.ViewModels;

/// <summary>
/// 主窗口 ViewModel，负责硬件数据绑定、LED 灯效控制、hook 消息处理
/// </summary>
public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly Settings _settings;
    private HardwareData _hwData = new();
    private LedMode _currentLedMode = LedMode.BothOff;
    private ILedEffect _ledEffect;
    private DateTime _ledStartTime = DateTime.Now;
    private bool _hwInfoConnected;
    private DispatcherTimer? _ledIdleTimer;

    // LED 颜色
    private Color _ledGreen = Colors.Black;

    private Color _ledRed = Colors.Black;
    private Color _ledYellow = Colors.Black;

    // CPU
    private string _cpuUsageText = "N/A";

    private string _cpuFreqText = "N/A";
    private string _cpuTempText = "N/A";
    private double _cpuUsagePercent;
    private double _cpuTempPercent;
    private Brush _cpuUsageColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));
    private Brush _cpuTempColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));
    private Brush _cpuUsageBarColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));
    private Brush _cpuTempBarColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));

    // GPU
    private string _gpuUsageText = "N/A";

    private string _gpuFreqText = "N/A";
    private string _gpuTempText = "N/A";
    private double _gpuUsagePercent;
    private double _gpuTempPercent;
    private Brush _gpuUsageColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));
    private Brush _gpuTempColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));
    private Brush _gpuUsageBarColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));
    private Brush _gpuTempBarColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));

    // 主机
    private string _memoryText = "N/A";

    private string _batteryText = "N/A";
    private double _memoryPercent;
    private double _batteryPercent;
    private Brush _memoryColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));
    private Brush _batteryColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));
    private Brush _memoryBarColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));
    private Brush _batteryBarColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));

    // 磁盘
    private string _diskReadText = "N/A";

    private string _diskWriteText = "N/A";

    // 网络
    private string _netUpText = "N/A";

    private string _netDownText = "N/A";

    // HWiNFO 连接状态
    private string _hwInfoStatusText = "HWiNFO64...";

    private Brush _hwInfoStatusColor = new SolidColorBrush(Color.FromRgb(0xF0, 0xA8, 0x40));

    /// <summary>
    /// 属性变化事件
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 初始化 ViewModel
    /// </summary>
    public MainViewModel(Settings settings)
    {
        _settings = settings;

        // 应用默认灯效
        LedMode defaultMode = (LedMode)settings.DefaultLedMode;
        if (settings.DefaultLedMode >= 0 && settings.DefaultLedMode <= 19)
        {
            _currentLedMode = defaultMode;
        }
        _ledEffect = LedEffectFactory.Create(_currentLedMode);

        // 初始化空闲恢复定时器
        if (settings.LedIdleRestoreSeconds > 0)
        {
            _ledIdleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(settings.LedIdleRestoreSeconds)
            };
            _ledIdleTimer.Tick += OnLedIdleTimeout;
        }
    }

    /// <summary>
    /// 更新硬件数据并刷新所有绑定属性
    /// </summary>
    public void UpdateHardwareData(HardwareData data)
    {
        _hwData = data;

        if (data.CpuUsageValid) // CPU
        {
            CpuUsageText = $"{data.CpuUsage:F1}%";
            CpuUsagePercent = data.CpuUsage;
            CpuUsageColor = GetBrushForValue(data.CpuUsage, _settings.ColorThresholds.UsageGreen, _settings.ColorThresholds.UsageYellow);
            CpuUsageBarColor = CpuUsageColor;
        }
        else
        {
            CpuUsageText = "N/A";
            CpuUsagePercent = 0;
            CpuUsageColor = Brushes.Gray;
            CpuUsageBarColor = Brushes.Transparent;
        }

        if (data.CpuFreqValid) // CPU 频率
            CpuFreqText = $"{data.CpuFreqMHz:F0} MHz";
        else
            CpuFreqText = "N/A";

        if (data.CpuTempValid)
        {
            CpuTempText = $"{data.CpuTemp:F0}°C";
            CpuTempPercent = _settings.CpuMaxTempC > 0 ? data.CpuTemp / _settings.CpuMaxTempC * 100 : 0;
            CpuTempColor = GetBrushForValue(data.CpuTemp, _settings.ColorThresholds.TempGreen, _settings.ColorThresholds.TempYellow);
            CpuTempBarColor = CpuTempColor;
        }
        else
        {
            CpuTempText = "N/A";
            CpuTempPercent = 0;
            CpuTempColor = Brushes.Gray;
            CpuTempBarColor = Brushes.Transparent;
        }

        if (data.GpuUsageValid) // GPU
        {
            GpuUsageText = $"{data.GpuUsage:F1}%";
            GpuUsagePercent = data.GpuUsage;
            GpuUsageColor = GetBrushForValue(data.GpuUsage, _settings.ColorThresholds.UsageGreen, _settings.ColorThresholds.UsageYellow);
            GpuUsageBarColor = GpuUsageColor;
        }
        else
        {
            GpuUsageText = "N/A";
            GpuUsagePercent = 0;
            GpuUsageColor = Brushes.Gray;
            GpuUsageBarColor = Brushes.Transparent;
        }

        if (data.GpuFreqValid) // GPU 频率
            GpuFreqText = $"{data.GpuFreqMHz:F0} MHz";
        else
            GpuFreqText = "N/A";

        if (data.GpuTempValid)
        {
            GpuTempText = $"{data.GpuTemp:F0}°C";
            GpuTempPercent = _settings.GpuMaxTempC > 0 ? data.GpuTemp / _settings.GpuMaxTempC * 100 : 0;
            GpuTempColor = GetBrushForValue(data.GpuTemp, _settings.ColorThresholds.TempGreen, _settings.ColorThresholds.TempYellow);
            GpuTempBarColor = GpuTempColor;
        }
        else
        {
            GpuTempText = "N/A";
            GpuTempPercent = 0;
            GpuTempColor = Brushes.Gray;
            GpuTempBarColor = Brushes.Transparent;
        }

        if (data.MemoryValid && data.MemoryTotalGB > 0) // 内存
        {
            double memPercent = data.MemoryUsedGB / data.MemoryTotalGB * 100;
            MemoryText = $"{data.MemoryUsedGB:F1} GB ({memPercent:F1}%)";
            MemoryPercent = memPercent;
            MemoryColor = GetBrushForValue(MemoryPercent, _settings.ColorThresholds.UsageGreen, _settings.ColorThresholds.UsageYellow);
            MemoryBarColor = MemoryColor;
        }
        else if (data.MemoryValid)
        {
            // 只有百分比，没有总量
            MemoryText = $"{data.MemoryUsedGB:F1}%";
            MemoryPercent = data.MemoryUsedGB;
            MemoryColor = GetBrushForValue(MemoryPercent, _settings.ColorThresholds.UsageGreen, _settings.ColorThresholds.UsageYellow);
            MemoryBarColor = MemoryColor;
        }
        else
        {
            MemoryText = "N/A";
            MemoryPercent = 0;
            MemoryColor = Brushes.Gray;
            MemoryBarColor = Brushes.Transparent;
        }

        if (data.BatteryValid) // 电池功耗
        {
            BatteryText = $"{data.BatteryWatt:F2} W";
            BatteryPercent = Math.Min(data.BatteryWatt / 100 * 100, 100);
        }
        else
        {
            BatteryText = "0.00 W";
            BatteryPercent = 0;
        }
        BatteryColor = GetBrushForValue(data.BatteryWatt, 30, 60);
        BatteryBarColor = BatteryColor;

        // 磁盘
        DiskReadText = data.DiskValid ? FormatBytesPerSec(data.DiskReadBps) : "N/A";
        DiskWriteText = data.DiskValid ? FormatBytesPerSec(data.DiskWriteBps) : "N/A";

        // 网络
        NetUpText = data.NetValid ? FormatBytesPerSec(data.NetUpBps) : "N/A";
        NetDownText = data.NetValid ? FormatBytesPerSec(data.NetDownBps) : "N/A";
    }

    /// <summary>
    /// 更新 HWiNFO 连接状态显示
    /// </summary>
    public void UpdateHwInfoStatus(bool connected)
    {
        _hwInfoConnected = connected;
        if (connected)
        {
            HwInfoStatusText = "HWiNFO64";
            HwInfoStatusColor = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));
        }
        else
        {
            HwInfoStatusText = "HWiNFO64";
            HwInfoStatusColor = new SolidColorBrush(Color.FromRgb(0xF0, 0xA8, 0x40));
        }
    }

    /// <summary>
    /// 切换 LED 灯效模式
    /// </summary>
    public void SetLedMode(LedMode mode)
    {
        _currentLedMode = mode;
        _ledEffect = LedEffectFactory.Create(mode);
        _ledStartTime = DateTime.Now;

        // 重置空闲定时器
        ResetLedIdleTimer();
    }

    /// <summary>
    /// 重置 LED 空闲恢复定时器
    /// </summary>
    private void ResetLedIdleTimer()
    {
        if (_ledIdleTimer == null) return;

        _ledIdleTimer.Stop();
        _ledIdleTimer.Start();
    }

    /// <summary>
    /// LED 空闲超时，恢复为默认灯效
    /// </summary>
    private void OnLedIdleTimeout(object? sender, EventArgs e)
    {
        _ledIdleTimer?.Stop();

        LedMode defaultMode = (LedMode)_settings.DefaultLedMode;
        if (_settings.DefaultLedMode >= 0 && _settings.DefaultLedMode <= 19)
        {
            _currentLedMode = defaultMode;
            _ledEffect = LedEffectFactory.Create(defaultMode);
            _ledStartTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 更新 LED 灯效帧（每帧调用，由 CompositionTarget.Rendering 驱动）
    /// </summary>
    public void UpdateLed()
    {
        long elapsedMs = (long)(DateTime.Now - _ledStartTime).TotalMilliseconds;
        LedColor color = _ledEffect.Update(elapsedMs);
        LedGreen = color.Green;
        LedRed = color.Red;
        LedYellow = color.Yellow;
    }

    /// <summary>
    /// 处理命名管道 hook 消息：设置灯效
    /// </summary>
    public void HandleHookMessage(HookMessage msg)
    {
        if (msg.LedMode >= 0 && msg.LedMode <= 19)
            SetLedMode((LedMode)msg.LedMode);
        else
            SetLedMode(LedMode.Taichi);
    }

    private static string FormatBytesPerSec(double bps)
    {
        if (bps < 1024)
            return $"{bps:F0} B/s";
        if (bps < 1024 * 1024)
            return $"{bps / 1024:F1} KB/s";

        return $"{bps / (1024 * 1024):F1} MB/s";
    }

    private static SolidColorBrush GetBrushForValue(double value, double greenThreshold, double yellowThreshold)
    {
        if (value < greenThreshold)
            return new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F)); // 绿
        if (value < yellowThreshold)
            return new SolidColorBrush(Color.FromRgb(0xF0, 0xA8, 0x40)); // 琥珀
        return new SolidColorBrush(Color.FromRgb(0xE0, 0x40, 0x50)); // 红
    }

    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>
    /// 绿灯当前颜色
    /// </summary>
    public Color LedGreen
    {
        get => _ledGreen;
        set { _ledGreen = value; OnPropertyChanged(nameof(LedGreen)); }
    }

    /// <summary>
    /// 红灯当前颜色
    /// </summary>
    public Color LedRed
    {
        get => _ledRed;
        set { _ledRed = value; OnPropertyChanged(nameof(LedRed)); }
    }

    /// <summary>
    /// 黄灯当前颜色
    /// </summary>
    public Color LedYellow
    {
        get => _ledYellow;
        set { _ledYellow = value; OnPropertyChanged(nameof(LedYellow)); }
    }

    /// <summary>
    /// CPU 使用率文本
    /// </summary>
    public string CpuUsageText
    {
        get => _cpuUsageText;
        set { _cpuUsageText = value; OnPropertyChanged(nameof(CpuUsageText)); }
    }

    /// <summary>
    /// CPU 频率文本
    /// </summary>
    public string CpuFreqText
    {
        get => _cpuFreqText;
        set { _cpuFreqText = value; OnPropertyChanged(nameof(CpuFreqText)); }
    }

    /// <summary>
    /// CPU 温度文本
    /// </summary>
    public string CpuTempText
    {
        get => _cpuTempText;
        set { _cpuTempText = value; OnPropertyChanged(nameof(CpuTempText)); }
    }

    /// <summary>
    /// CPU 使用率百分比（0-100）
    /// </summary>
    public double CpuUsagePercent
    {
        get => _cpuUsagePercent;
        set { _cpuUsagePercent = value; OnPropertyChanged(nameof(CpuUsagePercent)); }
    }

    /// <summary>
    /// CPU 温度百分比（进度条用）
    /// </summary>
    public double CpuTempPercent
    {
        get => _cpuTempPercent;
        set { _cpuTempPercent = value; OnPropertyChanged(nameof(CpuTempPercent)); }
    }

    /// <summary>
    /// CPU 使用率文本颜色
    /// </summary>
    public Brush CpuUsageColor
    {
        get => _cpuUsageColor;
        set { _cpuUsageColor = value; OnPropertyChanged(nameof(CpuUsageColor)); }
    }

    /// <summary>
    /// CPU 温度文本颜色
    /// </summary>
    public Brush CpuTempColor
    {
        get => _cpuTempColor;
        set { _cpuTempColor = value; OnPropertyChanged(nameof(CpuTempColor)); }
    }

    /// <summary>
    /// CPU 使用率进度条颜色
    /// </summary>
    public Brush CpuUsageBarColor
    {
        get => _cpuUsageBarColor;
        set { _cpuUsageBarColor = value; OnPropertyChanged(nameof(CpuUsageBarColor)); }
    }

    /// <summary>
    /// CPU 温度进度条颜色
    /// </summary>
    public Brush CpuTempBarColor
    {
        get => _cpuTempBarColor;
        set { _cpuTempBarColor = value; OnPropertyChanged(nameof(CpuTempBarColor)); }
    }

    /// <summary>
    /// GPU 使用率文本
    /// </summary>
    public string GpuUsageText
    {
        get => _gpuUsageText;
        set { _gpuUsageText = value; OnPropertyChanged(nameof(GpuUsageText)); }
    }

    /// <summary>
    /// GPU 频率文本
    /// </summary>
    public string GpuFreqText
    {
        get => _gpuFreqText;
        set { _gpuFreqText = value; OnPropertyChanged(nameof(GpuFreqText)); }
    }

    /// <summary>
    /// GPU 温度文本
    /// </summary>
    public string GpuTempText
    {
        get => _gpuTempText;
        set { _gpuTempText = value; OnPropertyChanged(nameof(GpuTempText)); }
    }

    /// <summary>
    /// GPU 使用率百分比（0-100）
    /// </summary>
    public double GpuUsagePercent
    {
        get => _gpuUsagePercent;
        set { _gpuUsagePercent = value; OnPropertyChanged(nameof(GpuUsagePercent)); }
    }

    /// <summary>
    /// GPU 温度百分比（进度条用）
    /// </summary>
    public double GpuTempPercent
    {
        get => _gpuTempPercent;
        set { _gpuTempPercent = value; OnPropertyChanged(nameof(GpuTempPercent)); }
    }

    /// <summary>
    /// GPU 使用率文本颜色
    /// </summary>
    public Brush GpuUsageColor
    {
        get => _gpuUsageColor;
        set { _gpuUsageColor = value; OnPropertyChanged(nameof(GpuUsageColor)); }
    }

    /// <summary>
    /// GPU 温度文本颜色
    /// </summary>
    public Brush GpuTempColor
    {
        get => _gpuTempColor;
        set { _gpuTempColor = value; OnPropertyChanged(nameof(GpuTempColor)); }
    }

    /// <summary>
    /// GPU 使用率进度条颜色
    /// </summary>
    public Brush GpuUsageBarColor
    {
        get => _gpuUsageBarColor;
        set { _gpuUsageBarColor = value; OnPropertyChanged(nameof(GpuUsageBarColor)); }
    }

    /// <summary>
    /// GPU 温度进度条颜色
    /// </summary>
    public Brush GpuTempBarColor
    {
        get => _gpuTempBarColor;
        set { _gpuTempBarColor = value; OnPropertyChanged(nameof(GpuTempBarColor)); }
    }

    /// <summary>
    /// 内存使用文本
    /// </summary>
    public string MemoryText
    {
        get => _memoryText;
        set { _memoryText = value; OnPropertyChanged(nameof(MemoryText)); }
    }

    /// <summary>
    /// 电池功耗文本
    /// </summary>
    public string BatteryText
    {
        get => _batteryText;
        set { _batteryText = value; OnPropertyChanged(nameof(BatteryText)); }
    }

    /// <summary>
    /// 内存使用百分比
    /// </summary>
    public double MemoryPercent
    {
        get => _memoryPercent;
        set { _memoryPercent = value; OnPropertyChanged(nameof(MemoryPercent)); }
    }

    /// <summary>
    /// 电池百分比
    /// </summary>
    public double BatteryPercent
    {
        get => _batteryPercent;
        set { _batteryPercent = value; OnPropertyChanged(nameof(BatteryPercent)); }
    }

    /// <summary>
    /// 内存文本颜色
    /// </summary>
    public Brush MemoryColor
    {
        get => _memoryColor;
        set { _memoryColor = value; OnPropertyChanged(nameof(MemoryColor)); }
    }

    /// <summary>
    /// 电池文本颜色
    /// </summary>
    public Brush BatteryColor
    {
        get => _batteryColor;
        set { _batteryColor = value; OnPropertyChanged(nameof(BatteryColor)); }
    }

    /// <summary>
    /// 内存进度条颜色
    /// </summary>
    public Brush MemoryBarColor
    {
        get => _memoryBarColor;
        set { _memoryBarColor = value; OnPropertyChanged(nameof(MemoryBarColor)); }
    }

    /// <summary>
    /// 电池进度条颜色
    /// </summary>
    public Brush BatteryBarColor
    {
        get => _batteryBarColor;
        set { _batteryBarColor = value; OnPropertyChanged(nameof(BatteryBarColor)); }
    }

    /// <summary>
    /// 磁盘读取速度文本
    /// </summary>
    public string DiskReadText
    {
        get => _diskReadText;
        set { _diskReadText = value; OnPropertyChanged(nameof(DiskReadText)); }
    }

    /// <summary>
    /// 磁盘写入速度文本
    /// </summary>
    public string DiskWriteText
    {
        get => _diskWriteText;
        set { _diskWriteText = value; OnPropertyChanged(nameof(DiskWriteText)); }
    }

    /// <summary>
    /// 网络上传速度文本
    /// </summary>
    public string NetUpText
    {
        get => _netUpText;
        set { _netUpText = value; OnPropertyChanged(nameof(NetUpText)); }
    }

    /// <summary>
    /// 网络下载速度文本
    /// </summary>
    public string NetDownText
    {
        get => _netDownText;
        set { _netDownText = value; OnPropertyChanged(nameof(NetDownText)); }
    }

    /// <summary>
    /// HWiNFO 连接状态文本
    /// </summary>
    public string HwInfoStatusText
    {
        get => _hwInfoStatusText;
        set { _hwInfoStatusText = value; OnPropertyChanged(nameof(HwInfoStatusText)); }
    }

    /// <summary>
    /// HWiNFO 连接状态颜色
    /// </summary>
    public Brush HwInfoStatusColor
    {
        get => _hwInfoStatusColor;
        set { _hwInfoStatusColor = value; OnPropertyChanged(nameof(HwInfoStatusColor)); }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _ledIdleTimer?.Stop();
        _ledIdleTimer = null;
    }
}