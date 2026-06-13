using MagicCenterHub.Models;
using MagicCenterHub.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using Drawing = System.Drawing;
using WinForms = System.Windows.Forms;

namespace MagicCenterHub;

/// <summary>
/// 应用程序入口：单实例、托盘图标、开机自启
/// </summary>
public partial class App : Application
{
    private const string AppName = "MagicCenterHub";
    private const string RegRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    private Mutex? _mutex;
    private MainWindow? _mainWindow;
    private WinForms.NotifyIcon? _trayIcon;
    private WinForms.ContextMenuStrip? _trayMenu;

    protected override void OnStartup(StartupEventArgs e)
    {
        // 注册 CodePages 编码提供程序（支持 GBK 等编码）
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        // 命令行参数处理
        if (e.Args.Length > 0)
        {
            string arg = e.Args[0].ToLowerInvariant();
            if (arg == "/register")
            {
                RegisterStartup();
                Shutdown();
                return;
            }
            if (arg == "/unregister")
            {
                UnregisterStartup();
                Shutdown();
                return;
            }
        }

        // 单实例检测
        _mutex = new Mutex(true, AppName, out bool isNew);
        if (!isNew)
        {
            // 激活已有实例
            Process current = Process.GetCurrentProcess();
            foreach (Process proc in Process.GetProcessesByName(current.ProcessName))
            {
                if (proc.Id != current.Id)
                {
                    NativeMethods.SetForegroundWindow(proc.MainWindowHandle);
                    break;
                }
            }
            Shutdown();
            return;
        }

        base.OnStartup(e);

        Settings settings = SettingsService.Load();
        _mainWindow = new MainWindow(settings);
        _mainWindow.Show();

        SetupTrayIcon();

        // 启动后回收一次，减少初始内存占用
        GC.Collect(2, GCCollectionMode.Forced, false);
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new WinForms.NotifyIcon
        {
            Text = AppName,
            Visible = true,
            Icon = CreateTrayIcon()
        };

        RefreshTrayMenu();

        _trayIcon.DoubleClick += (s, e) => ToggleWindow();
    }

    /// <summary>
    /// 刷新托盘右键菜单（预设变化时调用）
    /// </summary>
    private void RefreshTrayMenu()
    {
        WinForms.ContextMenuStrip menu = new WinForms.ContextMenuStrip
        {
            Renderer = new DarkMenuRenderer(),
            BackColor = Drawing.Color.FromArgb(30, 30, 34),
            ForeColor = Drawing.Color.FromArgb(220, 220, 220),
            Padding = new WinForms.Padding(8, 4, 8, 4),
            ShowImageMargin = false
        };

        // 标题
        WinForms.ToolStripLabel title = new WinForms.ToolStripLabel($"  {AppName}")
        {
            ForeColor = Drawing.Color.FromArgb(0x7D, 0xD4, 0xD4),
            Font = new Drawing.Font("Segoe UI", 10, Drawing.FontStyle.Bold),
            Margin = new WinForms.Padding(0, 4, 0, 2)
        };
        menu.Items.Add(title);
        menu.Items.Add(new WinForms.ToolStripSeparator());

        // 菜单项
        menu.Items.Add(CreateMenuItem("显示主窗口", "☀", (s, e) => ToggleWindow()));
        menu.Items.Add(CreateMenuItem("LED 灯效测试", "💡", (s, e) => OpenLedTest()));

        // 窗口位置子菜单
        WinForms.ToolStripMenuItem posMenu = new WinForms.ToolStripMenuItem("  📍  窗口位置")
        {
            ForeColor = Drawing.Color.FromArgb(220, 220, 220),
            Font = new Drawing.Font("Segoe UI", 10),
            Padding = new WinForms.Padding(4, 2, 4, 2)
        };

        WinForms.ToolStripMenuItem resetItem = new WinForms.ToolStripMenuItem("  🖥  default")
        {
            ForeColor = Drawing.Color.FromArgb(220, 220, 220),
            Font = new Drawing.Font("Segoe UI", 10),
            Padding = new WinForms.Padding(4, 2, 4, 2)
        };
        resetItem.Click += (s, e) => _mainWindow?.MoveToPrimaryScreenCenter();
        posMenu.DropDownItems.Add(resetItem);

        if (_mainWindow?.Settings.PositionPresets.Count > 0)
        {
            posMenu.DropDownItems.Add(new WinForms.ToolStripSeparator());
            foreach (WindowPositionPreset preset in _mainWindow.Settings.PositionPresets)
            {
                WinForms.ToolStripMenuItem presetItem = new WinForms.ToolStripMenuItem($"  📌  {preset.Name}")
                {
                    ForeColor = Drawing.Color.FromArgb(220, 220, 220),
                    Font = new Drawing.Font("Segoe UI", 10),
                    Padding = new WinForms.Padding(4, 2, 4, 2)
                };
                double left = preset.Left;
                double top = preset.Top;
                presetItem.Click += (s, e) => _mainWindow?.MoveToPosition(left, top);
                posMenu.DropDownItems.Add(presetItem);
            }
        }

        menu.Items.Add(posMenu);

        menu.Items.Add(CreateMenuItem("设置", "⚙", (s, e) => OpenSettings()));

        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add(CreateMenuItem("退出程序", "✕", (s, e) => _mainWindow?.ExitApp(), isExit: true));

        WinForms.ContextMenuStrip? oldMenu = _trayMenu;
        _trayMenu = menu;
        if (_trayIcon != null)
            _trayIcon.ContextMenuStrip = menu;
        oldMenu?.Dispose();
    }

