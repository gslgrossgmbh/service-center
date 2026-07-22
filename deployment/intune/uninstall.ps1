$ErrorActionPreference = "SilentlyContinue"

$AppName = "GSL Service Center"
$InstallDir = "C:\GSL-Support\GSL-Service-Center"
$ExePath = Join-Path $InstallDir "GSL-Service-Center.exe"

$OldInstallDir = "C:\GSL-Support\GSL-Rettungsring"
$OldExePath = Join-Path $OldInstallDir "GSL-Rettungsring.exe"

$DesktopShortcutPath = Join-Path (Join-Path $env:PUBLIC "Desktop") "GSL Service Center.lnk"
$StartMenuShortcutPath = Join-Path (Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs") "GSL Service Center.lnk"

$OldShortcuts = @(
    (Join-Path (Join-Path $env:PUBLIC "Desktop") "GSL-Rettungsring.lnk"),
    (Join-Path (Join-Path $env:PUBLIC "Desktop") "GSL Support anfordern.lnk"),
    (Join-Path (Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs") "GSL-Rettungsring.lnk"),
    (Join-Path (Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs") "GSL Support anfordern.lnk")
)

$RunKeyPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"

Get-Process -Name "GSL-Service-Center" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process -Name "GSL-Rettungsring" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

Remove-ItemProperty -Path $RunKeyPath -Name "GSL Service Center" -Force -ErrorAction SilentlyContinue
Remove-ItemProperty -Path $RunKeyPath -Name "GSL-Rettungsring" -Force -ErrorAction SilentlyContinue

Remove-Item -Path $DesktopShortcutPath -Force -ErrorAction SilentlyContinue
Remove-Item -Path $StartMenuShortcutPath -Force -ErrorAction SilentlyContinue
foreach ($shortcut in $OldShortcuts) {
    Remove-Item -Path $shortcut -Force -ErrorAction SilentlyContinue
}

Remove-Item -Path $ExePath -Force -ErrorAction SilentlyContinue
Remove-Item -Path $OldExePath -Force -ErrorAction SilentlyContinue

if (Test-Path $InstallDir) {
    $remaining = Get-ChildItem -Path $InstallDir -Force -ErrorAction SilentlyContinue
    if ($null -eq $remaining) {
        Remove-Item -Path $InstallDir -Force -ErrorAction SilentlyContinue
    }
}

if (Test-Path $OldInstallDir) {
    $remainingOld = Get-ChildItem -Path $OldInstallDir -Force -ErrorAction SilentlyContinue
    if ($null -eq $remainingOld) {
        Remove-Item -Path $OldInstallDir -Force -ErrorAction SilentlyContinue
    }
}
