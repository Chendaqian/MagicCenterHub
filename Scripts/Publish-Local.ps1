param(
    [string]$OutputRoot = (Join-Path $PSScriptRoot '..\publish')
)

$ErrorActionPreference = 'Stop'

$projectPath = Join-Path $PSScriptRoot '..\src\MagicCenterHub\MagicCenterHub.csproj'
$resolvedOutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)

Write-Host "发布项目: $projectPath" -ForegroundColor Cyan

function Publish-Version {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [bool]$SelfContained
    )

    $outputPath = Join-Path $resolvedOutputRoot $Name
    $resolvedOutputPath = [System.IO.Path]::GetFullPath($outputPath)

    Write-Host "输出目录: $resolvedOutputPath" -ForegroundColor Cyan

    dotnet publish $projectPath `
        --configuration Release `
        --runtime win-x64 `
        --self-contained $SelfContained.ToString().ToLowerInvariant() `
        --output $resolvedOutputPath `
        -p:DebugType=None `
        -p:DebugSymbols=false

    Get-ChildItem -LiteralPath $resolvedOutputPath -Recurse -File -Filter '*.pdb' |
        Remove-Item -Force

    $pdbFiles = @(Get-ChildItem -LiteralPath $resolvedOutputPath -Recurse -File -Filter '*.pdb')
    if ($pdbFiles.Count -gt 0) {
        throw "$Name 发布目录仍包含 PDB 文件。"
    }

    $executablePath = Join-Path $resolvedOutputPath 'MagicCenterHub.exe'
    if (-not (Test-Path -LiteralPath $executablePath -PathType Leaf)) {
        throw "未找到 $Name 发布结果: $executablePath"
    }

    Write-Host "$Name 发布完成: $executablePath" -ForegroundColor Green
}

Publish-Version -Name 'self-contained' -SelfContained $true
Publish-Version -Name 'framework-dependent' -SelfContained $false
