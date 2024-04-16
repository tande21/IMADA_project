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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays conroller/mouse icons based on input device in main menu.
/// </summary>
public class MainMenuControllerIcons : MonoBehaviour
{
    public GameObject[] controllerButtons;
    public GameObject[] mouseButtons;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleCursorLocking();
    }

    private bool controllerInputActive = true;
    private void HandleCursorLocking()
    {
        if (Input.GetAxis(GameConstants.k_AxisNameMenuHorizontal) != 0 ||
            Input.GetAxis(GameConstants.k_AxisNameMenuVertical) != 0)
        {
            controllerInputActive = true;
        }
        if (Input.GetAxis(GameConstants.k_MouseAxisNameHorizontal) != 0 ||
            Input.GetAxis(GameConstants.k_MouseAxisNameVertical) != 0 ||
            Input.GetKeyDown(KeyCode.Mouse0))
        {
            controllerInputActive = false;
        }
        if (controllerInputActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            foreach (GameObject button in controllerButtons)
            {
                button.SetActive(true);
            }
            foreach (GameObject button in mouseButtons)
            {
                button.SetActive(false);
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            foreach (GameObject button in controllerButtons)
            {
                button.SetActive(false);
            }
            foreach (GameObject button in mouseButtons)
            {
                button.SetActive(true);
            }
        }
    }
}
