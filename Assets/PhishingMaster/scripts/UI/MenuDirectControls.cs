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
using System.Linq;

/// <summary>
/// Manages menu window with multiple pages that can be traversed by arrow keys or gamepad control sticks.
/// </summary>
public class MenuDirectControls : MonoBehaviour
{
    public GameObject selfObject;
    public GameObject[] pages;
    public GameObject[] appearOnClose;
    private int currentPage;

    private bool readyForAxis;

    void Start()
    {
        currentPage = 0;
        readyForAxis = true;
    }

    void Update()
    {
        if (selfObject && selfObject.activeSelf && Input.GetButtonDown(GameConstants.k_ButtonNameCancel))
        {
            CloseMenu();
        }

        if (selfObject.activeSelf && (Input.GetAxis(GameConstants.k_AxisNameMenuHorizontal) != 0) && readyForAxis)
        {
            // Set to false to avoid calls happening every frame, wait until axis is 0 again
            readyForAxis = false;
            float x = Input.GetAxis(GameConstants.k_AxisNameMenuHorizontal);
            if (x > 0)
            {
                NextMenu();

            }
            else
            {
                PreviousMenu();
            }
        }
        if (selfObject.activeSelf && (Input.GetAxis(GameConstants.k_AxisNameMenuHorizontal) == 0))
        {
            if (!readyForAxis)
            {
                // Axis is 0, set to true to enable inputs again
                readyForAxis = true;
            }
        }
    }

    public void NextMenu()
    {
        if (currentPage < pages.Length - 1)
        {
            pages[currentPage + 1].SetActive(true);
            pages[currentPage].SetActive(false);
            currentPage++;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void PreviousMenu()
    {
        if (currentPage > 0)
        {
            pages[currentPage - 1].SetActive(true);
            pages[currentPage].SetActive(false);
            currentPage--;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void CloseMenu()
    {
        selfObject.SetActive(false);
        for (int i = 0; i < appearOnClose.Length; i++)
        {
            appearOnClose[i].SetActive(true);
        }

        // Deactivate current page
        pages[currentPage].SetActive(false);
        // Reset page
        currentPage = 0;
        pages[currentPage].SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }
}

