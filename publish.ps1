# 发布脚本 - 创建 tag 并推送触发 GitHub Action 自动发布
# 用法: .\publish.ps1 1.0.0

param(
    [Parameter(Mandatory=$true)]
    [string]$Version
)

$tag = "v$Version"

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
git tag -a $tag -m "Release $tag"
git push origin $tag

Write-Host "已推送 tag $tag，GitHub Action 将自动构建并发布" -ForegroundColor Green
Write-Host "查看发布进度: https://github.com/$(git remote get-url origin | ForEach-Object { $_ -replace '.*github.com[:/](.+)\.git$', '$1' })/actions" -ForegroundColor Cyan
