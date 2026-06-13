namespace MagicCenterHub.Models;

/// <summary>
/// 应用配置模型
/// </summary>
public class Settings
{
    /// <summary>
    /// 窗口 X 坐标（-1 表示自动定位）
    /// </summary>
    public double WindowLeft { get; set; } = -1;

    /// <summary>
    /// 窗口 Y 坐标（-1 表示自动定位）
    /// </summary>
    public double WindowTop { get; set; } = -1;

    /// <summary>
    /// 窗口是否置顶
    /// </summary>
    public bool WindowTopMost { get; set; } = true;

    /// <summary>
    /// CPU 温度上限（进度条满值，摄氏度）
    /// </summary>
    public double CpuMaxTempC { get; set; } = 100;

    /// <summary>
    /// GPU 温度上限（进度条满值，摄氏度）
    /// </summary>
    public double GpuMaxTempC { get; set; } = 95;

    /// <summary>
    /// HWiNFO 采集间隔（毫秒）
    /// </summary>
    public int PollIntervalMs { get; set; } = 3000;

    /// <summary>
    /// 三档颜色阈值配置
    /// </summary>
    public ColorThresholds ColorThresholds { get; set; } = new();

    /// <summary>
    /// 窗口位置预设列表
    /// </summary>
    public List<WindowPositionPreset> PositionPresets { get; set; } = new();
}

/// <summary>
/// 三档颜色阈值配置
/// </summary>
public class ColorThresholds
{
    /// <summary>
    /// 使用率绿色阈值（低于此值为绿色）
    /// </summary>
    public double UsageGreen { get; set; } = 50;

    /// <summary>
    /// 使用率黄色阈值（低于此值为黄色，高于为红色）
    /// </summary>
    public double UsageYellow { get; set; } = 80;

    /// <summary>
    /// 温度绿色阈值（低于此值为绿色）
    /// </summary>
    public double TempGreen { get; set; } = 60;

    /// <summary>
    /// 温度黄色阈值（低于此值为黄色，高于为红色）
    /// </summary>
    public double TempYellow { get; set; } = 80;
}

/// <summary>
/// 窗口位置预设
/// </summary>
public class WindowPositionPreset
{
    /// <summary>
    /// 预设名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 窗口 X 坐标
    /// </summary>
    public double Left { get; set; }

    /// <summary>
    /// 窗口 Y 坐标
    /// </summary>
    public double Top { get; set; }
}