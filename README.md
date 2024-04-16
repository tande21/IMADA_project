## Phishing Master
`Phishing Master` ist ein Videospiel zur Aufklärung über das Thema *Phishing*.
Dieses wurde ursprünglich im Rahmen des Praktikums *KASTEL Praktikum Sicherheit* am [KIT](https://kit.edu) entwickelt und anschließend 2020 im Rahmen des Praktikums *Praktikum Security, Usability and Society* erweitert.
In `Phishing Master` werden dem Spieler nacheinander verschiedene E-Mail- oder Text-Nachrichten gezeigt, bei denen es sich entweder um Phishing-Versuche oder legetime Nachrichten handelt.
Die Aufgabe des Spielers ist es dann zu entschieden um was für eine Nachricht es sich handelt.
## Kompatibilität
Die Webversion von `Phishing Master 3.0.0` wurde mit verschiedenen Systemkonfigurationen getestet.
Die Bedienung mit der Maus funktioniert bei jeder getesteten Konfiguration.
Dabei ist zu beachten, dass das Zoom-Level des Browsers auf 100% eingestellt sein muss.
Für die Steuerung mit einem Gamepad (DS4/Xbox One) werden folgende Browser für die höchste Kompatibilität empfohlen:
 - Windows: Chrome, Firefox, Opera, Edge(Chrome)
 - Linux: Chromium
 - MacOS: Opera

 Andere Browser können jedoch auch funktionieren, für genauere Details die folgende Übersicht anschauen:
<details>
<summary markdown="span">Übersicht über die Systemkonfigurationen</summary>

| Browser/OS    |   Windows (10)  |   Linux (Ubuntu 18.04)    |       MacOS (Catalina)     |
| :------------ | --------------- | ------------------------- | --------------- |
| Chrome/Chromium   | Maus: **funktioniert**<br>Xbox One: **funktioniert**<br>DS4: **funktioniert** | Maus: **funktioniert**<br>Xbox One: **funktioniert**<br>DS4: eingeschränkt | Maus: **funktioniert**<br>Xbox One: funktioniert nicht<br>DS4: **funktioniert** |
| Firefox           | Maus: **funktioniert**<br>Xbox One: **funktioniert**<br>DS4: **funktioniert** | Maus: **funktioniert**<br>Xbox One: eingeschränkt[3]<br>DS4: eingeschränkt[2] | Maus: **funktioniert**<br>Xbox One: funktioniert nicht<br>DS4: **funktioniert** |
| Opera             | Maus: **funktioniert**[1]<br>Xbox One: **funktioniert**<br>DS4: **funktioniert** | Maus: ---<br>Xbox One: ---<br>DS4: --- | Maus: **funktioniert**[1]<br>Xbox One: **funktioniert**<br>DS4: **funktioniert** |
| Edge (Chrome)     | Maus: **funktioniert**<br>Xbox One: **funktioniert**<br>DS4: **funktioniert** | Maus: ---<br>Xbox One: ---<br>DS4: --- | Maus: **funktioniert**<br>Xbox One: funktioniert nicht<br>DS4: **funktioniert** |
| Safari            | Maus: ---<br>Xbox One: ---<br>DS4: --- | Maus: ---<br>Xbox One: ---<br>DS4: --- | Maus: eingeschränkt[4]<br>Xbox One: ---<br>DS4: --- |

Testdatum: 2020-08-07 - Es wurden jeweils die aktuellen Browserversionen verwendet.

**`funktioniert`**: Es wurden keine Probleme festgestellt.<br>
`eingeschränkt`: Es wurden Probleme festgestellt, die die Bedienung einschränken.<br>
`funktioniert nicht`: Steuerung nicht möglich.<br>
`---`: Nicht getestet.<br>
[1]: Mausgesten sollten deaktiviert werden.<br>
[2]: Tasten anders belegt, Menü-Auswahl dauernd nach links.<br>
[3]: Untere Schultertasten funktionieren nicht, Regeln durchwechseln nicht möglich.<br>
[4]: Mausempfindlichkeit sehr gering.
</details>

## Installation
Das Spiel wurde in der **Unity-Version 2019.4.5** entwickelt und kann mit dieser Version direkt geladen werden.
Zur Nutzung der **Highscore-Funktionalität** wird zusätzlich ein Webserver benötigt, siehe [PhishingMasterServer](PhishingMasterServer/).
## Konfiguration des Spiels
Die meisten Konfigurationsparameter sind im [DataManager](Assets/PhishingMaster/scripts/DataManager.cs) zu finden.
Im Folgenden wird auf die Parameter eingegangen, die beachtet werden sollten.
#### Level
Es gibt zwei verschiedene Sets mit Nachrichten, eines mit echten Markennamen und eines mit Erfundenen.
Zusätzlich gibt es auch noch ein Trainings-Level.
Für jedes Level gibt es sowohl **Bilddateien**, die die Nachricht darstellen, als auch **XML Dateien** für die Phishing Nachrichten, in denen verschiedene Eigenschaften beschrieben werden.
Die Level sind unter [Assets/Resources/Images](Assets/Resources/Images) und [Assets/Resources/XML](Assets/Resources/XML) zu finden.
Sowohl im Web- als auch im Standalone-Build sind beide Datensätze enthalten.
Die Auswahl des zu verwendenden Datensatzes erfolgt im [DataManager](Assets/PhishingMaster/scripts/DataManager.cs).
Dabei wird im Web-Build standardmäßig das Level mit den erfundenen Firmennamen und im Standalone-Build mit den echten Firmennamen gewählt.
###### Konfiguration in Unity
Wo die Level zu finden sind, wird im Spiel im [DataManager](Assets/PhishingMaster/scripts/DataManager.cs) durch die Variable `availableLevels` festgelegt.
```csharp
private static Level[] availableLevels = {
        new Level("training_level", "Training", "Training/", 10, false),
        new Level("real_company_level_1", "Punktemodus", "Level 1/", 10, true), //Level with real company names
        new Level("fake_company_level_1", "Punktemodus", "Level 2/", 10, true)}; //Level with fake company names used for web_version
```
Wie die einzelnen Parameter gewählt werden können und **welche Namen zulässig** sind ist im Code dokumentiert.
###### Hinzufügen neuer Nachrichten
Um eine neue Nachricht hinzuzufügen muss die **Bilddatei** im entsprechenden Ordner unter [Assets/Resources/Images](Assets/Resources/Images) plaziert werden.
Zusätzlich muss nun in Unity bei den **Import-Einstellungen** für das Bild das **Aniso Level auf 3** gesetzt und der Parameter **Non-Power of 2** auf **'None'** gestellt werden, damit die Bilder im richtigen Seitenverhältnis und auch bei höherer Entfernung scharf dargestellt werden.

Handelt es sich um eine Phishing-Nachricht muss zusätzlich eine **XML Datei** mit dem gleichen Namen wie die Bilddatei im entsprechenden Ordner unter [Assets/Resources/XML](Assets/Resources/XML) platziert werden.
Das Format sollte entsprechend den bereits vorhandenen Dateien sein.
Dabei geben die Werte `x1,y1,x2,y2` den Bereich an, indem sich das Erkennungsmerkmal der Phishing-Nachricht befindet.
Unter `description` wird der Text eingefügt, der dem Spieler am Ende als Erklärung angezeigt werden soll.
#### Highscore-Server
###### Server-Adresse
Die Adresse des Highscore Servers kann im [DataManager](Assets/PhishingMaster/scripts/DataManager.cs) angepasst werden.
Standardmäßig sind hier folgende Werte eingetragen:
```csharp
public static readonly string[] HIGHSCORE_SERVER_ADDRESSES = 
    { "", 
    "http://phishing-master-highscore-server.local", 
    "http://localhost" };
```
Die Adressen werden in der angegebenen Reihenfolge probiert.
Die erste, leere Adresse sorgt dafür, dass die Highscore-Schnittstelle unter der gleichen URL gesucht wird, wie das Spiel.
Somit können das Spiel (in der WebGL-Version) und die Highscore-Schnittstelle auf dem **gleichen Webserver** betrieben werden ohne die Adresse explizit angeben zu müssen.
Die zweite Adresse ist für die **Entwicklung** da und sollte vor einem Production-Build entfernt werden.
Die dritte Adresse versucht eine Verbindung zu einem **lokalen Highscore-Server** (z.B. für die Standalone-Version).
###### Authentifizierung
Zur Authentisierung gegenüber dem Highscore-Server wird beim Eintragen einer neuen Punktzahl ein Hashwert mitgesedet.
Dieser basiert auf den übersendeten Informationen und einem Geheimnis (`secretKey`).
Dieses Geheimnis muss sowohl im Highscore-Server, wie in der [README](PhishingMasterServer/README.md) beschrieben, als auch im Spiel hinterlegt werden.
Im Spiel wird dieser Wert im [DataManager](Assets/PhishingMaster/scripts/DataManager.cs) durch die Variable `HIGHSCORE_SECRET_KEY` festgelegt.
#### Optional
###### PlayerPrefs
In den PlayerPrefs können lokal Einstellungen gespreichert werden.
Diese werden bei der Standalone-Version in der Windows-Registry gespeichert und bei der Web-Version als Cookie (IndexedDB).
Folgende Variablen werden vom Spiel gesetzt:
- `Score` (Score der aktuellen Spielrunde)
- `Tutorial_Complete` : Gibt an, ob das Tutorial bereits einmal abgeschlossen wurde
- `HighScore` : Gibt den bisherigen Highscore auf dem Gerät an
- `resultsWindowInfo` : Gibt an, ob der Hinweis zum Results-Bildschirm bereits gelesen wurde
- `Player_Name` : Gibt den zuletzt vom Spieler eingegebenen Namen an
###### (Development) Tutorial überspringen
Standardmäßig kann man das Tutorial erst überspringen, wenn man es bereits einmal abgeschlossen hat.
Um dies während der Entwicklung zu deaktivieren, kann die Variable `disableTutorialCompleteCheck` im [DataManager](Assets/PhishingMaster/scripts/DataManager.cs) auf `true` gesetzt werden.
So wird einem jedes mal die Auswahl angezeigt.
## Build des Spiels erstellen
Bevor ein Build des Spiels erstellt wird, sollten die Schritte im Abschhnitt **Konfiguration** beachtet werden.
Vor der Erstellung eines neuen Builds müssen eventuell nachdem das Build-Target (PC oder WebGL) in Unity ausgewählt wurde die Addressables neu erstellt werden. Dazu Window -> Asset-Management -> Addressables -> Groups auswählen und dann Build -> Clear Build Cache -> All anklicken. Dannach über Build -> New Build -> Default Build Script die Addressables neu erstellen.
#### Standalone-Version
Zum Erstellen eines Builds der Standalone-Version muss nichts zusätzlich beachtet werden.
Einfach in Unity auf File -> Build Settings -> PC Standalone -> Build.
#### Web
In dieser Dokumentation wird davon ausgegangen, dass das Build in einem Ordner mit der Bezeichnung `htdocs` erstellt wird.
Dazu in Unity auf File -> Build Settings -> WebGL -> Build und dann einen Ordner mit dem Namen `htdocs` erstellen/auswählen.
###### Probleme mit Maus-Input im Browser
Damit es nicht zu Problemen mit der Mausposition in der Web-Version kommt, wenn der Zoom nicht auf 100% gestellt ist oder eine Skalierung duch das Betriebssystem aktiviert wurde, muss Folgendes beachtet werden.
In den **Player Settings** in Unity unter dem Punkt **Publishing Setting** muss der **Compression Format** auf **Disabled** gesetzt werden.
Dadurch kann nach erfolgreichem Erstellen des Builds die Datei `Build/htdocs.wasm.framework.unityweb` bearbeitet werden.
In dieser Datei sollten Zwei Einträge für den Suchbegriff `devicePixelRatio=window.devicePixelRatio` vorhanden sein.
An beiden Stelle muss der Wert durch 1 ersetzt werden, wie hier beispielhaft dargestellt:
```javascript
...var devicePixelRatio=window.devicePixelRatio;...
ersetzen durch:
...var devicePixelRatio=1;...
```

## Deployment
Für Informationen, wie man das Spiel anderen zur Verfügung stellen kann und es bedient wird, die [Deployment Dokumentation](Docs/Deployment/) beachten

## Quellen und Lizens
###### Verwendete Unity Assests aus dem Asset Store
Das Spiel basiert auf der offiziellen FPS Microgame Vorlage. Außerdem wurden folgende kostenlose Assets verwendet:
- https://assetstore.unity.com/packages/2d/textures-materials/sky/skybox-series-free-103633
- https://assetstore.unity.com/packages/essentials/asset-packs/unity-particle-pack-5-x-73777
- https://assetstore.unity.com/packages/3d/environments/snaps-prototype-office-137490
- https://assetstore.unity.com/packages/2d/textures-materials/milky-way-skybox-94001 (entfernt)
###### Lizenz
Dieses Projekt enthält Dateien von verschiedenen Quellen, wie beispielsweise die Unity Assets oder Bilddateien.
Die entsprechenden Lizenzen für die einzelnen Dateien sind in der jeweiligen `LICENSE.md` Datei in den entsprechenden Ordnern angegeben.

Alle von uns erstellten Dateien und Veränderungen steht unter der MIT Lizenz.

MIT License

Copyright (c) 2020 Tobias Länge and Philipp Matheis

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
