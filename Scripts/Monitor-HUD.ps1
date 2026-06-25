# 由任务计划程序在设备断开事件(1010)触发
# 逻辑：设备断开 → 等待重新连接 → 启动应用
$TargetDeviceID = "VID_17EF&PID_1117"
$AppPath = "D:\Source\Repos\MagicCenterHub\src\MagicCenterHub\bin\Debug\net8.0-windows\MagicCenterHub.exe"
$AppName = [System.IO.Path]::GetFileNameWithoutExtension($AppPath)
$TimeoutSeconds = 120  # 最长等待2分钟

# 设备断开时，杀掉应用进程
$running = Get-Process -Name $AppName -ErrorAction SilentlyContinue
if ($running) {
    Stop-Process -Name $AppName -Force
}

# 等待设备重新连接
$elapsed = 0
while ($elapsed -lt $TimeoutSeconds) {
    $device = Get-CimInstance -ClassName Win32_PnPEntity |
        Where-Object { $_.DeviceID -like "*$TargetDeviceID*" -and $_.Status -eq "OK" }

    if ($device) {
        $running = Get-Process -Name $AppName -ErrorAction SilentlyContinue
        if (-not $running) {
            Start-Sleep -Seconds 10
            Start-Process -FilePath $AppPath
        }
        exit
    }

    Start-Sleep -Seconds 2
    $elapsed += 2
}