    private static WinForms.ToolStripMenuItem CreateMenuItem(string text, string icon, EventHandler onClick, bool isExit = false)
    {
        WinForms.ToolStripMenuItem item = new WinForms.ToolStripMenuItem($"  {icon}  {text}")
        {
            ForeColor = isExit ? Drawing.Color.FromArgb(0xF0, 0x60, 0x80) : Drawing.Color.FromArgb(220, 220, 220),
            Font = new Drawing.Font("Segoe UI", 10)
        };
        item.Click += onClick;
        item.Padding = new WinForms.Padding(4, 2, 4, 2);
        return item;
    }

    private static WinForms.ToolStripMenuItem CreateMenuItem(string text, Drawing.Bitmap? icon, EventHandler onClick, bool isExit = false)
    {
        WinForms.ToolStripMenuItem item = new WinForms.ToolStripMenuItem($"  {text}")
        {
            ForeColor = isExit ? Drawing.Color.FromArgb(0xF0, 0x60, 0x80) : Drawing.Color.FromArgb(220, 220, 220),
            Font = new Drawing.Font("Segoe UI", 10)
        };
        item.Click += onClick;
        item.Padding = new WinForms.Padding(4, 2, 4, 2);
        if (icon != null) item.Image = icon;
        return item;
    }

