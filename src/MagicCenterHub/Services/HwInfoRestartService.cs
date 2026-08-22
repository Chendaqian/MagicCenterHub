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
    private DateTime? _nextScheduledRestartAt;

    public HwInfoRestartService()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _timer.Tick += OnTimerTick;
    }

    public void Start(Settings settings)
    {
        _settings = settings;
        ResetSchedule();
        _timer.Start();
    }

    public void UpdateSettings(Settings settings)
    {
        bool scheduleChanged = _settings?.HwInfoScheduledRestartEnabled != settings.HwInfoScheduledRestartEnabled ||
            _settings?.HwInfoRestartIntervalHours != settings.HwInfoRestartIntervalHours;
        _settings = settings;
        if (scheduleChanged)
            ResetSchedule();
    }

    public bool RestartNow()
    {
        return RestartHwInfo();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_settings?.HwInfoScheduledRestartEnabled != true || _settings.HwInfoRestartIntervalHours <= 0)
        {
            return;
        }

        DateTime now = DateTime.Now;
        if (_nextScheduledRestartAt == null)
            ResetSchedule();

        if (_nextScheduledRestartAt == null || now < _nextScheduledRestartAt)
            return;

        _nextScheduledRestartAt = now.AddHours(_settings.HwInfoRestartIntervalHours);
        RestartHwInfo();
    }

    private void ResetSchedule()
    {
        _nextScheduledRestartAt = _settings?.HwInfoScheduledRestartEnabled == true &&
                                  _settings.HwInfoRestartIntervalHours > 0
            ? DateTime.Now.AddHours(_settings.HwInfoRestartIntervalHours)
            : null;
    }

    private static bool RestartHwInfo()
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

            executablePath ??= FindHWiNFOExecutable();

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

            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                return false;

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

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        GC.SuppressFinalize(this);
    }
}