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

/// <summary>
/// Sets text for skip tutorial pop up at main menu.
/// </summary>
public class SkipTutorialPopUp : MonoBehaviour
{

    public Text mainText;
    public Text skipButtonText;
    public Text tutorialButtonText;
    // Start is called before the first frame update
    void Start()
    {
        UpdateText();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        string trainingName = DataManager.GetLevels()[DataManager.currentLevel].name;
        string nextLevelName = DataManager.GetLevels()[Mathf.Min(DataManager.currentLevel + 1, DataManager.GetLevels().Length - 1)].name;
        mainText.text = LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "skip_tutorial_popup_details").GetLocalizedString(trainingName, nextLevelName);
        tutorialButtonText.text = LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "start_tutorial_text").GetLocalizedString(trainingName);
        skipButtonText.text = LocalizationManager.Instance.GetStringTableEntry("strings_tutorial", "start_with_level").GetLocalizedString(nextLevelName);
    }
}
