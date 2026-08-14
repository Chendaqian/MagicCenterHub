# Claude Code Hook 调用脚本
# 功能：通过命名管道控制 LED 灯效 + 可选 BurntToast 通知
# 用法: powershell -File send-hook.ps1 -LedMode <0-19> [-Message <msg>] [-NoNotify] [-Agent <claudecode|codex>] [-EnableLog]
# 示例: powershell -File send-hook.ps1 -LedMode 5 -Message "✅ 任务已完成"
# 示例: powershell -File send-hook.ps1 -LedMode 2 -NoNotify

param(
    [Parameter(Mandatory=$true)]
    [int]$LedMode,
    [string]$Message = "",
    [string]$Sound = "Default",
    [switch]$NoNotify,
    [ValidateSet("claudecode", "codex")]
    [string]$Agent = "claudecode",
    [switch]$EnableLog,
    [switch]$Worker
)

if (-not $Worker) {
    function ConvertTo-ProcessArgument {
        param([AllowEmptyString()][string]$Value)

        if ($null -eq $Value) {
            return '""'
        }

        $escaped = $Value -replace '(\\*)"', '$1$1\"'
        $escaped = $escaped -replace '(\\+)$', '$1$1'
        return '"' + $escaped + '"'
    }

    $workerArguments = @(
        '-NoProfile',
        '-ExecutionPolicy', 'Bypass',
        '-File', $MyInvocation.MyCommand.Path,
        '-LedMode', $LedMode.ToString(),
        '-Sound', $Sound,
        '-Agent', $Agent,
        '-Worker'
    )
    if ($Message -ne '') {
        $workerArguments += @('-Message', $Message)
    }
    if ($NoNotify) {
        $workerArguments += '-NoNotify'
    }
    if ($EnableLog) {
        $workerArguments += '-EnableLog'
    }

    $argumentLine = ($workerArguments | ForEach-Object { ConvertTo-ProcessArgument $_ }) -join ' '
    Start-Process -FilePath 'pwsh' -ArgumentList $argumentLine -WorkingDirectory (Get-Location).Path -WindowStyle Hidden | Out-Null
    exit 0
}

$pipeName = "ClaudeCodeMagicCenterHub"

# --- 1. 发送 LED 灯效到命名管道 ---
$json = @{
    ledMode = $LedMode
} | ConvertTo-Json -Compress

$logFile = if ($EnableLog) { "$env:TEMP\magichook-debug.log" } else { $null }
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

function Write-Log {
    param([string]$Text)

    if ($null -ne $logFile) {
        $Text | Out-File -FilePath $logFile -Append -Encoding UTF8
    }
}

Write-Log "$timestamp - Sending: $json"

try {
    $pipe = New-Object System.IO.Pipes.NamedPipeClientStream(".", $pipeName, [System.IO.Pipes.PipeDirection]::Out)
    $pipe.Connect(2000)
    $writer = New-Object System.IO.StreamWriter($pipe)
    $writer.WriteLine($json)
    $writer.Flush()
    $writer.Dispose()
    $pipe.Dispose()
    Write-Log "$timestamp - LED Success"
} catch {
    Write-Log "$timestamp - LED Error: $_"
    [Console]::Error.WriteLine("Failed to send hook: $_")
}

# --- 2. BurntToast 通知（除非 -NoNotify）---
if (-not $NoNotify -and $Message -ne "") {
    try {
        $title = Split-Path -Leaf (Get-Location)
        $iconName = if ($Agent -eq "codex") { "Codex.png" } else { "Claude.png" }
        $icon = "D:\Source\Repos\MagicCenterHub\src\MagicCenterHub\Resources\$iconName"
        $toastScript = "Import-Module BurntToast; New-BurntToastNotification -Text '$title', '$Message' -AppLogo '$icon' -Sound $Sound"

        # 异步启动，不阻塞 hook 进程
        Start-Process pwsh -ArgumentList "-ExecutionPolicy RemoteSigned -Command `"$toastScript`"" -WindowStyle Hidden

        Write-Log "$timestamp - Toast Launched: $title / $Message"
    } catch {
        Write-Log "$timestamp - Toast Error: $_"
    }
}

# 始终返回成功，硬件不可用时 hook 不会报错
exit 0
