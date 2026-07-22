# GSL Service Center

Windows-Tray-App fuer den GSL-Support.

## Stand v0.5.13

Diese Version ist weiterhin bewusst schlank gehalten: keine Update-Logik, keine Logs, kein GitHub-Updater und kein GitHub-Token.

## Wichtige Aenderungen in v0.5.13

- DPI-Stabilisierung fuer unterschiedliche Windows-Skalierungen umgesetzt.
- Das Dashboard verwendet eine feste Mindest-Clientgroesse.
- DPI-Skalierung darf das Fenster vergroessern, aber nicht kleiner als die Zielgroesse machen.
- App-Manifest um PerMonitorV2-DPI-Awareness ergaenzt.
- Button/Link `Zusammenarbeit mit dem GSL-Support` zeigt weiterhin auf `https://www.gsl-computer.de/gsl-support/`.
- Version auf `0.5.13 / 0.5.13.0` gesetzt.

## Markdown-News

Produktiv:

```text
https://api.github.com/repos/gslgrossgmbh/service-center/contents/news.md?ref=main
```

DEV-Modus:

```text
https://api.github.com/repos/gslgrossgmbh/service-center/contents/news_dev.md?ref=main
```

Start DEV-Modus:

```text
GSL-Service-Center.exe --news-dev
```

Die App liest die Datei aus der GitHub-Contents-API, decodiert den Base64-Inhalt und zeigt daraus den Aktuelles-Bereich an.

## Projektdatei

```text
src\GSL-Service-Center\GSL-Service-Center.csproj
```

## Publish-Hinweis

In Visual Studio als framework-dependent Single-File-App veroeffentlichen.

Erwartete EXE:

```text
GSL-Service-Center.exe
```

## Intune

Die Intune-Skripte liegen separat im Paket `gsl-service-center-v0.5.13-intune-skripte.zip`.
Die Detection soll weiterhin ueber die Intune-GUI per Dateiversion erfolgen.


## Markdown-Frontmatter fuer News

Verwendet werden nur diese Felder:

```markdown
---
title: Neues aus dem GSL Service Center
moreUrl: https://www.gsl-computer.de/aktuelles/
buttonText: Mehr erfahren
---
```

Das Feld `date` wird nicht benoetigt und von der App nicht ausgewertet.


## Einfache Markdown-Regeln im Aktuelles-Bereich

```text
Leerzeile = Abstand
Neue Textzeile = neue Zeile in der App
<br> = neue Zeile in der App
- Aufzaehlung = Bulletpoint
* Aufzaehlung = Bulletpoint
## Ueberschrift = fett dargestellte Zwischenueberschrift
```

Empfohlenes Beispiel fuer Copilot:

```markdown
---
title: Microsoft Copilot bereitstellen
moreUrl: https://www.gsl-computer.de/microsoft-copilot/
buttonText: Mehr zu Copilot
---

## Microsoft Copilot fuer Ihr Unternehmen

Wir begleiten Ihr Unternehmen praxisnah und sicher bei der Einfuehrung.<br>
Von der Vorbereitung bis zur Bereitstellung in Microsoft 365.

- Analyse der Voraussetzungen
- Einrichtung und Bereitstellung
- Begleitung fuer eine sichere Nutzung
```
