using MagicCenterHub.Models;
using MagicCenterHub.Services;
using MagicCenterHub.Utils;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MagicCenterHub;

/// <summary>
/// 设置窗口：开机自启、窗口位置、阈值配置
/// </summary>
public partial class SettingsWindow : Window
{
    private const string AppName = "MagicCenterHub";
    private const string RegRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private readonly Settings _settings;
    private readonly Action<Settings>? _onSaved;
    private readonly Action? _onPresetsChanged;
    private Point _dragStartPoint;
    private bool _isDragging;

    /// <summary>
    /// 初始化设置窗口
    /// </summary>
    /// <param name="settings">当前配置实例</param>
    /// <param name="onSaved">保存成功后的回调</param>
    /// <param name="onPresetsChanged">预设变化后的回调（用于刷新托盘菜单）</param>
    public SettingsWindow(Settings settings, Action<Settings>? onSaved = null, Action? onPresetsChanged = null)
    {
        InitializeComponent();
        _settings = settings;
        _onSaved = onSaved;
        _onPresetsChanged = onPresetsChanged;
        Icon = IconHelper.CreateIcon("⚙", 0x7D, 0xD4, 0xD4);
        LoadValues();
        LoadPresets();
    }

    private void LoadValues()
    {
        // 开机启动
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegRunKey, false);
            ChkAutoStart.IsChecked = key?.GetValue(AppName) != null;
        }
        catch
        {
            ChkAutoStart.IsChecked = false;
        }

        // 窗口置顶
        ChkTopMost.IsChecked = _settings.WindowTopMost;

        // 窗口位置
        if (!double.IsNaN(_settings.WindowLeft))
            TxtWinLeft.Text = _settings.WindowLeft.ToString("F0");
        if (!double.IsNaN(_settings.WindowTop))
            TxtWinTop.Text = _settings.WindowTop.ToString("F0");

        // 采集间隔
        TxtPollInterval.Text = _settings.PollIntervalMs.ToString();

        // 温度上限
        TxtCpuMaxTemp.Text = _settings.CpuMaxTempC.ToString("F0");
        TxtGpuMaxTemp.Text = _settings.GpuMaxTempC.ToString("F0");

        // 颜色阈值
        TxtUsageGreen.Text = _settings.ColorThresholds.UsageGreen.ToString("F0");
        TxtUsageYellow.Text = _settings.ColorThresholds.UsageYellow.ToString("F0");
        TxtTempGreen.Text = _settings.ColorThresholds.TempGreen.ToString("F0");
        TxtTempYellow.Text = _settings.ColorThresholds.TempYellow.ToString("F0");
    }

    private void LoadPresets()
    {
        PresetList.ItemsSource = null;
        PresetList.ItemsSource = _settings.PositionPresets;
    }

    private void CapturePos_Click(object sender, RoutedEventArgs e)
    {
        if (Application.Current.MainWindow is MainWindow main)
        {
            TxtWinLeft.Text = main.Left.ToString("F0");
            TxtWinTop.Text = main.Top.ToString("F0");
        }
    }

    private void SavePreset_Click(object sender, RoutedEventArgs e)
    {
        string name = TxtPresetName.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(name))
        {
            ShowError("请输入预设名称");
            return;
        }

        double left = 0;
        double top = 0;
        if (Application.Current.MainWindow is MainWindow main)
        {
            left = main.Left;
            top = main.Top;
        }
        else if (double.TryParse(TxtWinLeft.Text, out double l) && double.TryParse(TxtWinTop.Text, out double t))
        {
            left = l;
            top = t;
        }

        _settings.PositionPresets.Add(new WindowPositionPreset
        {
            Name = name,
            Left = left,
            Top = top
        });

        SettingsService.Save(_settings);
        LoadPresets();
        TxtPresetName.Text = "";
        _onPresetsChanged?.Invoke();
        ShowStatus($"已保存预设: {name}");
    }

    private void GotoPreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement btn && btn.Tag is WindowPositionPreset preset)
        {
            if (Application.Current.MainWindow is MainWindow main)
            {
                main.MoveToPosition(preset.Left, preset.Top);
                ShowStatus($"已跳转到: {preset.Name}");
            }
        }
    }

    private void DeletePreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement btn && btn.Tag is WindowPositionPreset preset)
        {
            _settings.PositionPresets.Remove(preset);
            SettingsService.Save(_settings);
            LoadPresets();
            _onPresetsChanged?.Invoke();
            ShowStatus($"已删除预设: {preset.Name}");
        }
    }

    private void PresetList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        _isDragging = false;
    }

    private void PresetList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _isDragging)
            return;

        Point pos = e.GetPosition(null);
        Vector diff = _dragStartPoint - pos;

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            if (sender is ListBox listBox)
            {
                ListBoxItem? item = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                if (item == null)
                    return;

                _isDragging = true;
                DragDrop.DoDragDrop(item, item.DataContext, DragDropEffects.Move);
                _isDragging = false;
            }
        }
    }

    private void PresetList_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
    }

    private void PresetList_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(WindowPositionPreset)) is not WindowPositionPreset droppedData)
            return;

        if (sender is not ListBox listBox)
            return;

        // 获取放置目标位置
        ListBoxItem? targetItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        if (targetItem == null)
            return;

        WindowPositionPreset? targetData = targetItem.DataContext as WindowPositionPreset;
        if (targetData == null || targetData == droppedData)
            return;

        int oldIndex = _settings.PositionPresets.IndexOf(droppedData);
        int newIndex = _settings.PositionPresets.IndexOf(targetData);

        if (oldIndex < 0 || newIndex < 0 || oldIndex == newIndex)
            return;

        // 移动元素
        _settings.PositionPresets.RemoveAt(oldIndex);
        _settings.PositionPresets.Insert(newIndex, droppedData);

        // 持久化
        SettingsService.Save(_settings);
        LoadPresets();
        PresetList.SelectedItem = droppedData;
        _onPresetsChanged?.Invoke();
        ShowStatus($"已移动预设: {droppedData.Name}");
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T target)
                return target;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // 开机启动
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegRunKey, true);
            if (ChkAutoStart.IsChecked == true)
            {
                string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
                key?.SetValue(AppName, exePath);
            }
            else
            {
                key?.DeleteValue(AppName, false);
            }
        }
        catch { }

        // 窗口置顶
        _settings.WindowTopMost = ChkTopMost.IsChecked == true;

        // 窗口位置
        if (double.TryParse(TxtWinLeft.Text, out double winLeft))
            _settings.WindowLeft = winLeft;
        if (double.TryParse(TxtWinTop.Text, out double winTop))
            _settings.WindowTop = winTop;

        // 采集间隔
        if (int.TryParse(TxtPollInterval.Text, out int pollMs) && pollMs >= 500)
            _settings.PollIntervalMs = pollMs;

        // 温度上限
        if (double.TryParse(TxtCpuMaxTemp.Text, out double cpuMax))
            _settings.CpuMaxTempC = cpuMax;
        if (double.TryParse(TxtGpuMaxTemp.Text, out double gpuMax))
            _settings.GpuMaxTempC = gpuMax;

        // 颜色阈值
        if (double.TryParse(TxtUsageGreen.Text, out double ug))
            _settings.ColorThresholds.UsageGreen = ug;
        if (double.TryParse(TxtUsageYellow.Text, out double uy))
            _settings.ColorThresholds.UsageYellow = uy;
        if (double.TryParse(TxtTempGreen.Text, out double tg))
            _settings.ColorThresholds.TempGreen = tg;
        if (double.TryParse(TxtTempYellow.Text, out double ty))
            _settings.ColorThresholds.TempYellow = ty;

        SettingsService.Save(_settings);
        _onSaved?.Invoke(_settings);

        StatusText.Text = "已保存";
    }

    private void ShowStatus(string message)
    {
        StatusText.Text = message;
        StatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x6F, 0xCF, 0x6F));
    }

    private void ShowError(string message)
    {
        StatusText.Text = message;
        StatusText.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF0, 0x60, 0x80));
    }
}