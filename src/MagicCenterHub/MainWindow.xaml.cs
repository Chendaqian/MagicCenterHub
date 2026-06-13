using MagicCenterHub.Models;
using MagicCenterHub.Services;
using MagicCenterHub.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MagicCenterHub;

/// <summary>
/// 主窗口：硬件监控悬浮窗 + LED 灯效指示
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly HardwareMonitorService _hwMonitor;
    private readonly NamedPipeListenerService _pipeListener;
    private readonly Settings _settings;
    private bool _isLedRunning;

    /// <summary>
    /// 数据绑定 ViewModel
    /// </summary>
    public MainViewModel ViewModel => _viewModel;

    /// <summary>
    /// 当前配置
    /// </summary>
    public Settings Settings => _settings;

    /// <summary>
    /// 初始化主窗口
    /// </summary>
    public MainWindow(Settings settings)
    {
        InitializeComponent();
        _settings = settings;
        _viewModel = new MainViewModel(settings);
        DataContext = _viewModel;

        _hwMonitor = new HardwareMonitorService();
        _pipeListener = new NamedPipeListenerService();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Topmost = _settings.WindowTopMost;
        RestoreWindowPosition();

        // 硬件采集
        _hwMonitor.DataUpdated += data =>
        {
            _viewModel.UpdateHardwareData(data);
        };
        _hwMonitor.ConnectionChanged += connected =>
        {
            Dispatcher.Invoke(() => _viewModel.UpdateHwInfoStatus(connected));
        };
        _hwMonitor.Start(_settings.PollIntervalMs);

        // 命名管道
        _pipeListener.HookMessageReceived += (msg) =>
        {
            Dispatcher.Invoke(() => _viewModel.HandleHookMessage(msg));
        };
        _pipeListener.Start();

        // LED 动画循环
        StartLedAnimation();
    }

    private void StartLedAnimation()
    {
        if (_isLedRunning) return;
        _isLedRunning = true;
        CompositionTarget.Rendering += OnRendering;
    }

    private void StopLedAnimation()
    {
        if (!_isLedRunning) return;
        _isLedRunning = false;
        CompositionTarget.Rendering -= OnRendering;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (IsVisible)
            _viewModel.UpdateLed();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        SaveWindowPosition();
        e.Cancel = true;
        Hide();
    }

    /// <summary>
    /// 退出应用程序，释放资源并关闭进程
    /// </summary>
    public void ExitApp()
    {
        SaveWindowPosition();
        StopLedAnimation();
        _hwMonitor.Dispose();
        _pipeListener.Dispose();
        Application.Current.Shutdown();
    }

    private void RestoreWindowPosition()
    {
        // 优先使用保存的位置（用户手动调整过的）
        if (!double.IsNaN(_settings.WindowLeft) && !double.IsNaN(_settings.WindowTop))
        {
            Left = _settings.WindowLeft;
            Top = _settings.WindowTop;
            return;
        }

        // 首次启动：找最接近窗口尺寸的小屏幕
        System.Windows.Forms.Screen? bestScreen = FindBestScreen();
        if (bestScreen != null)
        {
            System.Drawing.Rectangle area = bestScreen.WorkingArea;
            Left = area.Left;
            Top = area.Top;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }

    private System.Windows.Forms.Screen? FindBestScreen()
    {
        System.Windows.Forms.Screen? bestScreen = null;
        double bestDiff = double.MaxValue;

        foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
        {
            double diff = Math.Abs(screen.Bounds.Width - Width) + Math.Abs(screen.Bounds.Height - Height);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestScreen = screen;
            }
        }
        return bestScreen;
    }

    private void SaveWindowPosition()
    {
        _settings.WindowLeft = Left;
        _settings.WindowTop = Top;
        SettingsService.Save(_settings);
    }

    /// <summary>
    /// 应用新配置（窗口置顶、采集间隔等）
    /// </summary>
    public void ApplySettings(Settings newSettings)
    {
        Topmost = newSettings.WindowTopMost;
        _hwMonitor.UpdateInterval(newSettings.PollIntervalMs);
    }

    /// <summary>
    /// 移动窗口到主屏幕（笔记本屏幕）中心
    /// </summary>
    public void MoveToPrimaryScreenCenter()
    {
        System.Windows.Forms.Screen? primary = System.Windows.Forms.Screen.PrimaryScreen;
        if (primary == null) return;

        System.Drawing.Rectangle area = primary.WorkingArea;
        double scale = PresentationSource.FromVisual(this)?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
        Left = area.Left / scale + (area.Width / scale - Width) / 2;
        Top = area.Top / scale + (area.Height / scale - Height) / 2;
        SaveWindowPosition();
    }

    /// <summary>
    /// 移动窗口到指定预设位置
    /// </summary>
    public void MoveToPosition(double left, double top)
    {
        Left = left;
        Top = top;
        SaveWindowPosition();
    }
}