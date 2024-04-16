/*
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
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Linq; //Needed for XDocument

/// <summary>
/// Manages multiple windows in endscene.
/// Handles buttons, texts and effects on scoreMenu.
/// Loads data of phishing mails for resultsWindow.
/// </summary>
public class EndScreen : MonoBehaviour
{
    public GameObject[] mailPlaceHolders;
    public Sprite iconCorrect;
    public Sprite iconWrong;
    public Sprite iconTime;
    public Sprite phishIcon;
    public Sprite noPhishIcon;
    public GameObject[] smallScoreBoxes;
    public GameObject scoreBox;
    public GameObject highScoreBox;
    public GameObject endTutorialText;
    public GameObject startGameButton;
    public Color32 colorCorrect;
    public Color32 colorWrong;
    public Color32 colorExpired;
    public string noPhishText;
    public GameObject nextButtonScoreScreen;
    public GameObject resultsWindow;
    private MailDetail[] mailDetails;
    private int score;

    // Effects
    public GameObject biggerExplosionVFX;
    public GameObject explosionVFX;
    public GameObject[] explosionSpots;
    private float delayTimer;
    private int currentExplosionIndex;
    private bool playEffects = false;
    public GameObject scoreWindow;

    // Mail object representing the shown image and description on detail view
    public class MailDetail
    {
        public Sprite sprite;
        public string description;
        public Sprite iconSprite;
        public Color32 iconColor;
        public bool isPhish;

        public MailDetail(Sprite sprite, string description, Sprite iconSprite, Color32 iconColor, bool isPhish)
        {
            this.sprite = sprite;
            this.description = description;
            this.iconSprite = iconSprite;
            this.iconColor = iconColor;
            this.isPhish = isPhish;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Update lastLevelCompleted to currentLevel and update currentLevel
        DataManager.lastLevelCompleted = DataManager.currentLevel;
        DataManager.currentLevel = Mathf.Min(DataManager.currentLevel + 1, DataManager.GetLevels().Length - 1);

        // If tutorial mode, check if it was the first time and save to playerprefs
        if (DataManager.GetLevels()[DataManager.lastLevelCompleted].uniqueName == "training_level")
        {
            if (!PlayerPrefs.HasKey("Tutorial_Complete"))
            {
                PlayerPrefs.SetInt("Tutorial_Complete", 1);
            }
        }

        // Use smaller explosion on low graphics
        if (QualitySettings.names[QualitySettings.GetQualityLevel()].Equals("Low"))
        {
            biggerExplosionVFX = explosionVFX;
        }

        // Update score text
        score = DataManager.score;
        string localizedScore = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "final_score_text").GetLocalizedString(score);
        string localizedLevelComplete = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "level_complete").GetLocalizedString();
        string scoreText = DataManager.GetLevels()[DataManager.lastLevelCompleted].showPoints ? localizedScore : localizedLevelComplete;

        // Dont show points and skip high score screen after tutorial
        if (!DataManager.GetLevels()[DataManager.lastLevelCompleted].showPoints)
        {
            nextButtonScoreScreen.GetComponent<NextButtonNoExit>().nextMenu = resultsWindow;
        }

        scoreBox.GetComponent<TextMeshProUGUI>().text = scoreText;
        // Set score for each small scorebox
        foreach (GameObject scoreBox in smallScoreBoxes)
        {
            scoreBox.GetComponent<TextMeshProUGUI>().text = scoreText;
        }

        if (DataManager.GetLevels()[DataManager.lastLevelCompleted].showPoints)
        {
            int highscore = DataManager.DEFAULT_HIGH_SCORE;
            // Check for highscore
            if (PlayerPrefs.HasKey("HighScore"))
            {
                highscore = PlayerPrefs.GetInt("HighScore");
            }

            if (score > highscore)
            {
                // Set new highscore
                PlayerPrefs.SetInt("HighScore", score);
                highScoreBox.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "new_personal_record_text").GetLocalizedString(score);

                // Start timer for special effects
                delayTimer = 0.5f;
                currentExplosionIndex = 0;
                playEffects = true;
            }
            else
            {
                highScoreBox.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "personal_record_text").GetLocalizedString(highscore);
            }
        }
        else
        {
            // Show no text if points are disabled
            highScoreBox.SetActive(false);
            endTutorialText.SetActive(true);
        }

        // Update Play button and text for next level
        if (DataManager.currentLevel == DataManager.lastLevelCompleted)
        {
            startGameButton.transform.Find("SPIELEN").gameObject.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "play_again").GetLocalizedString(); ;
        }
        startGameButton.transform.Find("LevelName").gameObject.GetComponent<TextMeshProUGUI>().text = DataManager.GetLevels()[DataManager.currentLevel].name;


        // Replace mail placeholders in results window with the real mails
        if (DataManager.ShotMails != null)
        {
            mailDetails = new MailDetail[DataManager.ShotMails.Count];
            int i = 0;
            foreach (DataManager.ShotMail mail in DataManager.ShotMails)
            {
                if (i < mailPlaceHolders.Length)
                {
                    Sprite sprite = LoadImage(mail.fileName, mail.isPhish);
                    mailPlaceHolders[i].GetComponent<Image>().sprite = sprite;
                    string text = LoadDesciptionText(mail.fileName, mail.isPhish);
                    Color32 iconColor;

                    Sprite iconToUse;
                    if (mail.correct)
                    {
                        iconToUse = iconCorrect;
                        iconColor = colorCorrect;
                    }
                    else if (mail.expired)
                    {
                        iconToUse = iconTime;
                        iconColor = colorExpired;
                    }
                    else
                    {
                        iconToUse = iconWrong;
                        iconColor = colorWrong;
                    }
                    mailPlaceHolders[i].transform.Find("Icon/Image").gameObject.GetComponent<Image>().sprite = iconToUse;
                    mailPlaceHolders[i].transform.Find("Icon/Image").gameObject.GetComponent<Image>().color = iconColor;
                    // Create MailDetail object
                    mailDetails[i] = new MailDetail(sprite, text, iconToUse, iconColor, mail.isPhish);
                    i++;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Play effects with a delay
        if (scoreWindow.activeSelf && playEffects)
        {
            delayTimer -= Time.deltaTime;
            if ((currentExplosionIndex < explosionSpots.Length) && (delayTimer < 0))
            {
                var vfx = Instantiate(biggerExplosionVFX, explosionSpots[currentExplosionIndex].transform.position, biggerExplosionVFX.transform.rotation);
                Destroy(vfx, 3.8f);
                currentExplosionIndex++;
                delayTimer = 0.5f;
            }
        }
    }

    // Load image of phishing mail for results window
    private Sprite LoadImage(string name, bool isPhish)
    {
        string texturePath = DataManager.TEXTURE_PATH_PREFIX + DataManager.GetLevels()[DataManager.lastLevelCompleted].resourcePath;
        Texture2D spriteTexture;
        if (isPhish)
        {
            spriteTexture = Resources.Load<Texture2D>(texturePath + "Phish/" + name);
            // Create sprite out of texture
            return Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), 100, 0, SpriteMeshType.Tight);
        }
        else
        {
            spriteTexture = Resources.Load<Texture2D>(texturePath + "NoPhish/" + name);
            // Create sprite out of texture
            return Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), 100, 0, SpriteMeshType.Tight);
        }
    }

    // Load description of phishing mail from xml for results window
    private string LoadDesciptionText(string name, bool isPhish)
    {
        if (isPhish)
        {
            TextAsset textAsset = (TextAsset)Resources.Load(DataManager.XML_PATH_PREFIX + DataManager.GetLevels()[DataManager.lastLevelCompleted].resourcePath + name);
            XDocument xmlDoc = XDocument.Parse(textAsset.text);

            IEnumerable<XElement> items = xmlDoc.Descendants("mail").Elements();
            string text = "";

            foreach (var item in items)
            {
                if (item.Name == "description")
                {
                    text = item.Value;
                }

            }
            return text.Trim();
        }
        else
        {
            return noPhishText;
        }
    }

    public MailDetail GetMailDetail(int number)
    {
        return mailDetails[number];
    }

    public int GetScore()
    {
        return this.score;
    }
}
