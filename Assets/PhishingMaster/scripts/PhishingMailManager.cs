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
using UnityEngine.Events;
using System.IO;
using TMPro;
using System.Xml; //Needed for XML functionality
using System.Xml.Serialization; //Needed for XML Functionality
using System.Xml.Linq; //Needed for XDocument
using UnityEngine.EventSystems;
using UnityEngine.Localization.Tables;

/// <summary>
/// This class contains the main game logic.
/// Handles creation and destruction of phishing mails.
/// </summary>
public class PhishingMailManager : MonoBehaviour
{
    public GameObject phishingMail;
    public Vector3 startPosition;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI mailsCompleteText;

    PlayerCharacterController m_PlayerController;
    Objective m_ObjectiveKillEnemies;

    public List<PhishingMailController> enemies { get; private set; }
    public int numberOfEnemiesTotal { get; private set; }
    public int numberOfEnemiesRemaining => enemies.Count;
    public int points = 0;

    public UnityAction<PhishingMailController, int> onRemoveEnemy;

    public GameObject inGameMenu;// required to disable/enable in game menu

    private int phishingMailsFinished = 0;
    private int phishingMailsSpawned = 0;
    private int numberOfPhishingMails;
    private List<Mail> mails;
    private List<DataManager.ShotMail> shotMails = new List<DataManager.ShotMail>();

    public static float SPAWN_TIMER_DEFAULT = 3f;
    private static float SPAWN_TIMER_DISABLED = -30000000f;
    private float nextSpawnTimer;
    private PhishingMailController currentPMC;

    private TutorialManager tutorialManager;

    private const float MAIL_TIMER = 10;

    //--Points--
    public static readonly int basePointsCorrect = 100;
    public static readonly int maxTimeBonus = 50;
    public static readonly int perfectBonusPoints = 50;
    public static readonly float multiplierPerComboCount = 0.15f;
    private int comboCounter = 0;
    //----------

    /// <summary>
    /// Represents a mail with texture and area for perfect hit.
    /// </summary>
    private class Mail
    {
        public Object texture;
        public bool isPhish;
        public Vector4 phishArea;

        public Mail(Object texture, bool isPhish, Vector4 phishArea)
        {
            this.texture = texture;
            this.isPhish = isPhish;
            this.phishArea = phishArea;
        }
        // If no phish, area not needed
        public Mail(Object texture, bool isPhish)
        {
            this.texture = texture;
            this.isPhish = isPhish;
            this.phishArea = new Vector4(0, 0, 0, 0);
        }
    }

    private void Awake()
    {
        m_PlayerController = FindObjectOfType<PlayerCharacterController>();
        DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, PhishingMailManager>(m_PlayerController, this);

        m_ObjectiveKillEnemies = FindObjectOfType<Objective>();
        DebugUtility.HandleErrorIfNullFindObject<Objective, PhishingMailManager>(m_ObjectiveKillEnemies, this);

        enemies = new List<PhishingMailController>();

        DataManager.Level level = DataManager.GetLevels()[DataManager.currentLevel];

        if (!DataManager.challengeModeActive)
        {
            // select random seed if we are not in challenge mode
            DataManager.seed = Random.Range(int.MinValue, int.MaxValue);
        }
        else
        {
            DataManager.seed = DataManager.challengerChallengeCode.Seed;
        }
        if (DataManager.GetLevels()[DataManager.currentLevel].uniqueName.Equals("training_level"))
        {
            DataManager.seed = 80;
        }
        mails = LoadMailsFromResources(level.numberOfMailsToShow, DataManager.seed);
        numberOfPhishingMails = mails.Count;
    }

    /// <summary>
    /// Load numberOfMails mails if possible (50% phish and 50% nonPhish).
    /// Mails are selected and sorted randomly based on seed specified.
    /// Returns less mails if not enough available.
    /// </summary>
    /// <param name="numberOfMails">The number of mails to load.</param>
    /// <param name="seed">The seed used for random selection and sorting.</param>
    /// <returns></returns>
    private List<Mail> LoadMailsFromResources(int numberOfMails, int seed)
    {
        var comparer = Comparer<Object>.Create((k1, k2) => k1.name.CompareTo(k2.name));
        List<Object> pmt = new List<Object>(Resources.LoadAll(DataManager.TEXTURE_PATH_PREFIX + DataManager.GetLevels()[DataManager.currentLevel].resourcePath + "Phish", typeof(Texture)));
        pmt.Sort(comparer);
        List<Object> npmt = new List<Object>(Resources.LoadAll(DataManager.TEXTURE_PATH_PREFIX + DataManager.GetLevels()[DataManager.currentLevel].resourcePath + "NoPhish", typeof(Texture)));
        npmt.Sort(comparer);
        //remember state of random generator
        var oldRandomState = Random.state;
        //initialize random generator with our seed
        Random.InitState(seed);

        pmt = ShuffleList(pmt);
        npmt = ShuffleList(npmt);

        int rnopm = numberOfMails / 2; //requested number of phishing mails
        int rnonpm = rnopm; //requested number of phishing mails

        if (pmt.Count > rnopm)
        {
            pmt.RemoveRange(rnopm, pmt.Count - rnopm);
        }
        if (npmt.Count > rnonpm)
        {
            npmt.RemoveRange(rnonpm, npmt.Count - rnonpm);
        }

        List<Mail> result = new List<Mail>();

        foreach (Object o in pmt)
        {
            // get xml for phish
            Vector4 area = GetPhishArea(o.name);
            result.Add(new Mail(o, true, area));
        }

        foreach (Object o in npmt)
        {
            result.Add(new Mail(o, false));
        }

        result = ShuffleList(result);
        //reset random generator to old state
        Random.state = oldRandomState;
        return result;
    }

