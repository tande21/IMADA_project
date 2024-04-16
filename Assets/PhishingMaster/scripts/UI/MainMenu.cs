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

using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using static ChallengeManager;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Contains main menu logic.
/// Handles reading of challenge code and level initialization.
/// </summary>
public class MainMenu : MonoBehaviour
{
    public GameObject rulesWindow;
    public GameObject creditsWindow;
    public GameObject controlsWindow;
    public GameObject skipTutorialPopUp;
    public GameObject[] disappearOnOpen;
    public GameObject startButton;
    public TextMeshProUGUI gameVersionText;
    public GameObject challengePopup;
    public GameObject challengePopupProgressBar;
    public GameObject challengeText;

    private const float DEFAULT_TIME_TILL_COMPLETION = 2f;
    private int devCodePosition = 0; //Position of the dev-code sequence
    private ChallengeCode challengerCode = null;


    IEnumerator Start()
    {
        DataManager.currentLevel = 0;

        // Set language
        if (!LocalizationManager.Instance.IsUserLanguageSet())
        {
            string languageCode = GetBrowserLanguage();
            LocalizationManager.Instance.SetUserLanguage(languageCode);
        }

        // Wait until localization files are loaded
        while (!LocalizationManager.Instance.IsReady())
        {
            Debug.Log("Waiting for localization files to initialize...");
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("Localization files initialized.");

        gameVersionText.text = LocalizationManager.Instance.GetStringTableEntry("strings_base", "version").GetLocalizedString(Application.version);
        SetDevMode(false);

        // Get challenge code from browser
        string challengeCode = GetChallengeCodeInput();
        if (challengeCode != null && challengeCode != "none")
        {
            //Show pop up if challenge code is available
            ShowChallengePopup(challengeCode);
        }
    }

    /// <summary>
    /// Shows a pop up informing the user about the challenge mode.
    /// Retrieves information about the challenge from the server.
    /// </summary>
    /// <param name="challengeCode">The challenge code.</param>
    private void ShowChallengePopup(string challengeCode)
    {
        OpenPopUp(challengePopup);
        Text challengerText = challengePopup.transform.Find("ChallengerText").GetComponent<Text>();

        if (ChallengeCode.IsValidChallengeCode(challengeCode))
        {
            string loadingData = LocalizationManager.Instance.GetStringTableEntry("strings_base", "loading_data").GetLocalizedString();
            challengerText.text = LocalizationManager.Instance.GetStringTableEntry("strings_challenge", "challenge_mode_challenger").GetLocalizedString(loadingData, loadingData);
            //Start Coroutine to load data
            StartCoroutine(HighscoreController.GetScores(ChallengeCode.getDatabaseEntryIDFromChallengeCode(challengeCode), new HighscoreController.RequestCallback(
                (responseComplete) => { ShowChallengerData(challengeCode, responseComplete, true); }, (responseFailed) => { ShowChallengerData(challengeCode, responseFailed, false); })));
            challengePopupProgressBar.GetComponent<ProgressBarCircle>().Reset();
            challengePopupProgressBar.SetActive(true);
            challengePopupProgressBar.GetComponent<ProgressBarCircle>().SetEstimatedTimeTillCompletion(DEFAULT_TIME_TILL_COMPLETION);
        }
        else
        {
            challengerText.text = LocalizationManager.Instance.GetStringTableEntry("strings_challenge", "invalid_challenge_code").GetLocalizedString();
        }
    }

    /// <summary>
    /// Called when the challenge code data was retrieved from the server.
    /// Shows challenger data to the user.
    /// </summary>
    /// <param name="challengeCode">The challenge code entered by the user.</param>
    /// <param name="response">The response from the server.</param>
    /// <param name="successful">True if the server responded without error.</param>
    private void ShowChallengerData(string challengeCode, string response, bool successful)
    {
        challengePopupProgressBar.GetComponent<ProgressBarCircle>().SetComplete();
        Text challengerText = challengePopup.transform.Find("ChallengerText").GetComponent<Text>();
        if (!successful)
        {
            string dataNotFound = LocalizationManager.Instance.GetStringTableEntry("strings_base", "data_not_found").GetLocalizedString();
            challengerText.text = LocalizationManager.Instance.GetStringTableEntry("strings_challenge", "challenge_mode_challenger").GetLocalizedString(dataNotFound, dataNotFound);
        }
        else
        {
            HighscoreController.DatabaseEntry challengerData = HighscoreController.ParseDatabaseEntryListFromSelectScoreResponse(response).entries[0];
            challengerCode = new ChallengeCode(challengerData.level, challengerData.challengename, ChallengeCode.getDatabaseEntryIDFromChallengeCode(challengeCode));
            if (DataManager.GetAvailableLevel(challengerCode.UniqueLevelName) == null) // Level not found
            {
                challengerText.text = LocalizationManager.Instance.GetStringTableEntry("strings_challenge", "invalid_challenge_code").GetLocalizedString();
            }
            else
            {
                challengerText.text = LocalizationManager.Instance.GetStringTableEntry("strings_challenge", "challenge_mode_challenger").GetLocalizedString(challengerData.name, challengerData.score);
                challengePopup.transform.Find("AcceptButton").GetComponent<Button>().interactable = true;
            }
        }
    }

    /// <summary>
    /// Called when the player accepts the challenge.
    /// Initializes challenge and sets corresponding level.
    /// </summary>
    public void OnChallengeModeAcceptButtonClicked()
    {
        //close pop up
        challengePopup.GetComponent<NextButtonNoExit>().CloseWindow();

        DataManager.ActivateChallengeMode(challengerCode);


        // show challengeMode Text in right corner
        challengeText.SetActive(true);
        challengeText.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetStringTableEntry("strings_challenge", "challenge_mode_title").GetLocalizedString() + "\n" + DataManager.challengerChallengeCode.ChallengeName;
        // change Menu texts
        startButton.transform.Find("Challenge").gameObject.SetActive(true);
        startButton.transform.Find("Normal").gameObject.SetActive(false);

    }

    /// <summary>
    /// Called when the player declines the challenge.
    /// Deactivates challenge and sets default level.
    /// </summary>
    public void OnChallengeModeDeclineButtonClicked()
    {
        //close popup
        challengePopup.GetComponent<NextButtonNoExit>().CloseWindow();

        DataManager.DeactivateChallengeMode();

        // hide challengeMode Text in right corner
        challengeText.SetActive(false);
        // change Menu back
        startButton.transform.Find("Challenge").gameObject.SetActive(false);
        startButton.transform.Find("Normal").gameObject.SetActive(true);
    }

    private void OpenPopUp(GameObject popUp)
    {
        // deactivate all other buttons/clickable elements
        for (int i = 0; i < disappearOnOpen.Length; i++)
        {
            disappearOnOpen[i].SetActive(false);
        }
        //show pop up
        popUp.SetActive(true);
        // set selected element to null and let navigation of pop up handle
        EventSystem.current.SetSelectedGameObject(null);
    }

    void Update()
    {
        //Code for dev-mode, can be enabled by entering the コナミコマンド in the main menu
        if (Input.GetAxisRaw(GameConstants.k_AxisNameMenuVertical) != 0 ||
                            Input.GetAxisRaw(GameConstants.k_AxisNameMenuHorizontal) != 0 ||
                            Input.GetButtonDown(GameConstants.k_ButtonNameCancel) ||
                            Input.GetButtonDown(GameConstants.k_ButtonNameSubmit))
        {
            switch (devCodePosition)
            {
                case 0:
                    UpdateDevCode(true, Input.GetAxisRaw(GameConstants.k_AxisNameMenuVertical) > 0);//up
                    if (Input.GetButtonDown(GameConstants.k_ButtonNameCancel))
                    {
                        SetDevMode(false);
                    }
                    break;
                case 1:
                    UpdateDevCode(Input.GetAxisRaw(GameConstants.k_AxisNameMenuVertical) > 0, Input.GetAxisRaw(GameConstants.k_AxisNameMenuVertical) > 0);//up
                    break;
                case 2:
                    UpdateDevCode(Input.GetAxisRaw(GameConstants.k_AxisNameMenuVertical) > 0, Input.GetAxisRaw(GameConstants.k_AxisNameMenuVertical) < 0);//down
                    break;
                case 3:
                    UpdateDevCode(Input.GetAxisRaw(GameConstants.k_AxisNameMenuVertical) < 0, Input.GetAxisRaw(GameConstants.k_AxisNameMenuVertical) < 0);//down
                    break;
                case 4:
                    UpdateDevCode(Input.GetAxisRaw(GameConstants.k_AxisNameMenuVertical) < 0, Input.GetAxisRaw(GameConstants.k_AxisNameMenuHorizontal) < 0);//left
                    break;
                case 5:
                    UpdateDevCode(Input.GetAxisRaw(GameConstants.k_AxisNameMenuHorizontal) < 0, Input.GetAxisRaw(GameConstants.k_AxisNameMenuHorizontal) > 0);//right
                    break;
                case 6:
                    UpdateDevCode(Input.GetAxisRaw(GameConstants.k_AxisNameMenuHorizontal) > 0, Input.GetAxisRaw(GameConstants.k_AxisNameMenuHorizontal) < 0);//left
                    break;
                case 7:
                    UpdateDevCode(Input.GetAxisRaw(GameConstants.k_AxisNameMenuHorizontal) < 0, Input.GetAxisRaw(GameConstants.k_AxisNameMenuHorizontal) > 0);//right
                    break;
                case 8:
                    UpdateDevCode(Input.GetAxisRaw(GameConstants.k_AxisNameMenuHorizontal) > 0, Input.GetButtonDown(GameConstants.k_ButtonNameSubmit));//confirm
                    break;
                case 9:
                    UpdateDevCode(Input.GetButtonDown(GameConstants.k_ButtonNameSubmit), Input.GetButtonDown(GameConstants.k_ButtonNameCancel));//cancel
                    if (devCodePosition == 10)
                    {
                        devCodePosition = 0;
                        SetDevMode(true);
                    }
                    break;
            }
        }
    }

    private void SetDevMode(bool active)
    {
        if (active)
        {
            Debug.Log("Dev-Mode active");
            PhishingMailManager.SPAWN_TIMER_DEFAULT = 0.2f;
            gameVersionText.text = "DEV-MODE\n" + LocalizationManager.Instance.GetStringTableEntry("strings_base", "version").GetLocalizedString(Application.version);
            gameVersionText.color = Color.red;
            gameVersionText.gameObject.transform.localScale = new Vector3(20, 20);
        }
        else if (gameVersionText.gameObject.transform.localScale.Equals(new Vector3(20, 20)))
        {
            PhishingMailManager.SPAWN_TIMER_DEFAULT = 3f;
            gameVersionText.text = LocalizationManager.Instance.GetStringTableEntry("strings_base", "version").GetLocalizedString(Application.version);
            gameVersionText.color = Color.white;
            gameVersionText.gameObject.transform.localScale = new Vector3(1, 1);
        }
    }

    private void UpdateDevCode(bool currentCondition, bool nextCondition)
    {
        if (nextCondition)
        {
            devCodePosition++;
        }
        else if (!currentCondition)
        {
            devCodePosition = 0;
        }
    }

    public void OpenRules()
    {
        rulesWindow.SetActive(true);

        // avoid selecting other menu buttons
        for (int i = 0; i < disappearOnOpen.Length; i++)
        {
            disappearOnOpen[i].SetActive(false);
        }

        EventSystem.current.SetSelectedGameObject(null);

    }

    public void OpenCredits()
    {
        creditsWindow.SetActive(true);

        // avoid selecting other menu buttons
        for (int i = 0; i < disappearOnOpen.Length; i++)
        {
            disappearOnOpen[i].SetActive(false);
        }

        EventSystem.current.SetSelectedGameObject(null);

    }

    public void OpenControls()
    {
        controlsWindow.SetActive(true);

        // avoid selecting other menu buttons
        for (int i = 0; i < disappearOnOpen.Length; i++)
        {
            disappearOnOpen[i].SetActive(false);
        }

        EventSystem.current.SetSelectedGameObject(null);

    }

    public void OpenSkipTutorialPopUp()
    {
        // don't show skip tutorial pop up when first time play
        if (PlayerPrefs.HasKey("Tutorial_Complete") || DataManager.disableTutorialCompleteCheck)
        {
            skipTutorialPopUp.SetActive(true);
            // avoid selecting other menu buttons
            for (int i = 0; i < disappearOnOpen.Length; i++)
            {
                disappearOnOpen[i].SetActive(false);
            }

            // auto-select next button
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            SceneManager.LoadScene("TrainingScene");
        }
    }

    public void SkipTutorial(string sceneName)
    {
        DataManager.currentLevel = Mathf.Min(DataManager.currentLevel + 1, DataManager.GetLevels().Length - 1);
        SceneManager.LoadScene(sceneName);
    }
}