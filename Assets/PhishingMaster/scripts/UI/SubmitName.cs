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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.UIWidgets.service;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Manages player name input.
/// Uploads score to Server.
/// Generates Challenge code.
/// </summary>
public class SubmitName : MonoBehaviour
{
    [HideInInspector]
    public string userName;
    public GameObject errorPopUp;
    public GameObject continuePopUp;
    public GameObject submitPopUp;
    public GameObject nextButton;
    public GameObject form;
    public InputField textInput;
    public GameObject descritptionText;
    public GameObject submitButton;
    public GameObject endScreenManager;
    public GameObject progressBarCircle;
    public GameObject challengeCodeDisplay;
    public GameObject toolTip;
    public GameObject challengeNameForm;

    private bool isScoreSubmited;
    private bool hasScoreSubmitFailed;
    private bool submitAnonymous = false;
    private NextButtonNoExit nextButtonScript;
    private int score;
    private string submitPopUpText;
    private const string DEFAULT_USER_NAME = "Anonym";
    private const string DEFAULT_CHALLENGE_NAME = "Herausforderung";
    private string myChallengeCode = null;
    private string challengeName;

    private const float DEFAULT_TIME_TILL_COMPLETION = 2f;

    // Start is called before the first frame update
    void Start()
    {
        this.isScoreSubmited = false;
        this.hasScoreSubmitFailed = false;
        nextButtonScript = nextButton.GetComponent<NextButtonNoExit>();

        score = DataManager.score;

        if (DataManager.challengeModeActive)
        {
            challengeName = DataManager.challengerChallengeCode.ChallengeName;
        }
        else
        {
            challengeName = DEFAULT_CHALLENGE_NAME;
        }

        submitPopUpText = submitPopUp.GetComponentInChildren<Text>().text;

        // load last used name if available
        if (PlayerPrefs.HasKey("Player_Name"))
        {
            this.userName = PlayerPrefs.GetString("Player_Name");
            textInput.SetTextWithoutNotify(this.userName);
        }
        else
        {
            this.userName = DEFAULT_USER_NAME;
        }

    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// Called when user finished editing the name input field.
    /// </summary>
    /// <param name="newText">Text in input field</param>
    public void OnTextEdit(string newText)
    {
        if (newText == "")
        {
            // use default username
            this.userName = DEFAULT_USER_NAME;
        }
        else
        {
            this.userName = newText.Trim();
        }
    }

    public void OnTextChanged(string newText)
    {
        // don't hide tooltip
        //toolTip.SetActive(false);
    }

    /// <summary>
    /// Called when user finished editing the challenge name input field.
    /// </summary>
    /// <param name="newText">Text in input field</param>
    public void OnChallengeNameFormTextEdit(string newText)
    {
        if (newText == "")
        {
            // use default username
            this.challengeName = DEFAULT_CHALLENGE_NAME;
        }
        else
        {
            this.challengeName = newText.Trim();
        }
    }

    public void OnChallengeNameFormTextChanged(string newText)
    {
        // don't hide tooltip
        //challengeNameForm.transform.Find("Tooltip").gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when user clicks challenge name submit button.
    /// </summary>
    public void OnChallengeNameFormSubmit()
    {
        SubmitAndGenerateChallengeCode();
        challengeNameForm.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Called when user clicks name submit button.
    /// </summary>
    public void OnSubmitName()
    {
        // check for input
        if (this.userName == "")
        {
            OpenPopUp(errorPopUp);
        }
        else
        {
            toolTip.SetActive(false);
            this.userName = WordFilter.filterInput(this.userName);
            OpenPopUp(submitPopUp);
            submitPopUp.GetComponentInChildren<Text>().text = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "public_leaderboard_submit_popup_text").GetLocalizedString(this.userName, this.score);
        }

    }

    public void CheckForSubmit()
    {
        // check if player submited name or tried and it failed, then call next window script
        if (this.isScoreSubmited || this.hasScoreSubmitFailed)
        {
            nextButtonScript.NextWindow();
        }
        // if not submited yet, check if a name was typed in the textfield and submit name
        else if (this.userName != DEFAULT_USER_NAME)
        {
            OnSubmitName();
        }
        else
        {
            OpenPopUp(continuePopUp);
        }
    }

    /// <summary>
    /// Called when user accepts to submit name to server.
    /// </summary>
    public void OnAllowSubmit()
    {
        // remove popup and show other elements again
        submitPopUp.SetActive(false);
        // save name in playerprefs
        if (this.userName.Equals(DEFAULT_USER_NAME))
        {
            submitAnonymous = true;
        }
        else
        {
            PlayerPrefs.SetString("Player_Name", this.userName);
        }

        SubmitScore(this.userName, this.score);
        descritptionText.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "uploading_score").GetLocalizedString();
        progressBarCircle.GetComponent<ProgressBarCircle>().Reset();
        progressBarCircle.SetActive(true);
        progressBarCircle.GetComponent<ProgressBarCircle>().SetEstimatedTimeTillCompletion(DEFAULT_TIME_TILL_COMPLETION);
    }

    public void SubmitAnonymousAndContinue()
    {
        this.userName = DEFAULT_USER_NAME;
        this.submitAnonymous = true;
        // silent upload
        SubmitScore(this.userName, this.score);
        // continue
        nextButtonScript.NextWindow();
    }

    /// <summary>
    /// Submit score to leader board for the level with the given level name.
    /// </summary>
    /// <param name="userName">Player name.</param>
    /// <param name="score">Player score.</param>
    private void SubmitScore(string userName, int score)
    {
        // add score to level leader board if not in challenge, else add to challenge specific level
        if (!DataManager.challengeModeActive)
        {
            //Submit score to level leader board
            endScreenManager.GetComponent<EndScreen>().StartCoroutine(HighscoreController.AddScore(userName,
                score,
                DataManager.GetLevels()[DataManager.lastLevelCompleted].uniqueName,
                new HighscoreController.RequestCallback(
                    (responseComplete) => { SubmitScoreComplete(responseComplete); },
                    (responseFailed) => { SubmitScoreFailed(responseFailed); })));
        }
        else
        {
            //Create a challengeCode with invalid id
            ChallengeManager.ChallengeCode partialChallengeCode = new ChallengeManager.ChallengeCode(
                DataManager.GetLevels()[DataManager.lastLevelCompleted].uniqueName,
                DataManager.challengerChallengeCode.ChallengeName,
                DataManager.challengerChallengeCode.Seed,
                -1);
            //Submit score to seed specific leader board
            endScreenManager.GetComponent<EndScreen>().StartCoroutine(HighscoreController.AddScore(userName,
                score,
                partialChallengeCode.GetChallengeLevelName(),
                partialChallengeCode.ChallengeName,
                new HighscoreController.RequestCallback(
                    (responseComplete) => { SubmitScoreComplete(responseComplete); },
                    (responseFailed) => { SubmitScoreFailed(responseFailed); })));
        }
#if !(UNITY_WEBGL && !UNITY_EDITOR)
        //let player in standalone version copy the code from textfield and deactivate copy button
        challengeCodeDisplay.transform.Find("TextField").GetComponent<InputField>().interactable = true;
        challengeCodeDisplay.transform.Find("CopyChallengeCodeButton").gameObject.SetActive(false);
#endif
    }

    private void OpenPopUp(GameObject popUp)
    {
        // deactivate all other buttons/clickable elements
        //formScript.deactivateAll();
        form.SetActive(false);
        nextButton.SetActive(false);
        //nextButton.GetComponent<Button>().interactable = false;
        //show pop up
        popUp.SetActive(true);
        // set selected element to null and let navigation of pop up handle
        EventSystem.current.SetSelectedGameObject(null);

    }

    /// <summary>
    /// Submit score to seed specific level and generate challenge code.
    /// </summary>
    private void SubmitAndGenerateChallengeCode()
    {
        //Create a challengeCode with invalid id
        ChallengeManager.ChallengeCode partialChallengeCode = new ChallengeManager.ChallengeCode(DataManager.GetLevels()[DataManager.lastLevelCompleted].uniqueName, challengeName, DataManager.seed, -1);
        endScreenManager.GetComponent<EndScreen>().StartCoroutine(HighscoreController.AddScore(this.userName,
            this.score,
            partialChallengeCode.GetChallengeLevelName(),
            challengeName,
            new HighscoreController.RequestCallback(
                (responseComplete) => { SubmitAndGenerateChallengeCodeComplete(responseComplete); },
                (responseFailed) => { SubmitAndGenerateChallengeCodeFailed(responseFailed); })));
#if !UNITY_STANDALONE || UNITY_EDITOR //Don't show challenge code in standalone version, but in editor
        challengeCodeDisplay.SetActive(true);
        //show progress
        Transform progressBar = challengeCodeDisplay.transform.Find("ProgressBarCircle");
        progressBar.GetComponent<ProgressBarCircle>().Reset();
        progressBar.gameObject.SetActive(true);
        progressBar.GetComponent<ProgressBarCircle>().SetEstimatedTimeTillCompletion(DEFAULT_TIME_TILL_COMPLETION);
#endif
    }

    /// <summary>
    /// Called when ChallengeCode was successfully submitted.
    /// </summary>
    /// <param name="response">Response from server.</param>
    private void SubmitAndGenerateChallengeCodeComplete(string response)
    {
#if !UNITY_STANDALONE || UNITY_EDITOR //Don't show challenge code in standalone version, but in editor
        int id = HighscoreController.ParseIDFromAddScoreResponse(response);
        DataManager.myChallengeCode = new ChallengeManager.ChallengeCode(DataManager.GetLevels()[DataManager.lastLevelCompleted].uniqueName, challengeName, DataManager.seed, id);
        Debug.Log(DataManager.myChallengeCode.GetChallengeCode());
        if (challengeCodeDisplay.activeSelf)
        {
            challengeCodeDisplay.transform.Find("ProgressBarCircle").GetComponent<ProgressBarCircle>().SetComplete();
        }
        else
        {
            challengeCodeDisplay.SetActive(true);
        }
        challengeCodeDisplay.transform.Find("TextField").GetComponent<InputField>().SetTextWithoutNotify(DataManager.myChallengeCode.GetChallengeCode());
        myChallengeCode = DataManager.myChallengeCode.GetChallengeCode();
#endif
    }

    /// <summary>
    /// Called when ChallengeCode creation failed or connection was not possible.
    /// </summary>
    /// <param name="response">Response from server.</param>
    private void SubmitAndGenerateChallengeCodeFailed(string response)
    {
#if !UNITY_STANDALONE || UNITY_EDITOR //Don't show challenge code in standalone version, but in editor
        //TODO do we need a retry button?
        challengeCodeDisplay.transform.Find("ProgressBarCircle").GetComponent<ProgressBarCircle>().SetComplete();
        challengeCodeDisplay.transform.Find("TextField").GetComponent<InputField>().SetTextWithoutNotify(LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "not_available").GetLocalizedString());
#endif
    }

