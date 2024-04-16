## UwAMP Installation und Einrichtung
In dieser Anleitung wird gezeigt, wie man unter Windows einen AMP-Stack mit dem Programm [UwAMP](https://www.uwamp.com/en/) einrichten kann.
Dieses hat den Vorteil, dass es nicht installiert werden muss und so die Daten einfach von einem Gerät auf ein anderes übertragen werden können.

#### Installation
Das Programm kann als zip-Ordner [hier](https://www.uwamp.com/en/?page=download) heruntergeladen werden.
Dieser muss nur noch entpackt werden und das Programm ist einsatzbereit.
Durch Ausführen der Datei UwAMP.exe wird das Program gestartet.
Ein Klick auf den Button `www Site` sollte den Web-Browser öffnen und eine Beispielseite anzeigen.
Nun ist die Intallation abgeschlossen.

#### Einrichtung
Über den Button `www Folder` kann das root-Verzeichnis des Webservers aufgerufen werden.
Hier befinden sich nach der Installation noch Beispieldateien.
Diese können gelöscht werden, sodass der Ordner dann leer ist.

Als nächstes sollten das Anzeigen von PHP-Fehlern ausgeschaltet werde, damit die Antworten des Servers der Highscore-Server-Schnittstelle entsprechen.
Dazu auf den Button `PHP Config` klicken und den Tab `PHP Settings` auswählen.
Hier nun die Option `Display errors` auf `Off` stellen.

Über den Button `PHPMyAdmin` wird das Web-Interface zur Bearbeitung der Datenbank geöffnet.
Hier kann man sich mit dem Nutzernamen `root` und dem Passwort `root` anmelden.

Ab hier kann den Schritten zur [Einrichtung der Datenbank](README.md#einrichtung-der-datenbank) gefolgt werden.
Beim Schritt ['php konfigurieren'](README.md#php-konfigurieren) ist darauf zu achten, dass für die Variable `$hostname` der Wert `localhost` eingetragen wird.
