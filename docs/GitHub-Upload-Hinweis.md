# GitHub-Upload-Hinweis

Dieses Paket ist als Repository-Inhalt vorbereitet.

Vorgehen:

1. ZIP entpacken.
2. Den Inhalt des entpackten Ordners in das Repository-Root hochladen.
3. `news.md` und `news_dev.md` muessen im Repository-Root liegen.
4. `bin/`, `obj/`, `.vs/`, veroeffentlichte EXE-Dateien, ZIPs und `.intunewin`-Dateien nicht hochladen.

Die App liest die News-Dateien aus:

```text
https://api.github.com/repos/gslgrossgmbh/service-center/contents/news.md?ref=main
https://api.github.com/repos/gslgrossgmbh/service-center/contents/news_dev.md?ref=main
```