    /// <summary>
    /// Called when share button was clicked.
    /// Shows challenge code in web browser.
    /// </summary>
    public void OnShareChallengeCode()
    {
        if (myChallengeCode != null)
        {
            ShowChallengeCode(myChallengeCode, this.score);
        }
    }
    private void ShowChallengeCode(string s, int score)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        //Copy the code to clipboard via javascript
        ChallengeManager.ShowChallengeCodeInBrowser(s, score);
#endif
    }

    /// <summary>
    /// Called when score was successfully submitted.
    /// </summary>
    /// <param name="response">Response from server.</param>
    private void SubmitScoreComplete(string response)
    {
        progressBarCircle.GetComponent<ProgressBarCircle>().SetComplete();
        descritptionText.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "entry_leaderboard_successful").GetLocalizedString();

        ChallengeManager.OnPlayerNameSubmitted(userName, score);

        //If we are already in challenge mode, we don't need to submit a new score
        if (DataManager.challengeModeActive)
        {
            SubmitAndGenerateChallengeCodeComplete(response);
        }
        else
        {
            if (!submitAnonymous)
            {
                //generate challenge code only if name was submitted
#if !UNITY_STANDALONE || UNITY_EDITOR //Don't show challenge name form in standalone version, but in editor
                challengeNameForm.SetActive(true);
#endif
            }
        }

        form.SetActive(true);
        nextButton.SetActive(true);
        this.isScoreSubmited = true;
        textInput.GetComponent<InputField>().interactable = false;
        submitButton.GetComponent<Button>().interactable = false;

        // select continue button
        EventSystem.current.SetSelectedGameObject(nextButton);
    }

    /// <summary>
    /// Called when score submission of score failed.
    /// </summary>
    /// <param name="response">Response from server.</param>
    private void SubmitScoreFailed(string response)
    {
        progressBarCircle.GetComponent<ProgressBarCircle>().SetComplete();
        descritptionText.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "entry_leaderboard_failed").GetLocalizedString();
        descritptionText.GetComponent<TextMeshProUGUI>().color = DataManager.kitOrange;
        form.SetActive(true);
        nextButton.SetActive(true);
        hasScoreSubmitFailed = true;
    }
}