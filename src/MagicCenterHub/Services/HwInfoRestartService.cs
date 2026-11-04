using System.Diagnostics;
using System.IO;

namespace MagicCenterHub.Services;

/// <summary>
/// HWiNFO 重启辅助，提供进程启动时间查询、可执行文件识别与手动重启。
/// </summary>
public sealed class HwInfoRestartService
{
    private static readonly string[] ProcessNames = ["HWiNFO64", "HWiNFO32"];

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
}
