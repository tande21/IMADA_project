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
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Disables the cancel funtionality on all objects in disableCancelFunction, when an object in gameObjects is selected.
/// </summary>
public class DisableCancelButton : MonoBehaviour
{
    public GameObject[] gameObjects;
    public NextButtonNoExit[] disableCancelFunction;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool disable = false;
        foreach (GameObject go in gameObjects)
        {
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.Equals(go))
            {
                disable = true;
                break;
            }
        }
        if (disable)
        {
            foreach (NextButtonNoExit nbne in disableCancelFunction)
            {
                nbne.DisableCancelButtonToCloseTemporary = true;
            }
        }
        else
        {
            foreach (NextButtonNoExit nbne in disableCancelFunction)
            {
                nbne.DisableCancelButtonToCloseTemporary = false;
            }
        }
    }
}