    /// <summary>
    /// 深色菜单渲染器
    /// </summary>
    private class DarkMenuRenderer : WinForms.ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColorTable())
        {
        }

        protected override void OnRenderMenuItemBackground(WinForms.ToolStripItemRenderEventArgs e)
        {
            Drawing.Rectangle rc = new Drawing.Rectangle(Drawing.Point.Empty, e.Item.Size);
            Drawing.Color bgColor = e.Item.Selected
                ? Drawing.Color.FromArgb(50, 50, 56)
                : Drawing.Color.FromArgb(30, 30, 34);
            using Drawing.SolidBrush brush = new Drawing.SolidBrush(bgColor);
            e.Graphics.FillRectangle(brush, rc);

            // 选中时左侧高亮条
            if (e.Item.Selected && e.Item is not WinForms.ToolStripLabel)
            {
                using Drawing.SolidBrush accent = new Drawing.SolidBrush(Drawing.Color.FromArgb(0x7D, 0xD4, 0xD4));
                e.Graphics.FillRectangle(accent, 0, 0, 3, rc.Height);
            }
        }

        protected override void OnRenderSeparator(WinForms.ToolStripSeparatorRenderEventArgs e)
        {
            Drawing.Rectangle rc = new Drawing.Rectangle(20, e.Item.Height / 2, e.Item.Width - 40, 1);
            using Drawing.Pen pen = new Drawing.Pen(Drawing.Color.FromArgb(60, 60, 66));
            e.Graphics.DrawLine(pen, rc.Left, rc.Top, rc.Right, rc.Top);
        }

        protected override void OnRenderToolStripBorder(WinForms.ToolStripRenderEventArgs e)
        {
            using Drawing.Pen pen = new Drawing.Pen(Drawing.Color.FromArgb(60, 60, 66));
            e.Graphics.DrawRectangle(pen, 0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1);
        }
    }

    private class DarkColorTable : WinForms.ProfessionalColorTable
    {
        public override Drawing.Color ToolStripDropDownBackground => Drawing.Color.FromArgb(30, 30, 34);
        public override Drawing.Color MenuBorder => Drawing.Color.FromArgb(60, 60, 66);
        public override Drawing.Color MenuItemBorder => Drawing.Color.FromArgb(60, 60, 66);
        public override Drawing.Color MenuItemSelected => Drawing.Color.FromArgb(50, 50, 56);
        public override Drawing.Color MenuStripGradientBegin => Drawing.Color.FromArgb(30, 30, 34);
        public override Drawing.Color MenuStripGradientEnd => Drawing.Color.FromArgb(30, 30, 34);
        public override Drawing.Color MenuItemSelectedGradientBegin => Drawing.Color.FromArgb(50, 50, 56);
        public override Drawing.Color MenuItemSelectedGradientEnd => Drawing.Color.FromArgb(50, 50, 56);
        public override Drawing.Color MenuItemPressedGradientBegin => Drawing.Color.FromArgb(40, 40, 46);
        public override Drawing.Color MenuItemPressedGradientEnd => Drawing.Color.FromArgb(40, 40, 46);
        public override Drawing.Color ImageMarginGradientBegin => Drawing.Color.FromArgb(30, 30, 34);
        public override Drawing.Color ImageMarginGradientMiddle => Drawing.Color.FromArgb(30, 30, 34);
        public override Drawing.Color ImageMarginGradientEnd => Drawing.Color.FromArgb(30, 30, 34);
    }

    /// <summary>
    /// 从 .ico 文件加载托盘图标，若不存在则使用默认图标
    /// </summary>
    private static Drawing.Icon CreateTrayIcon()
    {
        try
        {
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "MagicCenterHub.ico");
            if (File.Exists(iconPath))
                return new Drawing.Icon(iconPath, 32, 32);
        }
        catch
        {
            // 加载失败，使用默认图标
        }
        return Drawing.SystemIcons.Application;
    }

    /// <summary>
    /// 在指定屏幕位置显示右键菜单（供主窗口右键调用）
    /// </summary>
    public void ShowContextMenuAt(int screenX, int screenY)
    {
        _trayMenu?.Show(screenX, screenY);
    }

    private void ToggleWindow()
    {
        if (_mainWindow == null) return;
        if (_mainWindow.IsVisible)
            _mainWindow.Hide();
        else
            _mainWindow.Show();
    }

    private void OpenLedTest()
    {
        if (_mainWindow == null) return;
        LedTestWindow testWindow = new LedTestWindow(_mainWindow.ViewModel);
        testWindow.Show();
    }

    private void OpenSettings()
    {
        if (_mainWindow == null) return;
        SettingsWindow settingsWindow = new SettingsWindow(_mainWindow.Settings,
            onSaved: newSettings => _mainWindow.ApplySettings(newSettings),
            onPresetsChanged: () => RefreshTrayMenu());
        settingsWindow.Show();
    }

    private static void RegisterStartup()
    {
        try
        {
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegRunKey, true);
            key?.SetValue(AppName, exePath);
        }
        catch { }
    }

    private static void UnregisterStartup()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegRunKey, true);
            key?.DeleteValue(AppName, false);
        }
        catch { }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}

/// <summary>
/// P/Invoke 方法声明
/// </summary>
internal static partial class NativeMethods
{
    /// <summary>
    /// 将指定窗口带到前台并激活
    /// </summary>
    [System.Runtime.InteropServices.LibraryImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    public static partial bool SetForegroundWindow(IntPtr hWnd);
}