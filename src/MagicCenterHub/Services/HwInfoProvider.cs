using MagicCenterHub.Models;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace MagicCenterHub.Services;

/// <summary>
/// HWiNFO64 共享内存数据读取器（v2 格式）
/// </summary>
public class HwInfoProvider : IDisposable
{
    private const uint Signature_HNIS = 0x53494E48;
    private const uint Signature_HWiS = 0x53695748;
    private const int Offset_labelOrig = 12;
    private const int Offset_labelUser = 140;
    private const int Offset_unit = 268;
    private const int Offset_value = 284;

    private static readonly Encoding Gbk = Encoding.GetEncoding(936);

    private static readonly string[] SharedMemoryNames = [
        "Global\\HWiNFO_SENS_SM2",
        "Global\\HWiNFO_SM2",
        "Global\\HWiNFO_SENS_SM",
        "HWiNFO_SENS_SM2",
        "HWiNFO_SM2",
        "Local\\HWiNFO_SENS_SM2",
    ];

    private MemoryMappedFile? _mmf;
    private MemoryMappedViewAccessor? _accessor;
    private bool _isAvailable;

    // 复用 buffer，避免每帧分配
    private readonly byte[] _buf128 = new byte[128];

    private readonly byte[] _buf16 = new byte[16];

    /// <summary>
    /// 尝试连接 HWiNFO 共享内存
    /// </summary>
    /// <returns>连接是否成功</returns>
    public bool TryConnect()
    {
        foreach (string name in SharedMemoryNames)
        {
            try
            {
                Dispose();
                _mmf = MemoryMappedFile.OpenExisting(name, MemoryMappedFileRights.Read);
                _accessor = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
                _isAvailable = true;
                return true;
            }
            catch { }
        }
        _isAvailable = false;
        return false;
    }

    /// <summary>
    /// 从共享内存读取当前硬件数据
    /// </summary>
    /// <returns>硬件数据，连接断开时返回空数据（Valid 全 false）</returns>
    public HardwareData Read()
    {
        HardwareData data = new HardwareData();
        if (!_isAvailable || _accessor == null)
            return data;

        try
        {
            uint signature = _accessor.ReadUInt32(0);
            if (signature != Signature_HNIS && signature != Signature_HWiS)
            {
                _isAvailable = false;
                return data;
            }

            uint readingOffset = _accessor.ReadUInt32(32);
            uint readingSize = _accessor.ReadUInt32(36);
            uint readingNum = _accessor.ReadUInt32(40);

            for (uint i = 0; i < readingNum; i++)
            {
                int baseOff = (int)(readingOffset + (i * readingSize));

                int sensorType = _accessor.ReadInt32(baseOff);
                string labelOrig = ReadString(baseOff + Offset_labelOrig, _buf128, false);
                string labelUser = ReadString(baseOff + Offset_labelUser, _buf128, true);
                string unit = ReadString(baseOff + Offset_unit, _buf16, false);
                double value = _accessor.ReadDouble(baseOff + Offset_value);

                string label = labelUser.Length > 0 ? labelUser : labelOrig;
                MatchReading(data, label, unit, sensorType, value);
            }
        }
        catch
        {
            _isAvailable = false;
        }

        return data;
    }

    private string ReadString(int offset, byte[] buffer, bool useGbk)
    {
        _accessor!.ReadArray(offset, buffer, 0, buffer.Length);
        int nullIdx = Array.IndexOf(buffer, (byte)0);
        int length = nullIdx >= 0 ? nullIdx : buffer.Length;
        if (length == 0) return string.Empty;

        Encoding enc = useGbk ? Gbk : Encoding.ASCII;

        return enc.GetString(buffer, 0, length).Trim();
    }

