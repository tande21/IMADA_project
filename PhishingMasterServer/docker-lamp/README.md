## Einrichtung eines LAMP-Stacks mit Docker
Diese Anleitung erklärt, wie man einen LAMP-Stack mit docker erstellen kann.
Dabei gibt es sowohl eine Anleitung mit docker-compose, als auch eine um alle Container einzeln zu konfigurieren.
#### Docker image mit pdo_mysql erstellen
Das offizielle Image php:7.2-apache enthält nicht das Modul pdo_mysql, weshalb wir ein eigenes Image erstellen müssen.
Dazu in den Ordner php7.2-apache-mysql wechseln und folgenden Befehl ausführen:
```bash
$ docker build --no-cache -t php7.2-apache-mysql .
```
Nun haben wir ein Image mit dem Namen php7.2-apache-mysql, das das benötigte Modul enthält.
### Container starten/stoppen
Die Container können entweder mit docker-compose oder manuell gestartet werden.
Dabei wird ein Netzwerk mit dem Namen *phishing-master* erstellt und anschließend werden 3 Container mit den Namen *web-server*, *db* und *phpmyadmin* gestartet.
#### Starten mit docker-compose
Um die Container mit docker-compose zu starten muss folgender Befehl in diesem Verzeichnis ausgeführt werden:
```bash
$ docker-compose up -d
```
#### Mit docker-compose erstellte Container stoppen und löschen
In diesem Verzeichnis ausführen:
```bash
$ docker-compose down
```
#### Container einzeln starten (ohne docker-compose)
Um die Container einzeln zu starten können folgende Befehle verwendet werden.
Zuerst wird ein neues Netzwerk mit dem Namen *phishing-master* erstellt und anschließend die Container gestartet.
**Achtung**: Die Befehle müssen **in diesem Verzeichnis (*docker-lamp*)** ausgeführt werden, da sie mit relativen Pfaden arbeiten.
```bash
$ docker network create phishing-master
$ docker run -d --name="web-server" \
    -e "ALLOW_OVERRIDE=true" \
    -v "$(pwd)/container-data/web-server/html:/var/www/html/" \
    --restart=unless-stopped \
    -p 80:80 \
    --network="phishing-master" \
    php7.2-apache-mysql
$ docker run -d --name="db" \
    -e "MYSQL_ROOT_PASSWORD=change-me" \
    -v "$(pwd)/container-data/db/mysql:/var/lib/mysql" \
    --restart=unless-stopped \
    --network="phishing-master" \
    mariadb
$ docker run -d --name="phpmyadmin" \
    --restart=unless-stopped \
    -p 8890:80 \
    --network="phishing-master" \
    phpmyadmin/phpmyadmin:latest
```
#### Manuell erstellte Container stoppen und löschen (ohne docker-compose)
```bash
$ docker stop phpmyadmin web-server db
$ docker rm phpmyadmin web-server db
$ docker network rm phishing-master
```