    void Start()
    {
        //var enemy1 = Instantiate(phishingMail, startPosition, Quaternion.identity);
        nextSpawnTimer = SPAWN_TIMER_DEFAULT;

        // show objective at the beginning
        // m_ObjectiveKillEnemies.UpdateObjective("Erkenne die Phishing Mails", points + " Punkte", "");
        UpdateScoreText();
        UpdateMailsCompleteText();

        tutorialManager = GetComponent<TutorialManager>();
    }

    void Update()
    {
        nextSpawnTimer -= Time.deltaTime;

        if (nextSpawnTimer > SPAWN_TIMER_DISABLED && nextSpawnTimer < 0 && phishingMailsSpawned < numberOfPhishingMails)
        {
            CreateNextPhishingMail();
            nextSpawnTimer = SPAWN_TIMER_DISABLED;
            tutorialManager.ActionComplete(TutorialManager.TutorialEvent.MAIL_SPAWNED);
        }

        if (phishingMailsSpawned == phishingMailsFinished && phishingMailsSpawned < numberOfPhishingMails) //waiting to spawn new Mail
        {
            // Update Timer
            timerText.text = Mathf.RoundToInt(nextSpawnTimer).ToString() + " s";
        }
    }

    /// <summary>
    /// Update remaining lifetime of the current mail.
    /// Called by PhishingMailController to tell the current remaining lifetime.
    /// </summary>
    /// <param name="remainingLifeTime"></param>
    public void UpdateRemainingLifeTime(float remainingLifeTime)
    {
        timerText.text = Mathf.RoundToInt(remainingLifeTime).ToString() + " s";
        // call event for 10 seconds remaining
        if (remainingLifeTime <= MAIL_TIMER)
        {
            tutorialManager.ActionComplete(TutorialManager.TutorialEvent.MAIL_TIMER);
        }
    }

    private List<E> ShuffleList<E>(List<E> inputList)
    {
        List<E> randomList = new List<E>();

        int randomIndex = 0;
        while (inputList.Count > 0)
        {
            randomIndex = Random.Range(0, inputList.Count); //Choose a random object in the list
            randomList.Add(inputList[randomIndex]); //add it to the new, random list
            inputList.RemoveAt(randomIndex); //remove to avoid duplicates
        }
        return randomList; //return the new random list
    }

    private void CreateNextPhishingMail()
    {
        GameObject new_enemy = Instantiate(phishingMail, startPosition, Quaternion.identity);
        PhishingMailController pmc = new_enemy.GetComponent<PhishingMailController>();
        currentPMC = pmc;

        Mail m = mails[phishingMailsSpawned];
        pmc.SetPhishingMailTexture((Texture2D)m.texture, m.isPhish, m.phishArea, m.texture.name);

        phishingMailsSpawned++;
    }