    private static void MatchReading(HardwareData data, string label, string unit, int sensorType, double value)
    {
        string lower = label.ToLowerInvariant();
        bool isTemp = sensorType == 1;
        bool isUsage = sensorType == 7;

        // CPU 使用率
        if (isUsage && !data.CpuUsageValid
            && (lower.Contains("cpu") || lower.Contains("处理器"))
            && (lower.Contains("total") || lower.Contains('总') || lower.Contains("合计")))
        {
            data.CpuUsage = value;
            data.CpuUsageValid = true;
            return;
        }

        // CPU 温度
        if (isTemp && !data.CpuTempValid
            && (lower.Contains("package") || lower.Contains("封装")
                || (lower.Contains("cpu") && !lower.Contains("gpu") && !lower.Contains("核显"))))
        {
            data.CpuTemp = value;
            data.CpuTempValid = true;
            return;
        }

        // GPU 使用率
        if (isUsage && !data.GpuUsageValid
            && (lower.Contains("gpu") || lower.Contains("核显")))
        {
            data.GpuUsage = value;
            data.GpuUsageValid = true;
            return;
        }

        // GPU 温度
        if (isTemp && !data.GpuTempValid
            && (lower.Contains("gpu") || lower.Contains("核显")))
        {
            data.GpuTemp = value;
            data.GpuTempValid = true;
            return;
        }

        // 内存已用
        if ((lower.Contains("已用物理内存") || lower.Contains("physical memory used"))
            && !lower.Contains("可用"))
        {
            data.MemoryUsedGB = unit.Contains("GB") ? value : value / 1024.0;
            return;
        }

        // 内存可用
        if (lower.Contains("可用物理内存") || lower.Contains("physical memory available"))
        {
            double availGB = unit.Contains("GB") ? value : value / 1024.0;
            if (data.MemoryUsedGB > 0)
                data.MemoryTotalGB = data.MemoryUsedGB + availGB;
            data.MemoryValid = true;
            return;
        }

        // 内存使用率（备选）
        if (!data.MemoryValid
            && (lower.Contains("physical memory") || lower.Contains("物理内存"))
            && (lower.Contains("load") || lower.Contains("使用率") || lower.Contains("usage")))
        {
            data.MemoryUsedGB = value;
            data.MemoryValid = true;
            return;
        }

        // 内存总量
        if ((lower.Contains("physical memory") || lower.Contains("物理内存"))
            && (lower.Contains("total") || lower.Contains("size") || lower.Contains("总量") || lower.Contains("大小")))
        {
            if (unit.Contains("GB")) data.MemoryTotalGB = value;
            else if (unit.Contains("MB")) data.MemoryTotalGB = value / 1024.0;
            return;
        }

        // 电池功耗
        if ((lower.Contains("charge rate") || lower.Contains("充电速度") || lower.Contains("放电速度"))
            && unit.Contains('W'))
        {
            data.BatteryWatt = Math.Abs(value);
            data.BatteryValid = true;
            return;
        }

        // 磁盘读写
        if ((lower.Contains('读') || lower.Contains("read"))
            && (lower.Contains("速度") || lower.Contains("rate"))
            && !data.DiskValid)
        {
            data.DiskReadBps = ConvertToBytesPerSec(value, unit);
            data.DiskValid = true;
            return;
        }
        if ((lower.Contains('写') || lower.Contains("write"))
            && (lower.Contains("速度") || lower.Contains("rate"))
            && data.DiskValid)
        {
            data.DiskWriteBps = ConvertToBytesPerSec(value, unit);
            return;
        }

        // 网络下行
        if ((lower.Contains("下载") || lower.Contains("download") || lower.Contains("dl"))
            && (lower.Contains("速度") || lower.Contains("rate")))
        {
            double bps = ConvertToBytesPerSec(value, unit);
            if (bps > data.NetDownBps)
            {
                data.NetDownBps = bps;
                data.NetValid = true;
            }
            return;
        }
        // 网络上行
        if ((lower.Contains("上传") || lower.Contains("upload") || lower.Contains("up"))
            && (lower.Contains("速度") || lower.Contains("rate")))
        {
            double bps = ConvertToBytesPerSec(value, unit);
            if (bps > data.NetUpBps)
                data.NetUpBps = bps;

            return;
        }
    }

    private static double ConvertToBytesPerSec(double value, string unit)
    {
        string u = unit.ToLowerInvariant();
        if (u.Contains("mb")) return value * 1024 * 1024;
        if (u.Contains("kb")) return value * 1024;

        return value;
    }

    /// <summary>
    /// 释放共享内存资源
    /// </summary>
    public void Dispose()
    {
        _accessor?.Dispose();
        _mmf?.Dispose();
        _accessor = null;
        _mmf = null;
        _isAvailable = false;
        GC.SuppressFinalize(this);
    }
}