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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Functionality for the score board at the end of the game.
/// Loads data from high-score server and shows position of the current player.
/// </summary>
public class ScoreBoard : MonoBehaviour
{
    public GameObject[] cells;
    public GameObject progressBarCircle;
    private const float DEFAULT_TIME_TILL_COMPLETION = 2f;
    private int playerScore;
    private readonly int defaultPlayerCellIndex = 4;
    private readonly HighscoreController.HighscoreEntry emptyEntry = new HighscoreController.HighscoreEntry("", 0);
    private int playerPosition;
    private int challengerPosition;

    private static string playerName = "You";


    // Start is called before the first frame update
    void Start()
    {
        playerName = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "player_name_you").GetLocalizedString();

        // set cells to loading
        foreach (GameObject cell in cells)
        {
            HighscoreController.HighscoreEntry cellEntry = new HighscoreController.HighscoreEntry(LocalizationManager.Instance.GetStringTableEntry("strings_base", "loading_data").GetLocalizedString(), 0);
            SetCell(cell, cellEntry, 0);
        }
        // set player cell
        playerScore = DataManager.score;
        HighscoreController.HighscoreEntry playerEntry = new HighscoreController.HighscoreEntry(playerName, playerScore);
        SetCell(cells[defaultPlayerCellIndex], playerEntry, 0);

