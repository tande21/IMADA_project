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
using TMPro;
using System.Collections;

/// <summary>
/// This class contains the procedure of the interactive tutorial
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public GameObject leftPopup;
    public GameObject rightPopup;
    public GameObject bottomPopup;
    public GameObject weaponDisabledHint;

    private bool timePaused = false;
    private int state;
    private PhishingMailManager phishingMailManager;
    private const string HIGHLIGHT_COLOR = "#009682ff";
    /// <summary>
    /// Contains all events that can trigger a state change of the interactive tutorial
    /// </summary>
    public enum TutorialEvent
    {
        MAIL_SPAWNED,
        MAIL_TIMER,
        MAIL_DESTROYED,
        PLAYER_CONTINUE,
        LEVEL_COMPLETE,
        LEVEL_START
    }
    // Start is called before the first frame update
    void Start()
    {
        state = 0;
        phishingMailManager = GetComponent<PhishingMailManager>();
        // Trigger the first event at the beginning of the tutorial
        ActionComplete(TutorialEvent.LEVEL_START);
    }

    // Update is called once per frame
    void Update()
    {
        if (timePaused)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Debug.Log(Cursor.lockState);
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Checks whether the given event is the expected event at the current state. If the event fits the state gets updated.
    /// </summary>
    /// <param name="tutorialEvent">The event that is triggered</param>
    public void ActionComplete(TutorialEvent tutorialEvent)
    {
        // Only do something when the current level is training/tutorial level, return immediatly otherwise
        if (!(DataManager.GetLevels()[DataManager.currentLevel].uniqueName == "training_level"))
        {
            return;
        }
        switch (state)
        {
            case 0:
                if (tutorialEvent == TutorialEvent.LEVEL_START)
                {
                    SetTimeActive(false);
                    SetBottomPopup(true, LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "tutorial_text_1").GetLocalizedString());
                    state++;
                }
                break;
            case 1:
                if (tutorialEvent == TutorialEvent.PLAYER_CONTINUE)
                {
                    SetTimeActive(true);
                    SetWeaponActive(false);
                    SetBottomPopup(false, "");
                    state++;
                }
                break;
            case 2:
                if (tutorialEvent == TutorialEvent.MAIL_SPAWNED)
                {
                    SetBottomPopup(false, "");
                    SetRightPopup(true, LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "tutorial_text_2").GetLocalizedString());
                    state++;
                }
                break;
            case 3:
                if (tutorialEvent == TutorialEvent.MAIL_TIMER)
                {
                    phishingMailManager.GetCurrentPMC().StopMail();
                    phishingMailManager.GetCurrentPMC().SetNoPhishHittable(false);
                    SetRightPopup(false, "");
                    SetLeftPopup(true, LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "tutorial_text_3").GetLocalizedString());
                    SetWeaponActive(true);

                    state++;
                }
                break;
            case 4:
                if (tutorialEvent == TutorialEvent.MAIL_DESTROYED)
                {
                    SetLeftPopup(false, "");
                    SetRightPopup(true, LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "tutorial_text_4").GetLocalizedString());
                    SetWeaponActive(false);

                    state++;
                }
                break;
            case 5:
                if (tutorialEvent == TutorialEvent.MAIL_TIMER)
                {
                    phishingMailManager.GetCurrentPMC().StopMail();
                    SetRightPopup(false, "");
                    SetLeftPopup(true, LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "tutorial_text_5").GetLocalizedString());
                    SetWeaponActive(true);
                    phishingMailManager.GetCurrentPMC().SetNoPhishHittable(true);
                    phishingMailManager.GetCurrentPMC().SetMailHittable(false);

                    state++;
                }
                break;
            case 6:
                if (tutorialEvent == TutorialEvent.MAIL_DESTROYED)
                {
                    SetLeftPopup(false, "");
                    SetRightPopup(true, LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "tutorial_text_6").GetLocalizedString());
                    SetWeaponActive(false);

                    state++;
                }
                break;
            case 7:
                if (tutorialEvent == TutorialEvent.MAIL_TIMER)
                {
                    phishingMailManager.GetCurrentPMC().StopMail();
                    phishingMailManager.GetCurrentPMC().SetMailHittable(true);
                    phishingMailManager.GetCurrentPMC().SetNoPhishHittable(false);
                    SetRightPopup(false, "");
                    SetLeftPopup(true, LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "tutorial_text_7").GetLocalizedString());
                    SetWeaponActive(true);

                    state++;
                }
                break;
            case 8:
                if (tutorialEvent == TutorialEvent.MAIL_DESTROYED)
                {
                    SetLeftPopup(false, "");
                    SetRightPopup(true, LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "tutorial_text_8").GetLocalizedString());
                    SetWeaponActive(false);
                    state++;

                }
                break;
            case 9:
                if (tutorialEvent == TutorialEvent.MAIL_TIMER)
                {
                    phishingMailManager.GetCurrentPMC().StopMail();
                    phishingMailManager.GetCurrentPMC().SetMailHittable(true);
                    phishingMailManager.GetCurrentPMC().SetNoPhishHittable(false);
                    SetRightPopup(false, "");
                    SetLeftPopup(true, LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "tutorial_text_9").GetLocalizedString());
                    SetWeaponActive(true);

                    state++;
                }
                break;
            case 10:
                if (tutorialEvent == TutorialEvent.MAIL_DESTROYED)
                {
                    SetWeaponActive(false);
                    SetLeftPopup(false, "");
                    SetRightPopup(false, "");

                    state++;
                }
                break;
            case 11:
                if (tutorialEvent == TutorialEvent.MAIL_SPAWNED)
                {
                    SetBottomPopup(true, LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "tutorial_text_10").GetLocalizedString());
                    SetTimeActive(false);

                    state++;
                }
                break;
            case 12:
                if (tutorialEvent == TutorialEvent.PLAYER_CONTINUE)
                {
                    SetTimeActive(true);
                    phishingMailManager.GetCurrentPMC().ContinueMail();
                    SetWeaponActive(true);
                    SetBottomPopup(false, "");

                    state++;
                }
                break;
        }
    }

    private void SetTimeActive(bool active)
    {
        if (active)
        {
            phishingMailManager.ContinueTime();
            timePaused = false;
        }
        else
        {
            phishingMailManager.StopTime();
            timePaused = true;
        }
    }

    private void SetWeaponActive(bool active)
    {
        phishingMailManager.SetWeaponActive(active);
        weaponDisabledHint.SetActive(!active);
    }

    public void SetRightPopup(bool active, string text)
    {
        rightPopup.SetActive(active);
        rightPopup.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    public void SetLeftPopup(bool active, string text)
    {
        leftPopup.SetActive(active);
        leftPopup.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    public void SetBottomPopup(bool active, string text)
    {
        bottomPopup.SetActive(active);
        bottomPopup.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    private string Colorize(string text)
    {
        return "<color=" + HIGHLIGHT_COLOR + ">" + text + "</color>";
    }

}
