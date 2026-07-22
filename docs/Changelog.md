# Changelog

## 0.5.13

- DPI-Stabilisierung fuer das Dashboard umgesetzt.
- Fenster verwendet eine feste Mindest-Clientgroesse und darf durch DPI-Skalierung nur groesser, nicht kleiner werden.
- App-Manifest um PerMonitorV2-DPI-Awareness ergaenzt.
- Version auf `0.5.13 / 0.5.13.0` gesetzt.

## 0.5.12

- URL fuer `Zusammenarbeit mit dem GSL-Support` korrigiert auf `https://www.gsl-computer.de/gsl-support/`.
- Version auf `0.5.12 / 0.5.12.0` gesetzt.

## 0.5.11

- Fuehrendes `<br>` direkt nach dem Frontmatter wird jetzt als Abstand nach dem News-Haupttitel ausgewertet.
- Bestehendes `<br>`-Rendering fuer normale Zeilenumbrueche bleibt erhalten.
- Version auf `0.5.11 / 0.5.11.0` gesetzt.

## 0.5.10

- `<br>`, `<br/>` und `<br />` werden im News-Markdown als Zeilenumbruch ausgewertet.
- Bestehendes News-Rendering bleibt bewusst einfach: Leerzeilen, Aufzaehlungen und `##` werden weiterhin ausgewertet.
- Version auf `0.5.10 / 0.5.10.0` gesetzt.

## 0.5.9

- News-Haupttitel und News-Inhalt werden jetzt im selben Textfeld gerendert.
- Dadurch beginnt der Titel exakt an derselben sichtbaren linken Textkante wie der darunterliegende Inhalt.
- Keine funktionalen Aenderungen am News-Abruf.
- Version auf `0.5.9 / 0.5.9.0` gesetzt.

## 0.5.8

- Versuch zur optischen Ausrichtung des News-Titels korrigiert.

## 0.5.7

- Versuch zur optischen Ausrichtung des News-Titels umgesetzt.

## 0.5.6

- Markdown-Regeln fuer den Aktuelles-Bereich vereinfacht.
- Zeilenumbrueche und einfache Leerzeilen werden sichtbar ausgewertet.
- Aufzaehlungen mit `- ` oder `* ` werden als Bulletpoints dargestellt.
- Inhaltslaenge wird auf maximal 6 Inhaltszeilen begrenzt, damit der Bereich nicht ueberlaeuft.
- Copilot-Beispiel fuer `news.md` ergaenzt.

## 0.5.5

- Markdown-Darstellung im Aktuelles-Bereich verbessert.
- `date` wird nicht mehr als notwendiges Frontmatter-Feld dokumentiert.

## 0.5.4

- News-Abruf auf die oeffentliche GitHub-Contents-API umgestellt.
- Kein GitHub-Token erforderlich.

## 0.5.3

- DEV-Refresh-Button als sichtbares Overlay oben rechts eingebaut.

## 0.5.2

- DEV-Refresh-Button und Tray-Rechtsklick-Verhalten ueberarbeitet.