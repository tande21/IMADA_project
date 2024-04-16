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

/// <summary>
/// Manages a button that calls a next menu/window.
/// </summary>
public class NextButtonNoExit : MonoBehaviour
{
    public GameObject selfObject;
    public GameObject nextMenu;
    public bool enableCancelButtonToClose = true;

    public GameObject[] appearOnClose;
    private bool disableCancelButtonToCloseTemporary = false;

    void Start()
    {
    }

    void Update()
    {
        if (selfObject && selfObject.activeSelf && Input.GetButtonDown(GameConstants.k_ButtonNameCancel) && enableCancelButtonToClose && !disableCancelButtonToCloseTemporary)
        {
            CloseWindow();
        }
    }

    public void OpenResults()
    {
        nextMenu.SetActive(true);
        selfObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void NextWindow()
    {
        nextMenu.SetActive(true);
        selfObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseWindow()
    {
        selfObject.SetActive(false);
        // let buttons appear again
        for (int i = 0; i < appearOnClose.Length; i++)
        {
            appearOnClose[i].SetActive(true);
        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    public bool DisableCancelButtonToCloseTemporary { get => disableCancelButtonToCloseTemporary; set => disableCancelButtonToCloseTemporary = value; }
}
