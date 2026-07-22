GSL Service Center - Intune Skripte v0.5.13

Wichtig:
Die EXE heisst ab dieser Version:

GSL-Service-Center.exe

Installationspfad:

C:\GSL-Support\GSL-Service-Center\GSL-Service-Center.exe

Paketstruktur fuer Intune:

GSL-Service-Center-Intune\
  install.ps1
  uninstall.ps1
  GSL-Service-Center.exe

Das Skript erstellt die Verknuepfungen selbst.
Eine .lnk-Datei muss nicht mehr ins Paket gelegt werden.

Installationsbefehl:

powershell.exe -ExecutionPolicy Bypass -File .\install.ps1

Deinstallationsbefehl:

powershell.exe -ExecutionPolicy Bypass -File .\uninstall.ps1

Das Installationsskript:

- beendet laufende Prozesse GSL-Service-Center und GSL-Rettungsring
- kopiert GSL-Service-Center.exe nach C:\GSL-Support\GSL-Service-Center
- erstellt Desktop-Verknuepfung fuer alle Benutzer
- erstellt Startmenue-Verknuepfung
- setzt HKLM Autostart
- entfernt alte GSL-Rettungsring-Verknuepfungen und alte Autostart-Eintraege
- startet die App NICHT im SYSTEM-Kontext

Intune Detection per GUI:

Rule type:
File

Path:
C:\GSL-Support\GSL-Service-Center

File or folder:
GSL-Service-Center.exe

Detection method:
File version

Operator:
Greater than or equal to

Value:
0.5.13.0

Kein Detection-Skript verwenden.
Keine Version im PowerShell-Skript hardcoden.
