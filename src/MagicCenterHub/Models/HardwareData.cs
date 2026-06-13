namespace MagicCenterHub.Models;

/// <summary>
/// 硬件监控数据模型，包含 CPU/GPU/内存/电池/磁盘/网络的实时指标
/// </summary>
/// <remark>每个指标都有对应的 Valid 标记，数据来源（HWiNFO）未提供时 Valid 为 false</remark>
public class HardwareData
{
    /// <summary>
    /// CPU 使用率
    /// </summary>
    /// <remark>百分比 0-100</remark>
    public double CpuUsage { get; set; }

    /// <summary>
    /// CPU 温度
    /// </summary>
    /// <remark>摄氏度</remark>
    public double CpuTemp { get; set; }

    /// <summary>
    /// CPU 当前频率
    /// </summary>
    /// <remark>MHz</remark>
    public double CpuFreqMHz { get; set; }

    /// <summary>
    /// CPU 最大频率
    /// </summary>
    /// <remark>MHz</remark>
    public double CpuMaxFreqMHz { get; set; }

    /// <summary>
    /// GPU 使用率
    /// </summary>
    /// <remark>百分比 0-100</remark>
    public double GpuUsage { get; set; }

    /// <summary>
    /// GPU 温度
    /// </summary>
    /// <remark>摄氏度</remark>
    public double GpuTemp { get; set; }

    /// <summary>
    /// GPU 当前频率
    /// </summary>
    /// <remark>MHz</remark>
    public double GpuFreqMHz { get; set; }

    /// <summary>
    /// 已用内存
    /// </summary>
    /// <remark>GB；当 MemoryPercentOnly 为 true 时为百分比</remark>
    public double MemoryUsedGB { get; set; }

    /// <summary>
    /// 总内存
    /// </summary>
    /// <remark>GB</remark>
    public double MemoryTotalGB { get; set; }

    /// <summary>
    /// 电池功耗
    /// </summary>
    /// <remark>瓦特</remark>
    public double BatteryWatt { get; set; }

    /// <summary>
    /// 磁盘读取速度
    /// </summary>
    /// <remark>字节/秒</remark>
    public double DiskReadBps { get; set; }

    /// <summary>
    /// 磁盘写入速度
    /// </summary>
    /// <remark>字节/秒</remark>
    public double DiskWriteBps { get; set; }

    /// <summary>
    /// 网络上传速度
    /// </summary>
    /// <remark>字节/秒</remark>
    public double NetUpBps { get; set; }

    /// <summary>
    /// 网络下载速度
    /// </summary>
    /// <remark>字节/秒</remark>
    public double NetDownBps { get; set; }

    /// <summary>
    /// CPU 使用率是否有效
    /// </summary>
    public bool CpuUsageValid { get; set; }

    /// <summary>
    /// CPU 温度是否有效
    /// </summary>
    public bool CpuTempValid { get; set; }

    /// <summary>
    /// CPU 频率是否有效
    /// </summary>
    public bool CpuFreqValid { get; set; }

    /// <summary>
    /// GPU 使用率是否有效
    /// </summary>
    public bool GpuUsageValid { get; set; }

    /// <summary>
    /// GPU 温度是否有效
    /// </summary>
    public bool GpuTempValid { get; set; }

    /// <summary>
    /// GPU 频率是否有效
    /// </summary>
    public bool GpuFreqValid { get; set; }

    /// <summary>
    /// 内存数据是否有效
    /// </summary>
    public bool MemoryValid { get; set; }

    /// <summary>
    /// 电池数据是否有效
    /// </summary>
    public bool BatteryValid { get; set; }

    /// <summary>
    /// 磁盘数据是否有效
    /// </summary>
    public bool DiskValid { get; set; }

    /// <summary>
    /// 网络数据是否有效
    /// </summary>
    public bool NetValid { get; set; }
}
