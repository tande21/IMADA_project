## Phishing Master Server
In diesem Repository befinden sich die Serveranwendugen zum Projekt "Phishing Master".
#### Einrichtung des Highscore-Servers
##### Einrichtung des Web-Servers
Zur Einrichtung des Highscore-Servers wird ein AMP-Stack (Apache, MySQL, and PHP) benötigt.
###### Web-Version
Für die Web-Version wird ein Linux-System mit einem AMP-Stack empfohlen.
Im Order [`docker-lamp`](docker-lamp/) liegen alle benötigten Dateien um einen Server für die Highscore-Funktionalität bereitzustellen.
Dieser kann auch genutzt werden um die Website zu hosten, dazu einfach die Dateien aus dem [`web-server`](web-server/) Ordner im [`html`](docker-lamp/web-server/html/) Ordner des Webservers ablegen.
###### Standalone-Version
Alternativ kann für die Standalone-Version unter Windows lokal ein AMP-Stack eingerichtet werden, indem man sich ein Programm wie beispielsweise [UwAMP](https://www.uwamp.com/en/), [WAMP](https://www.wampserver.com/en/) oder [XAMPP](https://www.apachefriends.org/de/index.html) herunterlädt.
Hierbei müssen dann die Dateien aus dem [`html`](docker-lamp/web-server/html/) Ordner in das root-Verzeichnis des Webservers gelegt werden.
Außerdem sollten die Dateien aus dem [`web-server`](web-server/) Ordner hinzugefügt werden um beispielsweise das Leaderboard im Browser öffnen zu können.
Die Einrichtung unter Windows zur Nutzung für die Standalone-Version wird in der [UwAMP-Server](UwAMP-Server.md) Anleitung gezeigt.
##### Einrichtung der Datenbank
Zum Konfigurieren der Datenbank das Webinterface von phpmyadmin aufrufen.
(Hinweis: Falls die Docker-Anleitung genutzt wurde unter http://localhost:8890 und mit dem Nutzernamen `root` und dem Passwort `change-me` anmelden.)
Als erstes sollte das Passwort geändert werden.
Dazu auf den Knopf **Change password** klicken.
Anschließend soll eine neue Datenbank und Tabelle angelegt werden.
Dazu auf den Tab Databases klicken und eine neue Datanbank mit dem Namen **highscores** und der Collation **utf8mb4_unicode_ci** anlegen.
Nun den Tab SQL auswählen und folgenden SQL Queries ausführen (Dabei die Passwörter für die Nutzer php-add-score und php-select-score durch Eigene ersetzen):
```sql
CREATE TABLE `highscores`.`scores` ( `id` INT NOT NULL AUTO_INCREMENT , `name` VARCHAR(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'anonymous' , `score` INT NOT NULL DEFAULT '0' , `level` VARCHAR(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL , `timestamp` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP , `challengename` VARCHAR(30) NOT NULL DEFAULT '' , PRIMARY KEY (`id`)) ENGINE = InnoDB;

CREATE USER 'php-add-score'@'%' IDENTIFIED BY 'php-add-score-password';GRANT USAGE ON *.* TO 'php-add-score'@'%' REQUIRE NONE WITH MAX_QUERIES_PER_HOUR 0 MAX_CONNECTIONS_PER_HOUR 0 MAX_UPDATES_PER_HOUR 0 MAX_USER_CONNECTIONS 0; 
GRANT INSERT ON `highscores`.`scores` TO 'php-add-score'@'%';

CREATE USER 'php-select-score'@'%' IDENTIFIED BY 'php-select-score-password';GRANT USAGE ON *.* TO 'php-select-score'@'%' REQUIRE NONE WITH MAX_QUERIES_PER_HOUR 0 MAX_CONNECTIONS_PER_HOUR 0 MAX_UPDATES_PER_HOUR 0 MAX_USER_CONNECTIONS 0; 
GRANT SELECT ON `highscores`.`scores` TO 'php-select-score'@'%';
```
##### (Optional) Starteinträge für das Leaderboard
Folgende SQL Befehle können ausgeführt werden, um die Datenbank mit Starteinträgen zu füllen.
```sql
INSERT INTO `scores` (`id`, `name`, `score`, `level`, `timestamp`, `challengename`) VALUES (NULL, 'Tom', '642', 'fake_company_level_1', CURRENT_TIMESTAMP - INTERVAL 1 MINUTE, '');
INSERT INTO `scores` (`id`, `name`, `score`, `level`, `timestamp`, `challengename`) VALUES (NULL, 'Mike', '938', 'fake_company_level_1', CURRENT_TIMESTAMP - INTERVAL 2 MINUTE, '');
INSERT INTO `scores` (`id`, `name`, `score`, `level`, `timestamp`, `challengename`) VALUES (NULL, 'Anna', '950', 'fake_company_level_1', CURRENT_TIMESTAMP - INTERVAL 3 MINUTE, '');
INSERT INTO `scores` (`id`, `name`, `score`, `level`, `timestamp`, `challengename`) VALUES (NULL, 'George', '878', 'fake_company_level_1', CURRENT_TIMESTAMP - INTERVAL 4 MINUTE, '');
INSERT INTO `scores` (`id`, `name`, `score`, `level`, `timestamp`, `challengename`) VALUES (NULL, 'Fabienne', '778', 'fake_company_level_1', CURRENT_TIMESTAMP - INTERVAL 5 MINUTE, '');
```

##### PHP konfigurieren
Damit PHP Zugriff auf die Datenbank hat müssen die Passwörter aus dem vorherigen Schritt in die entsprechenden Skripte eingetragen werden.
Dazu die Dateien im [`php`](docker-lamp/container-data/web-server/html/php/) Ordner bearbeiten und die Passwörter eintragen.
Außerdem muss die Variable `$hostname` angepasst werden, wenn beispielsweise die Datenbank unter `localhost` erreichbar ist.

##### (Optional) Zugriff von unterschielichen Domains
Falls der Webserver mit den php-Dateien und die das eigentliche Spiel auf unterschiedlichen Domains liegen (beispielsweise während der Entwicklung), muss folgendes durchgeführt werden, damit der Webserver Anfragen von anderen Domains erlaubt.
Im Ordner `container-data/web-server/html/php` muss eine Datei mit dem Namen `.htaccess` und folgendem Inhalt erstellt werden:
```
Header set Access-Control-Allow-Origin '*'
```
Außerdem muss das Modul 'Headers' für den apache web server aktiviert werden.
Dazu können folgende Befehle verwendet werden, während die Container laufen (web-server durch den Namen des web-server Containers ersetzen):
```bash
$ docker exec web-server a2enmod headers
$ docker exec web-server service apache2 restart
```

#### Einrichtung der Spiel-Website
Die Dateien im Ordner [`web-server`](web-server/) können auf einem Webserver abgelegt werden um das Spiel im Interet verfügbar zu machen.
Dazu müssen zusätzlich noch die Dateien aus dem Web-Build von Unity hinzugefügt werden (ohne die `index.html`).
###### (Optional) Build-Ordner-Name anpassen
Falls für den Build-Ordner nicht die Bezeichnung `htdocs` verwendet wurde, muss in der Datei script.js eine Zeile in der Funktion `loadGame()` angepasst werden:
```js
function loadGame() {
	var unityInstance = UnityLoader.instantiate("unityContainer", "Build/htdocs.json", {onProgress: UnityProgress});
}
```
Hier muss der Pfad `Build/htdocs.json` entsprechend der Bezeichnung aus dem Unity Build abgeändert werden.
Dazu einfach im Order Build nachschauen wie die entsprechende .json Datei heißt.
##### Einstellungsmöglichkeiten für die Website
Hier werden einige Einstellungen aufgelistet, die eventuell vorgenommen werden sollten.
###### Adresse(n) des Highscore-Servers
In der Datei `web-server/script.js` kann folgende Zeile angepasst werden um weitere Adressen hinzuzufügen, unter denen der Highscore-Server erreichbar sein könnte.
```js
// Address used to contact highscore server.
// Use "" to use the same address where this page is hosted.
// Addresses will be used in array order, if the first address does not work.
var highscoreServerAddresses = ["", "http://phishing-master-highscore-server.local/", "http://localhost/"];
```
Solange sich das Spiel und die php-Dateien auf dem selben Server befinden sollte der Eintrag `""` ausreichen.
###### Impressum und Datenschutzerklärung
Standardmäßig verweißen die Knöpfe auf `/impressum` und `/datenschutzerklaerung`.
Falls diese zu einer anderen Seite verweißen sollen, kann folgende Zeile in der `index.html` entsprechend angepasst werden.
```html
<div style="float:left;"><a href="/impressum">Impressum</a><a href="/datenschutzerklaerung" style="margin-left:10px;">Datenschutzerklärung</a></div>
```
#### Leaderboard-Seite
Im Ordner [`leaderboard`](web-server/leaderboard/) befindet sich eine einfache HTML-Seite zum Darstellen der Highscore-Liste.
Diese kann beispielsweiße bei einem Event im Vollbildmodus auf einem Bildschirm angezeigt werden.
Die Seite ruft automatisch alle 3 Sekunden die aktuellen Ergebnisse vom Server ab.
Falls ein externer Highscore-Server verwendet wird, muss in der Datei [`leaderboard/script.js`](web-server/leaderboard/script.js) die Adresse des Highscore-Servers angegeben werden.
```js
var highscoreServerAddresses = ["", "http://phishing-master-highscore-server.local/", "http://localhost/"];
```
#### Ordnerstruktur des Webservers
Wenn der Webserver für das Spiel inklusive Highscore-Server Funktionalität fertig eingerichtet ist, sollte das root-Verzeichnis des Webservers wie folgt aussehen (in dem Beispiel heißt das root-Verzeichnis `html`):
```
html
│   dependencies.txt
│   index.html
│   ProjectVersion.txt
│   script.js
│   style-unity.css
│   style.css
│
├───Build
│       htdocs.data.unityweb
│       htdocs.json
│       htdocs.wasm.code.unityweb
│       htdocs.wasm.framework.unityweb
│       UnityLoader.js
│
├───images
│       game_background.jpg
│
├───leaderboard
│       index.html
│       script.js
│       style.css
│
├───php
│       add-score.php
│       select-score.php
│
└───TemplateData
        favicon.ico
        fullscreen.png
        progressEmpty.Dark.png
        progressEmpty.Light.png
        progressFull.Dark.png
        progressFull.Light.png
        progressLogo.Dark.png
        progressLogo.Light.png
        style.css
        UnityProgress.js
        webgl-logo.png
```

#### Schnittstellen des Highscore-Servers
Auf die Datenbank kann über die zwei PHP-Skripte im [`php`](docker-lamp/container-data/web-server/html/php/) Ordner zugegriffen werden.
Eines zum Hinzufügen einer 'Score' und eines zum Auslesen.
##### add-score.php
Das Skript zum hinzufügen von neuen Scores benötigt vier Argumente:
- name
- score
- level
- hash
- challengename (optional)
Dabei wird der Hash verwendet, um zu Überprüfen, ob die Anfrage tatsächlich vom Spiel stammt indem ein geheimer Wert ("secretKey") zur Berechnung verwendet wird.
Der Hash-wert wird folgendermaßen berechnet:
```
hash = sha256(name + score + level + secretKey)
```
Stimmt der übergebene Hash mit dem berechneten überein, werden die Werte in die Datenbank eingetragen und Statuscode 200 wird zurückgegeben.
Ansonsten und bei jedem anderen Fehler wird Statuscode 500 zurückgegeben.
###### Beispiel
Mit `secretKey = "mySecretKey";` in *add-score.php* sollte folgendes Beispiel Statuscode 200 zurückgegeben:
```bash
$ curl -v "http://localhost/php/add-score.php?name=test&score=100&level=testLevel&hash=0d44acb726e824dd7e0596986cf839f8a7c47be8c42f913db29c3be1e44738f6"
```
##### select-score.php
Das Skript zum Abrufen von Highscore Werten kann ohne Argumente aufgerufen werden.
Dann werden alle Einträge zurückgegeben.
Zusätzlich können folgende Parameter übergeben werden:
- name
- level
- count
- order
- fromPos
- id
Der Parameter 'level' ist *erforderlich*.
Beim *optionalen* Parameter 'name' können auch SQL Wildcards wie '%' verwendet werden.
'count' und 'fromPos' können *optional* zum Begrenzen der Antwort verwendet werden.
Der Parameter 'order' ist *optional*.
Wird der Parameter 'id' verwendet werden die anderen Parameter ignoriert und nur dieser Eintrag (sofern vorhanden) zurückgegeben.
Standardmäßig wird nach score und timestamp sortiert, sodass bei gleicher Punktzahl der neuere Eintrag oben ist.
Durch übergabe des Wertes `order=time` wird nur nach timestamp sortiert um die neusten Einträge zu erhalten.
Die Rückgabe ist im JSON Format und der Statuscode ist 200.
Bei Fehlern wird der Statuscode 500 zurückgegeben.
###### Beispiel
```bash
$ curl "http://localhost/php/select-score.php"
```
Beispielausgabe:
```bash
  [{"name":"test","score":100,"level":"testLevel","challengename":"","timestamp":"2020-05-23 18:30:50"}]
```
