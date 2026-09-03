<#
  unity-check.ps1 - Unity バッチモードによるコンパイル検証スクリプト
  用途: コミット / プッシュ / PR 作成の前に、コンパイルエラーが無いことを検証する。
  例:   powershell -ExecutionPolicy Bypass -File .claude/scripts/unity-check.ps1
  終了コード: 0 = エラーなし / 1 = エラーあり(またはUnity起動失敗)
#>
param(
    [string]$UnityVersion = '6000.5.3f1',
    [switch]$ShowWarnings
)

$ErrorActionPreference = 'Stop'

$projectPath = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$unityExe    = Join-Path "C:\Program Files\Unity\Hub\Editor" "$UnityVersion\Editor\Unity.exe"
$logPath     = Join-Path $PSScriptRoot 'unity-check.log'

if (-not (Test-Path $unityExe)) {
    Write-Output "[unity-check] Unity が見つかりません: $unityExe"
    exit 1
}

if (Test-Path $logPath) { Remove-Item $logPath -Force }

Write-Output "[unity-check] プロジェクト: $projectPath"
Write-Output "[unity-check] Unity      : $UnityVersion"
Write-Output "[unity-check] コンパイル検証を開始します..."

$proc = Start-Process -FilePath $unityExe -PassThru -Wait -NoNewWindow -ArgumentList @(
    '-batchmode', '-quit', '-nographics',
    '-projectPath', "`"$projectPath`"",
    '-logFile', "`"$logPath`""
)

if (-not (Test-Path $logPath)) {
    Write-Output "[unity-check] ログが生成されませんでした。ExitCode=$($proc.ExitCode)"
    exit 1
}

$log      = Get-Content $logPath
# コンパイルエラーのみを対象とする(ライセンス初期化時の一過性メッセージを誤検出しないため)
$errors   = $log | Select-String -Pattern 'error CS\d+|Compilation failed'
$warnings = $log | Select-String -Pattern 'warning CS\d+'

if ($errors.Count -gt 0) {
    Write-Output "[unity-check] === コンパイルエラー $($errors.Count) 件 ==="
    $errors | ForEach-Object { Write-Output $_.Line }
    Write-Output "[unity-check] 結果: NG (ログ: $logPath)"
    exit 1
}

if ($ShowWarnings -and $warnings.Count -gt 0) {
    Write-Output "[unity-check] === 警告 $($warnings.Count) 件 ==="
    $warnings | Select-Object -First 50 | ForEach-Object { Write-Output $_.Line }
}

if ($proc.ExitCode -ne 0) {
    Write-Output "[unity-check] Unity が異常終了しました。ExitCode=$($proc.ExitCode) (ログ: $logPath)"
    exit 1
}

Write-Output "[unity-check] 結果: OK (コンパイルエラー 0 件 / 警告 $($warnings.Count) 件)"
exit 0
