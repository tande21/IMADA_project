## Deployment Standalone-Version
In dieser Anleitung das Deployment der Standalone-Version des Spiels erklärt.
#### Highscore Funktionalität
Zuerst wird ein Server benötigt, auf dem die Highscore Funktionalität zur Verfügung gestellt wird.
Dieser kann auch lokal auf dem Gerät ausgeführt werden.
Siehe dazu die [Anleitung zum Server](PhishingMasterServer/README.md).
#### Spiel Starten
Nachdem ein Build des Spiels erstellt wurde, kann dies durch Doppelklicken auf die `PhishingMaster.exe` ausgeführt werden.
#### Spiel Einrichten
###### Auswahl des Bilddatensatzes
Im Spiel enthalten sind zwei Bilddatensätze, `real_company_level_1` mit echten Firmennamen und `fake_company_level_1` mit erfundenen Namen.
Standardmäßig wird in der Standalone-Version `real_company_level_1` ausgewählt. Um den anderen `fake_company_level_1`-Datensatz zu nutzen kann das Spiel mit der Option `--web-mails` gestartet werden.
Dazu kann unter Windows eine neue Verknüpfung angelegt werden:
- Im Build-Ordner des Spiels Rechtsklick auf eine freie Fläche -> Neu -> Verknüpfung
- Bei **Geben Sie den Speicherort des Elements ein** die Datei `PhishingMaster.exe` auswählen
- Einen beliebigen Namen für die Verknüpfung vergeben
- Rechtsklick auf die erstellte Verknüfung -> Eigenschaften
- Den Eintrag bei **Ziel:** von `...\PhishingMaster.exe` in  `...\PhishingMaster.exe --web-mails` ändern
###### Anpassung der Grafik-Einstellungen
Standardmäßig startet das Spiel im Vollbildmodus auf dem ersten Display (falls mehrere Monitore angeschlossen sind).
Falls dies geändert werden muss können folgende Schitte verwendet werden um die Einstellung zu ändern.
Diese Einstellungen werden nach dem Ändern auch gespeichert.
Damit beim Starten des Spiels in der **Standalone**-Version die Auflösung, das zu verwendende Display und die Qualitätsstufe eingestellt werden können, sind folgende Schritte durchzuführen:
- Die Datei [WindowsPlayer.exe](https://github.com/Unity-Technologies/DesktopSamples/blob/master/ScreenSelectorExample/ScreenSelectorExampleProject/LauncherExecutable/x64/WindowsPlayer.exe) aus einem Offiziellen Unity-Repository runterladen.
- Die Datei in `WindowsPlayer.exe` in `PhishingMaster.exe` umbenennen und im Build-Ordner die alte PhishingMaster.exe ersetzen.
- Die (neue) `PhishingMaster.exe` durch Doppelklicken starten. Nun sollte ein Einstellungsdialog erscheinen.
In dem Einstellungsdialog können folgende Einstellungen vorgenommen werden:
- Auflösung
- Fenstermodus
- Qualitätsstufe (nur die Option **Low** hat einen Einfluss auf die Qualität. Alle anderen Optionen wählen die hohe Qualitätsstufe aus)
- Auswahl des Displays auf dem das Spiel angezeigt werden soll
Hinweis: Für die Auswahl des Bilddatensatzes muss die Original `PhishingMaster.exe` verwendet werden.
Nach dem Einstellen der Display-Einstellungen kann die Datei einfach wieder durch die Original-Datei ersetzt werden, da die Einstellungen übergreifend in der Windows-Registry gespeichert werden.

#### Aufbau des Spiels als Messestand
Für einen kompletten Standaufbau werden folgende Komponenten benötigt:
- 2 große Bildschirme (Bildschirm A für das Spiel, Bildschirm B für die Bestenliste)
- 1 Computer mit Windows 10
- 1 Xbox One Controller für die Steuerung des Spiels

Es wird ein Computer mit dedizierte Grafikeinheit empfohlen um alle Effekte flüssig darzustellen.
Falls dies nicht möglich ist kann die Grafikoption ‘Low’ zum Reduzieren der Effektqualität ausgewählt werden. 
Die beiden Bildschirme werden mit dem Computer verbunden und so hingestellt, dass Zuschauer zu Beiden gute Sichtbedingungen haben.
Des Weiteren wird eine Person benötigt, welche den Stand betreut und bei Fragen oder Problemen zur Verfügung steht.

Der Ablauf ist dann wie folgt:
- Auf dem Computer wird der lokale Highscore-Server gestartet.
- Die Highscore-Seite wird in einem Webbrowser auf Bildschirm B geöffnet. (sollte unter http://localhost/leaderboard/ erreichbar sein)
- Je nachdem welcher Datensatz verwendet wird, muss die Adresse angepasst werden. Beispiele:
  - http://localhost/leaderboard/?level=real_company_level_1
  - http://localhost/leaderboard/?level=fake_company_level_1
- Durch Drücken der Taste `F11` kann diese im Vollbildmodus dargestellt werden.
- Der Controller wird mit dem Computer verbunden und das Spiel gestartet, sodass es auf Bildschirm A angezeigt wird.
