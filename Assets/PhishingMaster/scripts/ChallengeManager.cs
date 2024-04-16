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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices; //needed for DllImport("__Internal")

/// <summary>
/// Provides functionality to work with challenge codes.
/// Provides methods to interact with web browser.
/// </summary>
public static class ChallengeManager
{
    private static readonly int HASH_STRING_LENGTH = 16; //length of the hash string used in challenge codes.

    // methods to talk with javascript code
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string GetChallengeCodeFromURL();

    [DllImport("__Internal")]
    private static extern string JSGetBrowserLanguage();

    [DllImport("__Internal")]
    private static extern void BrowserAlertString(string str);

    [DllImport("__Internal")]
    private static extern void JSDisplayChallengeCode(string str, int score);

    [DllImport("__Internal")]
    private static extern void JSPlayerNameSubmitted(string str, int score);
    
    [DllImport("__Internal")]
    private static extern string JSGetURL();
#endif


    /// <summary>
    /// Get the challenge code from web browser.
    /// </summary>
    /// <returns>the challenge code extracted from the URL or null/"none"</returns>
    public static string GetChallengeCodeInput()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return GetChallengeCodeFromURL();
#else
        return null;
#endif
    }

    /// <summary>
    /// Get current URL without parameters, e.g. https://example.com/games/phishing-master/.
    /// Works only in webgl build.
    /// </summary>
    /// <returns>The URL of the game without parameters</returns>
    public static string GetURLFromBrowser()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return JSGetURL();
#else
        return null;
#endif
    }

    /// <summary>
    /// Shows the challenge code in a pop-up on the web-page.
    /// </summary>
    /// <param name="s">The challenge code.</param>
    /// <param name="score">The score.</param>
    public static void ShowChallengeCodeInBrowser(string s, int score)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JSDisplayChallengeCode(s, score);
#endif
    }

    /// <summary>
    /// Notifies the web browser, that the name was submitted to the server.
    /// Web browser can then refresh the score board.
    /// </summary>
    /// <param name="name">The players name.</param>
    /// <param name="score">The score.</param>
    public static void OnPlayerNameSubmitted(string name, int score)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JSPlayerNameSubmitted(name, score);
#endif
    }

    /// <summary>
    /// Represents a challenge code with all necessary informations.
    /// </summary>
    public class ChallengeCode
    {
        private string uniqueLevelName;
        private string challengeName;
        private int databaseEntryID;
        private int seed;

        public ChallengeCode(string uniqueLevelName, string challengeName, int seed, int databaseEntryID)
        {
            this.uniqueLevelName = uniqueLevelName;
            this.challengeName = challengeName;
            this.seed = seed;
            this.databaseEntryID = databaseEntryID;
        }

        public ChallengeCode(string databaseLevelName, string challengeName, int databaseEntryID)
        {
            this.uniqueLevelName = databaseLevelName.Split('-')[0];
            this.challengeName = challengeName;
            this.seed = int.Parse(databaseLevelName.Split('-')[1], System.Globalization.NumberStyles.HexNumber);
            this.databaseEntryID = databaseEntryID;
        }

        /// <summary>
        /// Check if a challenge code is valid.
        /// </summary>
        /// <param name="challengeCode">The challenge code to check.</param>
        /// <returns>True if the code is valid.</returns>
        public static bool IsValidChallengeCode(string challengeCode)
        {
            if (challengeCode == null || challengeCode.Split('-').Length != 2)
            {
                return false;
            }
            string[] parts = challengeCode.Split('-');
            if (parts[0] == "" || parts[1] == "")
            {
                return false;
            }
            try
            {
                int.Parse(parts[0], System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                return false;
            }

            string myHash = HighscoreController.ComputeSHA256(
                int.Parse(parts[0], System.Globalization.NumberStyles.HexNumber).ToString()
                + DataManager.HIGHSCORE_SECRET_KEY)
                .Substring(0, HASH_STRING_LENGTH);
            if (!myHash.Equals(parts[1]))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Extracts the databaseEntryID from the given challenge code.
        /// Check validity of the challenge code before using this function.
        /// </summary>
        /// <param name="challengeCode">The challenge code.</param>
        /// <returns>The database entry ID.</returns>
        public static int getDatabaseEntryIDFromChallengeCode(string challengeCode)
        {
            return int.Parse(challengeCode.Split('-')[0], System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// Get the challenge code as string.
        /// </summary>
        /// <returns>The challenge code.</returns>
        public string GetChallengeCode()
        {
            return databaseEntryID.ToString("X") + "-"
                + HighscoreController.ComputeSHA256(databaseEntryID.ToString() + DataManager.HIGHSCORE_SECRET_KEY).Substring(0, HASH_STRING_LENGTH);
        }

        /// <summary>
        /// Get the challenge level name used by the database.
        /// Returns the unique level name and the seed combined in one string.
        /// This format is used to store the unique challenge level in the database.
        /// </summary>
        /// <returns>Challenge level name to be used with database.</returns>
        public string GetChallengeLevelName()
        {
            return UniqueLevelName + "-" + seed.ToString("X8");
        }
        public int DatabaseEntryID { get => databaseEntryID; }
        public int Seed { get => seed; }
        public string UniqueLevelName { get => uniqueLevelName; }
        public string ChallengeName { get => challengeName; }
    }

    /// <summary>
    /// Get the challenge code from web browser.
    /// </summary>
    /// <returns>the challenge code extracted from the URL or null/"none"</returns>
    public static string GetBrowserLanguage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return JSGetBrowserLanguage();
#else
        return null;
#endif
    }
}
