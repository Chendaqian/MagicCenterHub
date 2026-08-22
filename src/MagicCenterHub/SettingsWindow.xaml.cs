using MagicCenterHub.Models;
using MagicCenterHub.Services;
using MagicCenterHub.Utils;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MagicCenterHub;

/// <summary>
/// 设置窗口：开机自启、窗口位置、阈值配置
/// </summary>
public partial class SettingsWindow : Window
{
    private const string AppName = "MagicCenterHub";
    private const string RegRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string LatestReleaseApiUrl = "https://api.github.com/repos/ChenDaqian/MagicCenterHub/releases/latest";
    private const string LatestReleaseUrl = "https://github.com/ChenDaqian/MagicCenterHub/releases/latest";
    private static readonly HttpClient UpdateHttpClient = CreateUpdateHttpClient();
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
        TxtAppVersion.Text = $"当前版本：{GetCurrentVersionText()}";
        LoadValues();
        LoadPresets();
    }

    private static HttpClient CreateUpdateHttpClient()
    {
        HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("MagicCenterHub");
        return client;
    }

    private static Version CurrentVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0, 0);

    private static string GetCurrentVersionText()
    {
        return FormatVersion(CurrentVersion);
    }

    private static string FormatVersion(Version version)
    {
        return $"v{version.Major}.{version.Minor}.{Math.Max(version.Build, 0)}";
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

        // LED 灯效
        int defaultLedMode = Math.Clamp(_settings.DefaultLedMode, 0, 19);
        CmbDefaultLedMode.SelectedIndex = defaultLedMode;
        TxtLedIdleRestore.Text = _settings.LedIdleRestoreSeconds.ToString();
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

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
            return;

        Close();
        e.Handled = true;
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        Close();
    }

    private void SaveSettings()
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

        // LED 灯效
        if (CmbDefaultLedMode.SelectedItem is ComboBoxItem selectedMode && selectedMode.Tag is string tagStr && int.TryParse(tagStr, out int ledMode))
        {
            _settings.DefaultLedMode = ledMode;
        }
        if (int.TryParse(TxtLedIdleRestore.Text, out int idleSeconds) && idleSeconds >= 0)
        {
            _settings.LedIdleRestoreSeconds = idleSeconds;
        }

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

    private void OpenLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        OpenUrl(e.Uri.AbsoluteUri);
        e.Handled = true;
    }

    private static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    private void CheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (!BtnCheckUpdate.IsEnabled)
            return;

        BtnCheckUpdate.IsEnabled = false;
        UpdateStatusText.Foreground = new SolidColorBrush(Color.FromRgb(0xF0, 0xA8, 0x40));
        UpdateStatusText.Text = "正在检查更新...";
        _ = CheckForUpdatesAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            (Version latestVersion, string releaseUrl) = await GetLatestReleaseAsync();

            if (latestVersion > CurrentVersion)
            {
                UpdateStatusText.Foreground = new SolidColorBrush(Color.FromRgb(0xF0, 0xA8, 0x40));
                UpdateStatusText.Text = $"发现新版本：{FormatVersion(latestVersion)}";
                MessageBox.Show(this,
                    $"当前版本：{FormatVersion(CurrentVersion)}\n最新版本：{FormatVersion(latestVersion)}\n\n即将打开 GitHub Release 页面。",
                    "检查更新",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                OpenUrl(releaseUrl);
            }
            else
            {
                UpdateStatusText.Foreground = new SolidColorBrush(Color.FromRgb(0x6F, 0xCF, 0x6F));
                UpdateStatusText.Text = "当前已是最新版本，无需更新。";
            }
        }
        catch (Exception ex)
        {
            UpdateStatusText.Foreground = new SolidColorBrush(Color.FromRgb(0xF0, 0x60, 0x80));
            UpdateStatusText.Text = "无法完成更新检查。";
            MessageBox.Show(this, $"无法完成更新检查：{ex.Message}", "检查更新",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            BtnCheckUpdate.IsEnabled = true;
        }
    }

    private static async Task<(Version Version, string Url)> GetLatestReleaseAsync()
    {
        try
        {
            return await GetLatestReleaseFromApiAsync();
        }
        catch (Exception apiException)
        {
            try
            {
                return await GetLatestReleaseFromPageAsync();
            }
            catch (Exception pageException)
            {
                throw new InvalidOperationException(
                    $"GitHub API 和 Release 页面均无法访问：{pageException.Message}", apiException);
            }
        }
    }

    private static async Task<(Version Version, string Url)> GetLatestReleaseFromApiAsync()
    {
        using HttpResponseMessage response = await UpdateHttpClient.GetAsync(LatestReleaseApiUrl);
        response.EnsureSuccessStatusCode();

        await using Stream responseStream = await response.Content.ReadAsStreamAsync();
        using JsonDocument document = await JsonDocument.ParseAsync(responseStream);
        JsonElement root = document.RootElement;

        if (!root.TryGetProperty("tag_name", out JsonElement tagElement) ||
            !TryParseReleaseVersion(tagElement.GetString(), out Version latestVersion))
        {
            throw new InvalidOperationException("GitHub Release 版本无效。");
        }

        string releaseUrl = LatestReleaseUrl;
        if (root.TryGetProperty("html_url", out JsonElement urlElement) &&
            Uri.TryCreate(urlElement.GetString(), UriKind.Absolute, out Uri? parsedUrl) &&
            parsedUrl.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
        {
            releaseUrl = parsedUrl.AbsoluteUri;
        }

        return (latestVersion, releaseUrl);
    }

    private static async Task<(Version Version, string Url)> GetLatestReleaseFromPageAsync()
    {
        using HttpResponseMessage response = await UpdateHttpClient.GetAsync(
            LatestReleaseUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        Uri? finalUri = response.RequestMessage?.RequestUri;
        string? tagName = finalUri?.Segments.LastOrDefault()?.Trim('/');
        string releaseUrl = finalUri?.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) == true
            ? finalUri.AbsoluteUri
            : LatestReleaseUrl;

        string pageContent = await response.Content.ReadAsStringAsync();
        if (!TryParseReleaseVersion(tagName, out Version latestVersion))
        {
            Match tagMatch = Regex.Match(
                pageContent,
                @"/ChenDaqian/MagicCenterHub/releases/tag/([^""/?<]+)",
                RegexOptions.IgnoreCase);
            tagName = tagMatch.Success ? tagMatch.Groups[1].Value : null;
            releaseUrl = tagName == null ? releaseUrl :
                $"https://github.com/ChenDaqian/MagicCenterHub/releases/tag/{tagName}";
        }

        if (!TryParseReleaseVersion(tagName, out latestVersion))
            throw new InvalidOperationException("无法从 GitHub Release 页面读取版本。");

        return (latestVersion, releaseUrl);
    }

    private static bool TryParseReleaseVersion(string? tagName, out Version version)
    {
        version = new Version(0, 0, 0);
        if (string.IsNullOrWhiteSpace(tagName))
            return false;

        string normalized = tagName.Trim().TrimStart('v', 'V');
        int suffixIndex = normalized.IndexOfAny(['-', '+']);
        if (suffixIndex >= 0)
            normalized = normalized[..suffixIndex];

        if (!Version.TryParse(normalized, out Version? parsedVersion) || parsedVersion == null)
            return false;

        version = parsedVersion;
        return true;
    }

    /// <summary>
    /// LED 模式下拉框选择变化时，确保选中项文字为白色
    /// </summary>
    private void CmbDefaultLedMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            // 强制设置选中项的 Foreground 为白色
            selectedItem.Foreground = Brushes.White;
        }
    }
}
