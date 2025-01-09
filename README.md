## Phishing Master (Translated version)
`Phishing Master` is a video game designed to educate about the topic of *phishing*.  
It was originally developed as part of the internship *KASTEL Praktikum Sicherheit* at the [KIT](https://kit.edu) and then expanded in 2020 as part of the internship *Praktikum Security, Usability and Society*.  
In `Phishing Master`, the player is shown various email or text messages in succession, which can be either phishing attempts or legitimate messages.  
The player's task is then to decide what type of message is being shown.

## Compatibility
The web version of `Phishing Master 3.0.0` has been tested with various system configurations.  
Mouse control worked for each tested configuration. Please note that the browser’s zoom level must be set to 100%.  

For gamepad control (DS4/Xbox One), the following browsers are recommended for maximum compatibility:

- Windows: Chrome, Firefox, Opera, Edge(Chrome)
- Linux: Chromium
- MacOS: Opera

Other browsers may also work. For more details, see the following overview:

<details>
<summary markdown="span">Overview of system configurations</summary>

| Browser/OS          | Windows (10)                                                  | Linux (Ubuntu 18.04)                                        | MacOS (Catalina)                                           |
| :------------------ | :------------------------------------------------------------ | :---------------------------------------------------------- | :--------------------------------------------------------- |
| Chrome/Chromium     | Mouse: **works**<br>Xbox One: **works**<br>DS4: **works**     | Mouse: **works**<br>Xbox One: **works**<br>DS4: limited     | Mouse: **works**<br>Xbox One: does not work<br>DS4: **works** |
| Firefox             | Mouse: **works**<br>Xbox One: **works**<br>DS4: **works**     | Mouse: **works**<br>Xbox One: limited[3]<br>DS4: limited[2] | Mouse: **works**<br>Xbox One: does not work<br>DS4: **works** |
| Opera               | Mouse: **works**[1]<br>Xbox One: **works**<br>DS4: **works**  | Mouse: ---<br>Xbox One: ---<br>DS4: ---                     | Mouse: **works**[1]<br>Xbox One: **works**<br>DS4: **works**  |
| Edge (Chrome)       | Mouse: **works**<br>Xbox One: **works**<br>DS4: **works**     | Mouse: ---<br>Xbox One: ---<br>DS4: ---                     | Mouse: **works**<br>Xbox One: does not work<br>DS4: **works** |
| Safari              | Mouse: ---<br>Xbox One: ---<br>DS4: ---                       | Mouse: ---<br>Xbox One: ---<br>DS4: ---                     | Mouse: limited[4]<br>Xbox One: ---<br>DS4: ---             |

Date tested: 2020-08-07 – Current browser versions at that time were used.

**`works`**: No problems discovered.  
`limited`: We encountered problems that restrict usage.  
`does not work`: Control not possible.  
`---`: Not tested.  
[1]: Mouse gestures should be disabled.  
[2]: Buttons are mapped differently; menu selection constantly moves left.  
[3]: Lower shoulder buttons do not work; cannot cycle through rules.  
[4]: Very low mouse sensitivity.

</details>

## Installation
The game was developed using **Unity version 2019.4.5** and can be loaded directly with this version.  
To use the **Highscore functionality**, an additional web server is required. See [PhishingMasterServer](PhishingMasterServer/) for details.

## Game Configuration
Most configuration parameters can be found in the [DataManager](Assets/PhishingMaster/scripts/DataManager.cs).  
Below are the parameters that should be noted.

#### Level
There are two different sets of messages: one set with real brand names and one set with fictional ones. Additionally, there is a training level.  
For each level, there are **image files** that depict the message, as well as **XML files** (for phishing messages) in which various properties are described.  
These levels can be found under [Assets/Resources/Images](Assets/Resources/Images) and [Assets/Resources/XML](Assets/Resources/XML).  
Both data sets are included in both the web and standalone builds.  
The selection of the data set to use is made in the [DataManager](Assets/PhishingMaster/scripts/DataManager.cs). By default, the web build uses the set with fictional company names, and the standalone build uses the set with real brand names.

##### Configuration in Unity
The [DataManager](Assets/PhishingMaster/scripts/DataManager.cs) uses the `availableLevels` variable to specify where the levels can be found:

```csharp
private static Level[] availableLevels = {
    new Level("training_level", "Training", "Training/", 10, false),
    new Level("real_company_level_1", "Score Mode", "Level 1/", 10, true), // Level with real company names
    new Level("fake_company_level_1", "Score Mode", "Level 2/", 10, true)}; // Level with fake company names used for web_version
```

The code contains documentation on how the individual parameters can be chosen and **which names are allowed**.

##### Adding New Messages
To add a new message, place the **image file** in the corresponding folder under [Assets/Resources/Images](Assets/Resources/Images).  
Next, in Unity, open the **Import Settings** for that image and set **Aniso Level** to **3** and **Non-Power of 2** to **‘None’**, so that the images are displayed with the correct aspect ratio and remain sharp at greater distances.

If it is a phishing message, an additional **XML file** with the same name as the image file must be placed in the corresponding folder under [Assets/Resources/XML](Assets/Resources/XML).  
Its format should match the already existing files.  
The values `x1, y1, x2, y2` define the area where the phishing message’s identifying feature is located. Under `description`, enter the text that will be shown to the player at the end as an explanation.

#### Highscore Server
##### Server Address
The address of the highscore server can be adjusted in [DataManager](Assets/PhishingMaster/scripts/DataManager.cs).  
By default, the following values are set:

```csharp
public static readonly string[] HIGHSCORE_SERVER_ADDRESSES =
    { "",
    "http://phishing-master-highscore-server.local",
    "http://localhost" };
```

These addresses are tried in the specified order.  
The first empty address ensures that the highscore interface is searched for under the same URL as the game. Therefore, if the WebGL version of the game and the highscore interface run on the **same web server**, you do not need to explicitly specify the address.  
The second address is used for **development** and should be removed before creating a production build.  
The third address attempts to connect to a **local highscore server** (e.g., for the standalone version).

##### Authentication
When entering a new score, a hash value is sent for authentication to the highscore server. This hash is based on the information submitted and a secret (`secretKey`).  
This secret must be stored both on the highscore server, as described in the [README](PhishingMasterServer/README.md), and in the game.  
In the game, this value is set via the `HIGHSCORE_SECRET_KEY` variable in [DataManager](Assets/PhishingMaster/scripts/DataManager.cs).

#### Optional
##### PlayerPrefs
Settings can be stored locally via PlayerPrefs.  
For the standalone version, these are saved in the Windows registry; for the web version, they are saved as a cookie (IndexedDB).  
The following variables are set by the game:

- `Score` (the score of the current game round)
- `Tutorial_Complete`: indicates whether the tutorial was completed at least once
- `HighScore`: shows the previous high score on the device
- `resultsWindowInfo`: indicates whether the hint about the Results screen has already been displayed
- `Player_Name`: stores the player’s last entered name

##### (Development) Skip Tutorial
By default, the tutorial can only be skipped if it has been completed at least once.  
To disable this during development, set the variable `disableTutorialCompleteCheck` to `true` in [DataManager](Assets/PhishingMaster/scripts/DataManager.cs).  
This will always display the option to skip the tutorial.

## Creating a Build of the Game
Before creating a new build of the game, make sure to follow the steps in the **Configuration** section.  
After switching to the desired build target (PC or WebGL) in Unity, you may need to rebuild the Addressables. To do this, go to **Window -> Asset Management -> Addressables -> Groups**, then select **Build -> Clear Build Cache -> All**, and finally **Build -> New Build -> Default Build Script**.

### Standalone Version
Nothing special is required to create a standalone build.  
Simply go to **File -> Build Settings -> PC Standalone -> Build** in Unity.

### Web
For this documentation, we assume the build is placed in a folder named `htdocs`.  
In Unity, go to **File -> Build Settings -> WebGL -> Build**, then create/select a folder named `htdocs`.

#### Problems with Mouse Input in the Browser
To avoid problems with mouse position in the web version when the zoom is not at 100% or when the operating system is scaling, the following must be observed:  
In Unity’s **Player Settings** under **Publishing Settings**, set **Compression Format** to **Disabled**.  
After successfully creating the build, edit the file `Build/htdocs.wasm.framework.unityweb`.  
There should be two entries for the search term `devicePixelRatio=window.devicePixelRatio;`.  
At both locations, replace the value with `1`, as shown below:

```javascript
...var devicePixelRatio=window.devicePixelRatio;...
replace with:
...var devicePixelRatio=1;...
```

## Deployment
For information on how to provide the game to others and how to operate it, please refer to the [Deployment Documentation](Docs/Deployment/).

## Sources and License
##### Unity Assets Used from the Asset Store
The game is based on the official FPS Microgame template. The following free assets were also used:

- https://assetstore.unity.com/packages/2d/textures-materials/sky/skybox-series-free-103633  
- https://assetstore.unity.com/packages/essentials/asset-packs/unity-particle-pack-5-x-73777  
- https://assetstore.unity.com/packages/3d/environments/snaps-prototype-office-137490  
- https://assetstore.unity.com/packages/2d/textures-materials/milky-way-skybox-94001 (removed)

##### License
This project contains files from various sources, such as Unity assets or image files. The respective licenses for the individual files are listed in the corresponding `LICENSE.md` files in the relevant folders.

All files and modifications created by us are licensed under the MIT License.

```
Copyright (c) 2020 Tobias Länge and Philipp Matheis

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in 
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE.
``` 
