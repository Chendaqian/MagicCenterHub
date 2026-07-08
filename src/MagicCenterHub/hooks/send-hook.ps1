# Claude Code Hook 调用脚本
# 功能：通过命名管道控制 LED 灯效 + 可选 BurntToast 通知
# 用法: powershell -File send-hook.ps1 -LedMode <0-19> [-Message <msg>] [-NoNotify]
# 示例: powershell -File send-hook.ps1 -LedMode 5 -Message "✅ 任务已完成"
# 示例: powershell -File send-hook.ps1 -LedMode 2 -NoNotify

param(
    [Parameter(Mandatory=$true)]
    [int]$LedMode,
    [string]$Message = "",
    [string]$Sound = "Default",
    [switch]$NoNotify
)

$pipeName = "ClaudeCodeMagicCenterHub"

# --- 1. 发送 LED 灯效到命名管道 ---
$json = @{
    ledMode = $LedMode
} | ConvertTo-Json -Compress

$logFile = "$env:TEMP\magichook-debug.log"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
"$timestamp - Sending: $json" | Out-File -FilePath $logFile -Append -Encoding UTF8

try {
    $pipe = New-Object System.IO.Pipes.NamedPipeClientStream(".", $pipeName, [System.IO.Pipes.PipeDirection]::Out)
    $pipe.Connect(2000)
    $writer = New-Object System.IO.StreamWriter($pipe)
    $writer.WriteLine($json)
    $writer.Flush()
    $writer.Dispose()
    $pipe.Dispose()
    "$timestamp - LED Success" | Out-File -FilePath $logFile -Append -Encoding UTF8
} catch {
    "$timestamp - LED Error: $_" | Out-File -FilePath $logFile -Append -Encoding UTF8
    Write-Host "Failed to send hook: $_"
}

# --- 2. BurntToast 通知（除非 -NoNotify）---
if (-not $NoNotify -and $Message -ne "") {
    try {
        $title = Split-Path -Leaf (Get-Location)
        $icon = "D:\Source\Repos\MagicCenterHub\src\MagicCenterHub\Resources\Claude.png"
        $toastScript = "Import-Module BurntToast; New-BurntToastNotification -Text '$title', '$Message' -AppLogo '$icon' -Sound $Sound"

        # 异步启动，不阻塞 hook 进程
        Start-Process pwsh -ArgumentList "-ExecutionPolicy RemoteSigned -Command `"$toastScript`"" -WindowStyle Hidden

        "$timestamp - Toast Launched: $title / $Message" | Out-File -FilePath $logFile -Append -Encoding UTF8
    } catch {
        "$timestamp - Toast Error: $_" | Out-File -FilePath $logFile -Append -Encoding UTF8
    }
}

# 始终返回成功，硬件不可用时 hook 不会报错
exit 0
