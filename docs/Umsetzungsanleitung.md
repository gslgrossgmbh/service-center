# Umsetzungsanleitung v0.5.13

## Projekt öffnen

```text
src\GSL-Service-Center\GSL-Service-Center.csproj
```

## Veröffentlichen

Visual Studio Publish wie bisher:

- Single File: true
- Self-contained: false
- Runtime: win-x64
- Target Framework: .NET 8 Windows

Erwartete veröffentlichte Datei:

```text
GSL-Service-Center.exe
```

## Intune-Zielpfad

```text
C:\GSL-Support\GSL-Service-Center\GSL-Service-Center.exe
```

## Startargumente

Normale Kundenansicht:

```text
GSL-Service-Center.exe
```

Dashboard direkt öffnen:

```text
GSL-Service-Center.exe --dashboard
```

DEV-News testen:

```text
GSL-Service-Center.exe --news-dev
```

## Aktuelles-Bereich

Die Markdown-Datei wird beim Programmstart per HTTP abgerufen und nur im Arbeitsspeicher verwendet.
Es wird keine lokale `news.md` gespeichert.
Bei Fehler bleibt der eingebaute Standardtext sichtbar.


## News-MD pflegen

Unterstuetzt werden bewusst nur einfache Regeln:

```text
Leerzeile = Abstand
Neue Textzeile = neue Zeile in der App
<br> = neue Zeile in der App
- Aufzaehlung = Bulletpoint
* Aufzaehlung = Bulletpoint
## Ueberschrift = fett dargestellte Zwischenueberschrift
```

Empfohlener Inhalt fuer `news.md`:

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


## Korrektur v0.5.9

Der News-Titel ist linksbuendig zum darunterliegenden Text ausgerichtet.


## Korrektur v0.5.13

Das News-Rendering wertet jetzt zusaetzlich HTML-artige Zeilenumbrueche aus.
Unterstuetzt werden `&lt;br&gt;`, `&lt;br/&gt;` und `&lt;br /&gt;`.

In der Markdown-Datei kann dadurch bewusst ein Umbruch innerhalb eines Absatzes gesetzt werden:

```markdown
Wir begleiten Ihr Unternehmen praxisnah und sicher bei der Einfuehrung.<br>
Von der Vorbereitung bis zur Bereitstellung in Microsoft 365.
```


## Korrektur v0.5.13

Ein `<br>` direkt nach dem Frontmatter wird jetzt als bewusster Abstand nach dem News-Haupttitel ausgewertet. Dadurch kann die MD-Datei gezielt Abstand zwischen Titel und erstem Text erzeugen.

Beispiel:

```markdown
---
title: Zeit sparen mit Microsoft Copilot
moreUrl: https://www.gsl-computer.de/microsoft-copilot/
buttonText: Mehr zu Copilot
---
<br>
Wir begleiten Ihr Unternehmen praxisnah und individuell bei der Einfuehrung.
```
