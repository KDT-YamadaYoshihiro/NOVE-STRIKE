<#
  say.ps1 - 日本語音声案内スクリプト
  用途: 作業完了 / インタビュー実施 / 承認依頼 の3タイミングで発話する。
  例:   powershell -ExecutionPolicy Bypass -File .claude/scripts/say.ps1 -Text "作業が完了しました"
#>
param(
    [Parameter(Mandatory = $true)][string]$Text,
    [int]$Rate = 1
)

try {
    Add-Type -AssemblyName System.Speech
    $synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
    $jp = $synth.GetInstalledVoices() |
        Where-Object { $_.VoiceInfo.Culture.Name -eq 'ja-JP' } |
        Select-Object -First 1
    if ($null -ne $jp) { $synth.SelectVoice($jp.VoiceInfo.Name) }
    $synth.Rate = $Rate
    $synth.Speak($Text)
    $synth.Dispose()
}
catch {
    # 音声合成が失敗しても作業自体は止めない
    Write-Output "[say.ps1] 音声案内に失敗しました: $($_.Exception.Message)"
}
