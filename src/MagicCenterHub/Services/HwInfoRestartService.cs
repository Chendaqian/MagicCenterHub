using MagicCenterHub.Models;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;

namespace MagicCenterHub.Services;

/// <summary>
/// HWiNFO 定时重启服务，避免免费版运行时间达到限制后共享内存失效。
/// </summary>
public sealed class HwInfoRestartService : IDisposable
{
    private static readonly string[] ProcessNames = ["HWiNFO64", "HWiNFO32"];
    private readonly DispatcherTimer _timer;
    private Settings? _settings;

    public HwInfoRestartService()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _timer.Tick += OnTimerTick;
    }

    public void Start(Settings settings)
    {
        _settings = settings;
        _timer.Start();
    }

    public void UpdateSettings(Settings settings)
    {
        _settings = settings;
    }

    public bool RestartNow()
    {
        return RestartHwInfo(_settings?.HwInfoExecutablePath);
    }

    public bool RestartNow(string? executablePath)
    {
        return RestartHwInfo(executablePath);
    }

    /// <summary>
    /// 获取 HWiNFO 进程的启动时间（多个进程时取最早启动的），未运行时返回 null。
    /// </summary>
    public static DateTime? GetHwInfoStartTime()
    {
        DateTime? oldest = null;

        foreach (string processName in ProcessNames)
        {
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                using (process)
                {
                    try
                    {
                        DateTime startTime = process.StartTime;
                        if (oldest == null || startTime < oldest)
                            oldest = startTime;
                    }
                    catch
                    {
                        // 进程可能刚好退出，跳过
                    }
                }
            }
        }

        return oldest;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_settings?.HwInfoScheduledRestartEnabled != true || _settings.HwInfoRestartIntervalHours <= 0)
        {
            return;
        }

        // 以 HWiNFO 进程实际启动时间为基准，应用重启或系统睡眠后依然对齐
        DateTime? startTime = GetHwInfoStartTime();
        if (startTime == null)
            return;

        if (DateTime.Now - startTime.Value < TimeSpan.FromHours(_settings.HwInfoRestartIntervalHours))
            return;

        RestartHwInfo(_settings.HwInfoExecutablePath);
    }

    private static bool RestartHwInfo(string? configuredPath)
    {
        List<Process> processes = [];
        string? executablePath = null;

        try
        {
            foreach (string processName in ProcessNames)
            {
                foreach (Process process in Process.GetProcessesByName(processName))
                {
                    processes.Add(process);
                    executablePath ??= TryGetExecutablePath(process);
                }
            }

            executablePath ??= ResolveExecutablePath(configuredPath);

            // 没有可执行文件时视为无需处理，交由调用方继续重启当前程序。
            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                return true;

            foreach (Process process in processes)
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill(entireProcessTree: true);
                }
                catch
                {
                    return false;
                }
            }

            foreach (Process process in processes)
            {
                try
                {
                    if (!process.WaitForExit(5000))
                        return false;
                }
                catch
                {
                    return false;
                }
            }

            using Process? restartedProcess = Process.Start(new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = true
            });
            return restartedProcess != null;
        }
        catch
        {
            return false;
        }
        finally
        {
            foreach (Process process in processes)
                process.Dispose();
        }
    }

    private static string? TryGetExecutablePath(Process process)
    {
        try
        {
            return process.MainModule?.FileName;
        }
        catch
        {
            return null;
        }
    }

    private static string? FindHWiNFOExecutable()
    {
        string[] roots =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        ];

        foreach (string root in roots.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            string[] candidates =
            [
                Path.Combine(root, "HWiNFO64", "HWiNFO64.exe"),
                Path.Combine(root, "HWiNFO", "HWiNFO64.exe")
            ];
            string? match = candidates.FirstOrDefault(File.Exists);
            if (match != null)
                return match;
        }

        return null;
    }

    public static string? FindExecutablePath()
    {
        foreach (string processName in ProcessNames)
        {
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                using (process)
                {
                    string? executablePath = TryGetExecutablePath(process);
                    if (!string.IsNullOrWhiteSpace(executablePath) && File.Exists(executablePath))
                        return executablePath;
                }
            }
        }

        return FindHWiNFOExecutable();
    }

    private static string? ResolveExecutablePath(string? configuredPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath) && File.Exists(configuredPath.Trim()))
            return Path.GetFullPath(configuredPath.Trim());

        return FindHWiNFOExecutable();
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        GC.SuppressFinalize(this);
    }
}
