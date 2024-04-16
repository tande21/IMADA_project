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
using UnityEngine.UI;

/// <summary>
/// Manages user inputs to select menu items based on input.
/// </summary>
public class CustomMenuNavigation : MonoBehaviour
{
    public Selectable defaultSelection;

    void Start()
    {
    }

    void LateUpdate()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (Input.GetButtonDown(GameConstants.k_ButtonNameSubmit)
                || Input.GetAxisRaw(GameConstants.k_AxisNameMenuHorizontal) != 0
                || Input.GetAxisRaw(GameConstants.k_AxisNameMenuVertical) != 0)
            {
                if (defaultSelection.IsActive())
                {
                    // if element is active, set selection
                    EventSystem.current.SetSelectedGameObject(defaultSelection.gameObject);
                }
            }
        }
        //Check for mouse input
        if (Input.GetAxis(GameConstants.k_MouseAxisNameHorizontal) != 0 ||
            Input.GetAxis(GameConstants.k_MouseAxisNameVertical) != 0 ||
            Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                //clear selection if not focused on input field
                InputField component = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
                if (component == null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }
    }
}
