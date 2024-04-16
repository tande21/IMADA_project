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
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Class to handle communication with high-score server.
/// Also includes helper methods to parse responses from high-score server.
/// </summary>
public class HighscoreController : MonoBehaviour
{
    private static readonly string addScoreURL = "php/add-score.php";
    private static readonly string selectScoreURL = "php/select-score.php";
    private static readonly SHA256 sha256 = SHA256.Create();

    /// <summary>
    /// Add a score entry on the high-score server.
    /// Call this function with StartCoroutine().
    /// </summary>
    /// <param name="name">Player name.</param>
    /// <param name="score">The score.</param>
    /// <param name="level">The uniqueLevelName of the level,</param>
    /// <param name="callback">Callback for successful/failed request.</param>
    /// <returns></returns>
    public static IEnumerator AddScore(string name, int score, string level, IRequestCallback callback)
    {
        yield return AddScore(name, score, level, "", callback);
    }

    /// <summary>
    /// Add a score entry on the high-score server.
    /// Call this function with StartCoroutine().
    /// </summary>
    /// <param name="name">Player name.</param>
    /// <param name="score">The score.</param>
    /// <param name="level">The uniqueLevelName of the level.</param>
    /// <param name="challengeName">The name for the challenge.</param>
    /// <param name="callback">Callback for successful/failed request.</param>
    /// <returns></returns>
    public static IEnumerator AddScore(string name, int score, string level, string challengeName, IRequestCallback callback)
    {
        //Compute hash for authentication
        string hash = ComputeSHA256(name + score.ToString() + level + challengeName + DataManager.HIGHSCORE_SECRET_KEY);

        //Construct request URL without server address
        string requestUrl = addScoreURL + "?"
            + "name=" + UnityWebRequest.EscapeURL(name) + "&"
            + "score=" + UnityWebRequest.EscapeURL(score.ToString())
            + "&" + "level=" + UnityWebRequest.EscapeURL(level.ToString())
            + "&" + "challengename=" + UnityWebRequest.EscapeURL(challengeName.ToString())
            + "&" + "hash=" + hash;

        //Send request
        yield return SendRequest(DataManager.HIGHSCORE_SERVER_ADDRESSES, requestUrl, callback);
    }

    /// <summary>
    /// Private method for recursive calls.
    /// Used to try all addresses until success.
    /// </summary>
    private static IEnumerator SendRequest(string[] addresses, string requestUrl, IRequestCallback callback)
    {
        Debug.Log("Requesting " + addresses[0] + requestUrl + " ...");
        //Send request
        UnityWebRequest www = UnityWebRequest.Get(addresses[0] + requestUrl);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log("Request for " + addresses[0] + requestUrl + " failed with " + www.error);
            if (addresses.Length == 1)
            { //this was the last address to try
                callback.RequestFailedCallback(www.error);
            }
            else
            { //try other addresses
                yield return SendRequest(SubArray<string>(addresses, 1, addresses.Length - 1), requestUrl, callback);
            }
        }
        else
        {
            Debug.Log("Request for " + addresses[0] + requestUrl + " successful");
            callback.RequestCompleteCallback(www.downloadHandler.text);
        }
    }

    /// <summary>
    /// Get scores from the server.
    /// Call this function with StartCoroutine().
    /// </summary>
    /// <param name="level">The uniqueLevelName.</param>
    /// <param name="callback">Callback for successful/failed request.</param>
    /// <returns></returns>
    public static IEnumerator GetScores(string level, IRequestCallback callback)
    {
        yield return GetScores(level, -1, callback);
    }

    /// <summary>
    /// Get databaseEntry for a given ID from the server.
    /// Call this function with StartCoroutine().
    /// </summary>
    /// <param name="databaseEntryID">Specific database entry id.</param>
    /// <param name="callback">Callback for successful/failed request.</param>
    /// <returns></returns>
    public static IEnumerator GetScores(int databaseEntryID, IRequestCallback callback)
    {
        yield return GetScores(null, databaseEntryID, callback);
    }

    /// <summary>
    /// Sends a request to the web-server to retrieve all names and scores for the given level.
    /// If level is null, databaseEntryID is used the request a specific entry.
    /// </summary>
    private static IEnumerator GetScores(string level, int databaseEntryID, IRequestCallback callback)
    {
        string requestUrl;
        if (level == null)
        {
            requestUrl = selectScoreURL + "?" + "id=" + UnityWebRequest.EscapeURL(databaseEntryID.ToString());
        }
        else
        {
            requestUrl = selectScoreURL + "?" + "level=" + UnityWebRequest.EscapeURL(level.ToString());
        }

        yield return SendRequest(DataManager.HIGHSCORE_SERVER_ADDRESSES, requestUrl, callback);
    }

    public interface IRequestCallback
    {
        void RequestCompleteCallback(string response);

        void RequestFailedCallback(string response);
    }

    public class RequestCallback : IRequestCallback
    {
        private readonly Action<string> handleComplete;
        private readonly Action<string> handleFailed;
        public RequestCallback(Action<string> handleComplete, Action<string> handleFailed)
        {
            this.handleComplete = handleComplete;
            this.handleFailed = handleFailed;
        }

        public void RequestCompleteCallback(string response)
        {
            handleComplete(response);
        }

        public void RequestFailedCallback(string response)
        {
            handleFailed(response);
        }
    }

    /// <summary>
    /// Compute SHA-256 hash.
    /// </summary>
    /// <param name="data">String to hash.</param>
    /// <returns>SHA-256 hash of the given string.</returns>
    public static string ComputeSHA256(string data)
    {
        byte[] raw_hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        string hash = string.Empty;
        foreach (byte myByte in raw_hash)
        {
            hash += myByte.ToString("x2");
        }
        return hash;
    }

    public static int ParseIDFromAddScoreResponse(string jsonResponse)
    {
        return JsonUtility.FromJson<AddScoreResponse>(jsonResponse).id;
    }

    public static HighscoreList ParseHighscoreListFromSelectScoreResponse(string jsonResponse)
    {
        //extend json response with a top level node
        string json = "{\"entries\":" + jsonResponse + "}";

        return JsonUtility.FromJson<HighscoreList>(json);
    }

    public static DatabaseEntryList ParseDatabaseEntryListFromSelectScoreResponse(string jsonResponse)
    {
        //extend json response with a top level node
        string json = "{\"entries\":" + jsonResponse + "}";

        return JsonUtility.FromJson<DatabaseEntryList>(json);
    }

    private static T[] SubArray<T>(T[] array, int offset, int length)
    {
        T[] result = new T[length];
        Array.Copy(array, offset, result, 0, length);
        return result;
    }

    [Serializable]
    private class AddScoreResponse
    {
        public int id;

        public AddScoreResponse(int id)
        {
            this.id = id;
        }
    }

    [Serializable]
    public class HighscoreEntry
    {
        public string name;
        public int score;

        public HighscoreEntry(string name, int score)
        {
            this.name = name;
            this.score = score;
        }
    }

    [Serializable]
    public class HighscoreList
    {
        public HighscoreEntry[] entries;
    }

    [Serializable]
    public class DatabaseEntry
    {
        public string name;
        public int score;
        public string level;
        public string challengename;

        public DatabaseEntry(string name, int score, string level, string challengename)
        {
            this.name = name;
            this.score = score;
            this.level = level;
            this.challengename = challengename;
        }
    }

    [Serializable]
    public class DatabaseEntryList
    {
        public DatabaseEntry[] entries;
    }

}