    /// <summary>
    /// Read hit zones from XML file that belongs to the mail with the given name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private Vector4 GetPhishArea(string name)
    {
        Vector4 area = new Vector4(0, 0, 1, 1);
        try
        {
            TextAsset textAsset = (TextAsset)Resources.Load(DataManager.XML_PATH_PREFIX + DataManager.GetLevels()[DataManager.currentLevel].resourcePath + name);
            if (textAsset == null)
            { // file not found
                return area;
            }

            XDocument xmlDoc = XDocument.Parse(textAsset.text);

            IEnumerable<XElement> items = xmlDoc.Descendants("mail").Elements();
            float x1 = 0;
            float x2 = 0;
            float y1 = 0;
            float y2 = 0;

            foreach (var item in items)
            {
                if (item.Name == "zone")
                {
                    try
                    {
                        x1 = float.Parse(item.Element("x1").Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                        y1 = float.Parse(item.Element("y1").Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                        x2 = float.Parse(item.Element("x2").Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                        y2 = float.Parse(item.Element("y2").Value.Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (System.FormatException e)
                    {
                        Debug.Log("Format exception: " + e);
                    }
                }
            }
            area.Set(x1, y1, x2, y2);
        }
        catch (FileNotFoundException)
        {
            Debug.Log("No xml file available, using default values");
        }
        return area;
    }

    public void RegisterEnemy(PhishingMailController enemy)
    {
        enemies.Add(enemy);
        //enemy.isPhish = (Random.value > 0.5f);
        numberOfEnemiesTotal++;
    }

    /// <summary>
    /// Handles the destruction of a mail.
    /// Calculate points for player, creates message and updates texts depending on result.
    /// </summary>
    /// <param name="enemyKilled">Controller of destroyed mail</param>
    /// <param name="correct">If player decision was right or wrong</param>
    /// <param name="bonus">If the player should get bonus points</param>
    /// <param name="time">The remaining time</param>
    public void UnregisterEnemy(PhishingMailController enemyKilled, bool correct, bool bonus, float time)
    {
        int enemiesRemainingNotification = numberOfEnemiesRemaining - 1;

        int pointsAdded = 0;

        if (correct)
        {
            pointsAdded = basePointsCorrect + (int)(maxTimeBonus * (1 - Mathf.Min(time, PhishingMailController.LIFE_TIME_DEFAULT) / PhishingMailController.LIFE_TIME_DEFAULT)); //Base points and time bonus
            pointsAdded = (int)(pointsAdded * (1 + comboCounter * multiplierPerComboCount)); //combo multiplier
            pointsAdded += bonus ? perfectBonusPoints : 0; // perfect bonus
            shotMails.Add(new DataManager.ShotMail(enemyKilled.fileName, enemyKilled.isPhish, true, false));
            comboCounter++;
        }
        else
        {
            comboCounter = 0;
        }
        points += pointsAdded;
        phishingMailsFinished++;
        if (DataManager.GetLevels()[DataManager.currentLevel].showPoints)
        {
            UpdateComboText();
        }

        tutorialManager.ActionComplete(TutorialManager.TutorialEvent.MAIL_DESTROYED);
        var notificationText = "";
        if (pointsAdded == 0)
        {
            if (time > PhishingMailController.LIFE_TIME_DEFAULT)
            {
                // mail expired
                notificationText = "Zeit abgelaufen";
                shotMails.Add(new DataManager.ShotMail(enemyKilled.fileName, enemyKilled.isPhish, false, true));
            }
            else
            {
                // wrong
                notificationText = "Falsch";
                shotMails.Add(new DataManager.ShotMail(enemyKilled.fileName, enemyKilled.isPhish, false, false));
            }

        }
        else
        {
            notificationText = DataManager.GetLevels()[DataManager.currentLevel].showPoints ? "+" + pointsAdded + " Punkte" : "Richtig";
        }
        string descriptionText = "Erkenne die Phishing-Nachrichten";
        string pointsText = DataManager.GetLevels()[DataManager.currentLevel].showPoints ? points + " Punkte" : "";
        if (phishingMailsFinished == numberOfPhishingMails)
        {
            tutorialManager.ActionComplete(TutorialManager.TutorialEvent.LEVEL_COMPLETE);
            m_ObjectiveKillEnemies.CompleteObjective(descriptionText, pointsText, notificationText);
            // add score to DataManager to access it in endscreen
            DataManager.score = points;
            //PlayerPrefs.SetInt("Score", points);
            DataManager.ShotMails = shotMails;

        }
        else
        {
            m_ObjectiveKillEnemies.UpdateObjective(descriptionText, pointsText, notificationText);
        }

        // removes the enemy from the list, so that we can keep track of how many are left on the map
        enemies.Remove(enemyKilled);
        UpdateScoreText();
        UpdateMailsCompleteText();

        nextSpawnTimer = SPAWN_TIMER_DEFAULT;
    }

    private void UpdateScoreText()
    {
        scoreText.text = LocalizationManager.Instance.GetStringTableEntry("strings_game_scene", "score_text").GetLocalizedString(points);
    }

    private void UpdateComboText()
    {
        comboText.text = LocalizationManager.Instance.GetStringTableEntry("strings_game_scene", "combo_text").GetLocalizedString(comboCounter);
    }

    private void UpdateMailsCompleteText()
    {
        mailsCompleteText.text = LocalizationManager.Instance.GetStringTableEntry("strings_game_scene", "mails_complete_text").GetLocalizedString(phishingMailsFinished, numberOfPhishingMails);
    }

    public void UnregisterEnemy(PhishingMailController enemyKilled, bool correct, float time)
    {
        UnregisterEnemy(enemyKilled, correct, false, time);
    }

    public void SetWeaponActive(bool active)
    {
        m_PlayerController.SetWeaponActive(active);
    }

    public PhishingMailController GetCurrentPMC()
    {
        return currentPMC;
    }

    /// <summary>
    /// Shows cursor and freezes game for interactive tutorial pop up.
    /// </summary>
    public void StopTime()
    {
        // disable ingame menu to avoid cursor locking again
        inGameMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Hides cursor and continues game.
    /// </summary>
    public void ContinueTime()
    {
        // enable ingame menu again
        inGameMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
    }

    public void ContinueButtonPressed()
    {
        tutorialManager.ActionComplete(TutorialManager.TutorialEvent.PLAYER_CONTINUE);
    }
}
