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
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

/// <summary>
/// Manages results window where each mail is shown in multiple pages.
/// Creates detail view and replaces place holders.
/// </summary>
public class ResultsWindow : MonoBehaviour
{
    public GameObject detailWindow;
    public GameObject resultsWindow;
    public GameObject endScreenManager;
    public GameObject buttons;
    public GameObject[] pages;
    public TextMeshProUGUI pageCounter;
    public GameObject nextPageButton;
    public GameObject infoPopup;
    public GameObject nextButton;
    public CustomMenuNavigation firstPageMenuNavigation;

    private int currentPage;
    private readonly int MAILS_ON_PAGE = 6;

    private int selectedNumberOfMail;

    void Start()
    {
        selectedNumberOfMail = 0;
        currentPage = 0;
        pageCounter.text = "1/" + pages.Length;

        if (DataManager.ShotMails != null)
        {
            if (DataManager.ShotMails.Count <= 6)
            {
                pageCounter.text = "";
                nextPageButton.SetActive(false);
            }
        }

        // Show info text on first time the results are shown
        if (!PlayerPrefs.HasKey("resultsWindowInfo"))
        {
            ShowInfoPopup();
            PlayerPrefs.SetInt("resultsWindowInfo", 1);
        }
    }

    void Update()
    {

    }

    /// <summary>
    /// Creates and opens details view of mail with given index.
    /// </summary>
    /// <param name="numberOfMail">Index of clicked mail</param>
    public void OpenDetails(int numberOfMail)
    {
        detailWindow.SetActive(true);
        resultsWindow.SetActive(false);
        endScreenManager.GetComponent<CustomMenuScreen>().SetHideControls(false);
        buttons.SetActive(false);
        // Set currentpage from amount of mails
        if (numberOfMail >= MAILS_ON_PAGE)
        {
            currentPage = 1;
        }
        else
        {
            currentPage = 0;
        }
        pages[currentPage].SetActive(false);

        // Set mailNumber of script from detailWindow so current selection can be remembered
        selectedNumberOfMail = numberOfMail;
        // Set information of detailWindow script on which page the selected mail is located
        detailWindow.GetComponent<ResultDetails>().resultsWindow = resultsWindow;

        // Get details about selected Mail and set placeholders
        EndScreen.MailDetail mail = endScreenManager.GetComponent<EndScreen>().GetMailDetail(numberOfMail);
        detailWindow.transform.Find("Window/MailImage").gameObject.GetComponent<Image>().sprite = mail.sprite;
        detailWindow.transform.Find("Window/DescriptionText").gameObject.GetComponent<TextMeshProUGUI>().text = mail.description;
        detailWindow.transform.Find("Window/Icon/Image").gameObject.GetComponent<Image>().sprite = mail.iconSprite;
        detailWindow.transform.Find("Window/Icon/Image").gameObject.GetComponent<Image>().color = mail.iconColor;
        Sprite phishIcon;
        string legitText;
        Color32 legitColor;
        if (mail.isPhish)
        {
            legitText = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "result_details_phish_message").GetLocalizedString();
            phishIcon = endScreenManager.GetComponent<EndScreen>().phishIcon;
            legitColor = endScreenManager.GetComponent<EndScreen>().colorWrong;
        }
        else
        {
            legitText = LocalizationManager.Instance.GetStringTableEntry("strings_results_highscore", "result_details_legit_message").GetLocalizedString();
            phishIcon = endScreenManager.GetComponent<EndScreen>().noPhishIcon;
            legitColor = endScreenManager.GetComponent<EndScreen>().colorCorrect;
        }
        legitColor.a = 100;
        detailWindow.transform.Find("Window/LegitTextBox/LegitText").gameObject.GetComponent<TextMeshProUGUI>().text = legitText;
        foreach (Image image in detailWindow.transform.Find("Window/LegitTextBox/Borders").GetComponentsInChildren<Image>())
        {
            image.color = legitColor;
        }
        detailWindow.transform.Find("Window/PhishIcon").gameObject.GetComponent<Image>().sprite = phishIcon;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseDetailsWindow()
    {
        // Let menu appear again
        resultsWindow.SetActive(true);
        pages[currentPage].SetActive(true);
        endScreenManager.GetComponent<CustomMenuScreen>().SetHideControls(true);
        buttons.SetActive(true);
        detailWindow.SetActive(false);


        // Select mail that was selected
        EventSystem.current.SetSelectedGameObject(pages[currentPage].transform.Find("Mail" + selectedNumberOfMail).gameObject);
    }

    public void NextPage()
    {
        pages[currentPage].SetActive(false);
        pages[currentPage + 1].SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        currentPage++;
        pageCounter.text = (currentPage + 1) + "/" + pages.Length;
    }

    public void PreviousPage()
    {
        pages[currentPage].SetActive(false);
        pages[currentPage - 1].SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        currentPage--;
        pageCounter.text = (currentPage + 1) + "/" + pages.Length;

    }

    public void ShowInfoPopup()
    {
        infoPopup.SetActive(true);
        nextButton.SetActive(false);
        // Deactivate all buttons of mails on first page
        Button[] buttons = pages[0].GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.interactable = false;
        }
    }

    public void CloseInfoPopup()
    {
        infoPopup.SetActive(false);
        nextButton.SetActive(true);

        // Reactivate all buttons
        Button[] buttons = pages[0].GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.interactable = true;
        }

        // Select default of first page
        EventSystem.current.SetSelectedGameObject(null);
    }

}