        // load Data
        if (!DataManager.challengeModeActive)
        {
            StartCoroutine(HighscoreController.GetScores(DataManager.GetLevels()[DataManager.lastLevelCompleted].uniqueName, new HighscoreController.RequestCallback(
            (responseComplete) => { CreateLeaderboard(responseComplete); }, (responseFailed) => { RequestFailedCallback(responseFailed); })));
        }
        else
        {
            StartCoroutine(HighscoreController.GetScores(DataManager.challengerChallengeCode.GetChallengeLevelName(), new HighscoreController.RequestCallback(
            (responseComplete) => { CreateLeaderboard(responseComplete); }, (responseFailed) => { RequestFailedCallback(responseFailed); })));
        }
        progressBarCircle.GetComponent<ProgressBarCircle>().Reset();
        progressBarCircle.SetActive(true);
        if (!DataManager.challengeModeActive)
        {
            progressBarCircle.GetComponent<ProgressBarCircle>().SetEstimatedTimeTillCompletion(DEFAULT_TIME_TILL_COMPLETION);
        }
        else
        {
            progressBarCircle.GetComponent<ProgressBarCircle>().SetEstimatedTimeTillCompletion(DEFAULT_TIME_TILL_COMPLETION * 1.5f);
        }
    }

    /// <summary>
    /// Create entries for the leader board based on the given high-score list in response.
    /// Sort the player in at the correct position.
    /// Show score board afterwards.
    /// </summary>
    /// <param name="response">High-score list from the server.</param>
    private void CreateLeaderboard(string response)
    {
        if (!DataManager.challengeModeActive)
        {
            progressBarCircle.GetComponent<ProgressBarCircle>().SetComplete();
        }
        HighscoreController.HighscoreEntry[] highscoreList = HighscoreController.ParseHighscoreListFromSelectScoreResponse(response).entries;
        //Fill with empty entries
        if (highscoreList.Length < 8)
        {
            highscoreList = AppendEntries(highscoreList, emptyEntry, 7 - highscoreList.Length);
        }

        //select player position
        playerPosition = highscoreList.Length; //set to last place
        for (int i = 0; i < highscoreList.Length; i++)
        {
            if (highscoreList[i].score <= playerScore)
            {
                playerPosition = i;
                break;
            }
        }
        HighscoreController.HighscoreEntry myPlayerEntry = new HighscoreController.HighscoreEntry(playerName, playerScore);
        highscoreList = InsertEntry(highscoreList, myPlayerEntry, playerPosition);

        if (!DataManager.challengeModeActive)
        {
            //No challenge, show scoreboard
            ShowScoreBoard(highscoreList);
        }
        else
        {
            //challenge active, load challenger data first
            StartCoroutine(HighscoreController.GetScores(DataManager.challengerChallengeCode.DatabaseEntryID, new HighscoreController.RequestCallback(
                (responseComplete) => { CreateChallengeLeaderboard(highscoreList, responseComplete); }, (responseFailed) => { ShowScoreBoard(highscoreList); })));
        }
    }

    /// <summary>
    /// Search for challenger position and show challenge score board.
    /// </summary>
    /// <param name="response">Challenger data from the server.</param>
    private void CreateChallengeLeaderboard(HighscoreController.HighscoreEntry[] highscoreList, string response)
    {
        progressBarCircle.GetComponent<ProgressBarCircle>().SetComplete();
        HighscoreController.HighscoreEntry challengerData = HighscoreController.ParseHighscoreListFromSelectScoreResponse(response).entries[0];

        //find challenger position
        challengerPosition = -1;
        for (int i = 0; i < highscoreList.Length; i++)
        {
            HighscoreController.HighscoreEntry entry = highscoreList[i];
            if (entry.name.Equals(challengerData.name)
                && entry.score.Equals(challengerData.score)
                && entry != highscoreList[playerPosition])
            {
                challengerPosition = i;
                break;
            }
        }
        if (challengerPosition == -1)
        {
            //challenger not found, should not happen unless entry was deleted
            ShowScoreBoard(highscoreList);
            return;
        }
        ShowChallengeScoreBoard(highscoreList);
    }

    /// <summary>
    /// Display the entries of the given high-score list.
    /// </summary>
    private void ShowScoreBoard(HighscoreController.HighscoreEntry[] highscoreList)
    {
        int positionOffset = 1; //Offset from array position to real position (0 => 1)
        if (playerPosition <= 7)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                SetCell(cells[i], highscoreList[i], i + positionOffset);
            }
            ColorPlayerCell(cells[playerPosition]);
        }
        else
        {
            //set first 3 places
            for (int i = 0; i < 3; i++)
            {
                SetCell(cells[i], highscoreList[i], i + positionOffset);
            }
            //set scores of playerPosition-1 and playerPosition to cells 3,4
            SetCell(cells[3], highscoreList[playerPosition - 1], playerPosition - 1 + positionOffset);
            SetCell(cells[4], highscoreList[playerPosition], playerPosition + positionOffset);
            ColorPlayerCell(cells[4]);

            //fill cell 5 with playerPosition + 1 or empty
            if (playerPosition + 1 < highscoreList.Length)
            {
                SetCell(cells[5], highscoreList[playerPosition + 1], playerPosition + 1 + positionOffset);
            }
            else
            {
                SetCell(cells[5], emptyEntry, playerPosition + 1 + positionOffset);
            }

            //fill cell 6 and 7
            if (highscoreList.Length - 2 >= playerPosition + 1)
            {
                SetCell(cells[6], highscoreList[highscoreList.Length - 2], highscoreList.Length - 2 + positionOffset);
                SetCell(cells[7], highscoreList[highscoreList.Length - 1], highscoreList.Length - 1 + positionOffset);
            }
            else
            {
                SetCell(cells[6], emptyEntry, highscoreList.Length + positionOffset);
                SetCell(cells[7], emptyEntry, highscoreList.Length + positionOffset);
            }
        }
    }

    /// <summary>
    /// Display the entries of the given high-score list.
    /// Highlight the challenger position.
    /// </summary>
    private void ShowChallengeScoreBoard(HighscoreController.HighscoreEntry[] highscoreList)
    {
        int positionOffset = 1; //Offset from array position to real position (0 => 1)
        if (playerPosition < 8 && challengerPosition < 8)
        {
            //both in top 8, just set the color
            ShowScoreBoard(highscoreList);
            ColorChallengerCell(cells[challengerPosition]);
        }
        else if ((playerPosition >= 8 || challengerPosition >= 8) && Math.Abs(challengerPosition - playerPosition) == 1)
        {
            //both NOT in top 8, but 1 place apart, just set the color
            ShowScoreBoard(highscoreList);
            ColorChallengerCell(cells[4 + challengerPosition - playerPosition]);
        }
        else
        {
            int player1Position = playerPosition;
            int player2Position = challengerPosition;
            GameObject player1Cell;
            GameObject player2Cell;
            if (challengerPosition < playerPosition)
            {
                player1Position = challengerPosition;
                player2Position = playerPosition;
            }

            //place player1 in one of cells 0-5
            if (player1Position < 6)
            {
                //place player1 in top 6
                for (int i = 0; i < 6; i++)
                {
                    SetCell(cells[i], highscoreList[i], i + positionOffset);
                }
                player1Cell = cells[player1Position];
                //place player2 in the top of the bottom 2 cells
                SetCell(cells[6], highscoreList[player2Position], player2Position + positionOffset);
                player2Cell = cells[6];
                if (player2Position + 1 < highscoreList.Length)
                {
                    SetCell(cells[7], highscoreList[player2Position + 1], player2Position + 1 + positionOffset);
                }
                else
                {
                    SetCell(cells[7], emptyEntry, player2Position + 1 + positionOffset);
                }
            }
            else
            {
                //set first 3 places
                for (int i = 0; i < 3; i++)
                {
                    SetCell(cells[i], highscoreList[i], i + positionOffset);
                }
                //set scores of player1Position-1 and player1Position to cells 3,4
                SetCell(cells[3], highscoreList[player1Position - 1], player1Position - 1 + positionOffset);
                SetCell(cells[4], highscoreList[player1Position], player1Position + positionOffset);
                player1Cell = cells[4];

                //fill cell 5 with player1Position + 1
                SetCell(cells[5], highscoreList[player1Position + 1], player1Position + 1 + positionOffset);
                //place player2 in the top of the bottom 2 cells
                SetCell(cells[6], highscoreList[player2Position], player2Position + positionOffset);
                player2Cell = cells[6];
                if (player2Position + 1 < highscoreList.Length)
                {
                    SetCell(cells[7], highscoreList[player2Position + 1], player2Position + 1 + positionOffset);
                }
                else
                {
                    SetCell(cells[7], emptyEntry, player2Position + 1 + positionOffset);
                }
            }

            if (player1Position == playerPosition)
            {
                ColorPlayerCell(player1Cell);
                ColorChallengerCell(player2Cell);
            }
            else
            {
                ColorPlayerCell(player2Cell);
                ColorChallengerCell(player1Cell);
            }
        }
    }

    private HighscoreController.HighscoreEntry[] InsertEntry(HighscoreController.HighscoreEntry[] list, HighscoreController.HighscoreEntry entry, int position)
    {
        HighscoreController.HighscoreEntry[] newList = new HighscoreController.HighscoreEntry[list.Length + 1];
        for (int i = 0; i < position; i++)
        {
            newList[i] = list[i];
        }
        newList[position] = entry;
        for (int i = position + 1; i < newList.Length; i++)
        {
            newList[i] = list[i - 1];
        }
        return newList;
    }

    private HighscoreController.HighscoreEntry[] AppendEntries(HighscoreController.HighscoreEntry[] list, HighscoreController.HighscoreEntry entry, int count)
    {
        int origLength = list.Length;
        Array.Resize(ref list, origLength + count);
        for (int i = origLength; i < list.Length; i++)
        {
            list[i] = entry;
        }
        return list;
    }

    private void SetCell(GameObject cell, HighscoreController.HighscoreEntry entry, int position)
    {
        cell.transform.Find("Position").gameObject.GetComponent<Text>().text = "" + position;
        cell.transform.Find("Name").gameObject.GetComponent<Text>().text = entry.name;
        cell.transform.Find("Score").gameObject.GetComponent<Text>().text = "" + entry.score;
    }

    public void RequestFailedCallback(string response)
    {
        progressBarCircle.GetComponent<ProgressBarCircle>().SetComplete();
        Debug.Log("No connection possible");

        foreach (GameObject cell in cells)
        {
            HighscoreController.HighscoreEntry cellEntry = new HighscoreController.HighscoreEntry(LocalizationManager.Instance.GetStringTableEntry("strings_base", "no_connection_possible").GetLocalizedString(), 0);
            SetCell(cell, cellEntry, 0);
        }
        // set player cell
        playerScore = DataManager.score;
        HighscoreController.HighscoreEntry playerEntry = new HighscoreController.HighscoreEntry(playerName, playerScore);
        SetCell(cells[defaultPlayerCellIndex], playerEntry, 0);
        ColorPlayerCell(cells[defaultPlayerCellIndex]);
    }

    private void ColorPlayerCell(GameObject cell)
    {
        ColorCell(cell, DataManager.kitGreen);
    }

    private void ColorChallengerCell(GameObject cell)
    {
        ColorCell(cell, DataManager.kitOrange);
    }

    private void ColorCell(GameObject cell, Color color)
    {
        cell.transform.Find("Background").GetComponent<Image>().color = color;
    }
}
