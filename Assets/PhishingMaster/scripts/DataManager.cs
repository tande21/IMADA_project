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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The DataManager is used to store information that needs to be accessible by multiple other elements of the game.
/// It also initializes the levels based on the current platform or a given challenge code.
/// </summary>
public static class DataManager
{
    private static List<ShotMail> shotMails;

    public static readonly Color kitGreen = new Color(0f / 255, 150f / 255, 130f / 255, 255f / 255);
    public static readonly Color kitOrange = new Color(223f / 255, 155f / 255, 24f / 255, 255f / 255);
    public static readonly Color kitRed = new Color(162f / 255, 34f / 255, 35f / 255, 255f / 255);

    /// <summary>
    /// Used to authenticate messages against the high-score server and to check authenticity of challenge-codes
    /// </summary>
    public static readonly string HIGHSCORE_SECRET_KEY = "mySecretKey";
    /// <summary>
    /// Addresses for high-score server.
    /// Addresses are tried in the given order. 
    /// </summary>
    public static readonly string[] HIGHSCORE_SERVER_ADDRESSES = { "", "http://phishing-master-highscore-server.local/", "http://localhost/" };

    public static readonly string TEXTURE_PATH_PREFIX = "Images/";
    public static readonly string XML_PATH_PREFIX = "XML/";

    public static bool disableTutorialCompleteCheck = false;

    public static int DEFAULT_HIGH_SCORE = 0;

    public static int score = 0;

    public static ChallengeManager.ChallengeCode myChallengeCode = null;
    public static ChallengeManager.ChallengeCode challengerChallengeCode = null;
    public static bool challengeModeActive = false;
    /// <summary>
    /// Seed used for random mail selection and order.
    /// Used by PhishingMailManager during initialization to select the mails/textures from the current level folder.
    /// </summary>
    public static int seed;

    /// <summary>
    /// Current level index.
    /// Determines the level started next, increased by 1 when EndScreen is loaded.
    /// Increased up to levels.Length.
    /// </summary>
    public static int currentLevel = 0;
    /// <summary>
    /// Index of last level completed, set by EndScreen before currentLevel is incremented.
    /// </summary>
    public static int lastLevelCompleted = 0;
    private static Level[] availableLevels = {
        new Level("training_level", "Training", "Training/", 10, false),
        new Level("real_company_level_1", "Punktemodus", "Level 1/", 10, true), //Level with real company names
        new Level("fake_company_level_1", "Punktemodus", "Level 2/", 10, true)}; //Level with fake company names used for web_version

    /// <summary>
    /// Class representing a game level and all necessary informations about it.
    /// </summary>
    public class Level
    {
        /// <summary>
        /// Unique level name used internally and for high score server.
        /// May only contain the following characters: [a-z][A-Z][0-9]_ (letters, numbers and underscore).
        /// </summary>
        public readonly string uniqueName;
        /// <summary>
        /// Name shown to the user.
        /// </summary>
        private string _name;
        public string name
        {
            get
            {
                if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsTablesReady() && uniqueName != null)
                {
                    return LocalizationManager.Instance.GetStringTableEntry("strings_base", uniqueName + "_display_name").GetLocalizedString();
                }
                return _name;
            }
            private set { _name = value; }
        }
        /// <summary>
        /// Location of the texture/image- and XML- files.
        /// </summary>
        public readonly string resourcePath;
        /// <summary>
        /// Number of mails, that should be shown per playthrough.
        /// </summary>
        public readonly int numberOfMailsToShow;
        /// <summary>
        /// Show points to the user during this level.
        /// Also show scoreboard and let user enter name for high score.
        /// </summary>
        public readonly bool showPoints;

        public Level(string uniqueName, string name, string resourcePath, int numberOfMailsToShow, bool showPoints)
        {
            this.uniqueName = uniqueName;
            this.name = name;
            this.resourcePath = resourcePath;
            this.numberOfMailsToShow = numberOfMailsToShow;
            this.showPoints = showPoints;
        }
    }

    /// <summary>
    /// Represents a mail, that has been completed.
    /// Used to transfer the information from the PhishingMailManager to EndScreen.
    /// </summary>
    public class ShotMail
    {
        public string fileName;
        public bool isPhish;
        public Vector4 phishArea;
        public bool correct;
        public bool expired;

        public ShotMail(string fileName, bool isPhish, bool correct, bool expired)
        {
            this.fileName = fileName;
            this.isPhish = isPhish;
            this.phishArea = new Vector4(0, 0, 0, 0);
            this.correct = correct;
            this.expired = expired;
        }
    }


    public static List<ShotMail> ShotMails
    {
        get
        {
            return shotMails;
        }
        set
        {
            shotMails = value;
        }
    }

    /// <summary>
    /// Sets challenge mode to active, the challengerChallengeCode and the levels variable to [trainingLevel, challengeLevel].
    /// Make sure the level exists before calling this function (e.g. with GetAvailableLevel()).
    /// </summary>
    /// <param name="challengerCode">The challenge code of the challenger.</param>
    public static void ActivateChallengeMode(ChallengeManager.ChallengeCode challengerCode)
    {
        challengeModeActive = true;
        challengerChallengeCode = challengerCode;

        //select challenge level
        levels = new Level[] { availableLevels[0], GetAvailableLevel(challengerCode.UniqueLevelName) };
        Debug.Log("Selected Challenge Level: " + challengerCode.UniqueLevelName);
    }

    /// <summary>
    /// Disable challenge mode and set challengerChallengeCode to null.
    /// Also reinitializes levels based on current platform.
    /// </summary>
    public static void DeactivateChallengeMode()
    {
        challengeModeActive = false;
        challengerChallengeCode = null;

        //reinitialize levels
        levels = null;
        GetLevels();
    }

    /// <summary>
    /// Returns the level with the specified uniqueLevelName or null.
    /// </summary>
    /// <param name="uniqueLevelName">The unique level name to search for.</param>
    /// <returns>The level with the specified uniqueLevelName or null</returns>
    public static Level GetAvailableLevel(string uniqueLevelName)
    {
        foreach (Level l in availableLevels)
        {
            if (l.uniqueName.Equals(uniqueLevelName))
            {
                return l;
            }
        }
        return null;
    }

    private static Level[] levels = null;

    /// <summary>
    /// Get the current levels based on platform and challenge mode.
    /// </summary>
    /// <returns>The current levels.</returns>
    public static Level[] GetLevels()
    {
        if (levels == null)
        {
            InitializeLevels();
        }
        return levels;
    }

    /// <summary>
    /// Initializes levels based on current platform.
    /// </summary>
    private static void InitializeLevels()
    {
        if (System.Environment.GetCommandLineArgs().Contains("--web-mails"))
        {
            //if we get the parameter --web-mails, select training and the web level
            levels = new Level[] { availableLevels[0], availableLevels[2] };
        }
        else
        {
#if UNITY_WEBGL
            //Select training and web level on web-gl version
            levels = new Level[] { availableLevels[0], availableLevels[2] };
#else
            //By default select training and level 1 on all other versions
            levels = new Level[] { availableLevels[0], availableLevels[1] };
#endif
        }

        string output = "Initialized Levels: \n";
        foreach (Level l in levels)
        {
            output += l.uniqueName + "\n";
        }
        Debug.Log(output);
    }
}