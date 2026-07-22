$ErrorActionPreference = "Stop"

$AppName = "GSL Service Center"
$OldRunNames = @("GSL-Rettungsring", "GSL Service Center")

$InstallDir = "C:\GSL-Support\GSL-Service-Center"
$OldInstallDir = "C:\GSL-Support\GSL-Rettungsring"
$ExeName = "GSL-Service-Center.exe"
$ExeSource = Join-Path $PSScriptRoot $ExeName
$ExeTarget = Join-Path $InstallDir $ExeName

$ShortcutName = "GSL Service Center.lnk"
$DesktopShortcutTarget = Join-Path (Join-Path $env:PUBLIC "Desktop") $ShortcutName
$StartMenuShortcutTarget = Join-Path (Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs") $ShortcutName

$OldDesktopShortcuts = @(
    (Join-Path (Join-Path $env:PUBLIC "Desktop") "GSL-Rettungsring.lnk"),
    (Join-Path (Join-Path $env:PUBLIC "Desktop") "GSL Support anfordern.lnk")
)
$OldStartMenuShortcuts = @(
    (Join-Path (Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs") "GSL-Rettungsring.lnk"),
    (Join-Path (Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs") "GSL Support anfordern.lnk")
)

$RunKeyPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"

if (-not (Test-Path $ExeSource)) {
    throw "EXE nicht gefunden: $ExeSource"
}

Get-Process -Name "GSL-Service-Center" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process -Name "GSL-Rettungsring" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

New-Item -Path $InstallDir -ItemType Directory -Force | Out-Null
Copy-Item -Path $ExeSource -Destination $ExeTarget -Force

if (-not (Test-Path $ExeTarget)) {
    throw "Installation fehlgeschlagen. EXE nicht gefunden: $ExeTarget"
}

$wshShell = New-Object -ComObject WScript.Shell

$desktopShortcut = $wshShell.CreateShortcut($DesktopShortcutTarget)
$desktopShortcut.TargetPath = $ExeTarget
$desktopShortcut.Arguments = "--dashboard"
$desktopShortcut.WorkingDirectory = $InstallDir
$desktopShortcut.IconLocation = $ExeTarget
$desktopShortcut.Description = $AppName
$desktopShortcut.Save()

$startMenuShortcut = $wshShell.CreateShortcut($StartMenuShortcutTarget)
$startMenuShortcut.TargetPath = $ExeTarget
$startMenuShortcut.Arguments = "--dashboard"
$startMenuShortcut.WorkingDirectory = $InstallDir
$startMenuShortcut.IconLocation = $ExeTarget
$startMenuShortcut.Description = $AppName
$startMenuShortcut.Save()

foreach ($shortcut in $OldDesktopShortcuts + $OldStartMenuShortcuts) {
    Remove-Item -Path $shortcut -Force -ErrorAction SilentlyContinue
}

New-Item -Path $RunKeyPath -Force | Out-Null
foreach ($runName in $OldRunNames) {
    Remove-ItemProperty -Path $RunKeyPath -Name $runName -Force -ErrorAction SilentlyContinue
}
Set-ItemProperty -Path $RunKeyPath -Name $AppName -Value "`"$ExeTarget`""

if (Test-Path $OldInstallDir) {
    Remove-Item -Path (Join-Path $OldInstallDir "GSL-Rettungsring.exe") -Force -ErrorAction SilentlyContinue
    $remaining = Get-ChildItem -Path $OldInstallDir -Force -ErrorAction SilentlyContinue
    if ($null -eq $remaining) {
        Remove-Item -Path $OldInstallDir -Force -ErrorAction SilentlyContinue
    }
}
