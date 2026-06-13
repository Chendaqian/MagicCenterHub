using MagicCenterHub.Models;
using MagicCenterHub.Utils;
using MagicCenterHub.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MagicCenterHub;

/// <summary>
/// LED 灯效测试窗口，提供 20 种灯效模式的按钮切换
/// </summary>
public partial class LedTestWindow : Window
{
    private readonly MainViewModel _viewModel;

    private static readonly string[] ModeNames =
    [
        "全灭", "同闪", "绿闪", "红闪", "黄闪", "绿亮", "红亮", "黄亮", "全亮",
        "警车", "心跳", "SOS", "呼吸", "萤火虫", "心电",
        "跑马", "爆闪", "太极", "HELLO", "雷达"
    ];

    /// <summary>
    /// 初始化灯效测试窗口
    /// </summary>
    public LedTestWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        Icon = IconHelper.CreateIcon("☀", 0x7D, 0xD4, 0xD4);
    }

    private void Mode_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int mode))
        {
            _viewModel.SetLedMode((LedMode)mode);
            StatusText.Text = $"当前: {mode} {ModeNames[mode]}";
        }
    }
}