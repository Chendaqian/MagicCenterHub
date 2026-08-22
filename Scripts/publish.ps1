# 发布脚本 - 根据项目程序集版本创建 tag，并推送触发 GitHub Action 自动发布

$ErrorActionPreference = 'Stop'

$projectPath = Join-Path $PSScriptRoot '..\src\MagicCenterHub\MagicCenterHub.csproj'
$versionOutput = dotnet msbuild $projectPath -getProperty:Version | Out-String
if ($LASTEXITCODE -ne 0) {
    throw '无法读取项目程序集版本。'
}

$version = $versionOutput.Trim()

if ([string]::IsNullOrWhiteSpace($version) -or $version -notmatch '^\d+\.\d+\.\d+$') {
    throw "项目版本 '$version' 不是有效的三段式版本号。"
}

$tag = "v$version"

Write-Host "准备发布版本: $tag" -ForegroundColor Cyan

# 检查是否有未提交的更改
$status = git status --porcelain
if ($status) {
    Write-Host "警告: 有未提交的更改，请先提交:" -ForegroundColor Yellow
    Write-Host $status
    $confirm = Read-Host "继续发布? (y/N)"
    if ($confirm -ne 'y') {
        Write-Host "已取消" -ForegroundColor Red
        exit
    }
}

# 创建 tag 并推送
Write-Host "创建 tag: $tag" -ForegroundColor Green
git tag -a $tag -m $tag
git push origin $tag

Write-Host "已推送 tag $tag，GitHub Action 将自动构建并发布，Release title 为 $tag" -ForegroundColor Green
Write-Host "查看发布进度: https://github.com/$(git remote get-url origin | ForEach-Object { $_ -replace '.*github.com[:/](.+)\.git$', '$1' })/actions" -ForegroundColor Cyan
