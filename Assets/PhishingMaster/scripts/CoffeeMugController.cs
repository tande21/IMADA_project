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

public class CoffeeMugController : MonoBehaviour
{

    public Light[] lights;

    private bool lightColorChanged = false;

    private Vector3 popupOffset = new Vector3(0, 3, 0);

    void Start()
    {
        GetComponent<HitDetectable>().onHitDetected += OnHit;
    }

    void OnHit(Vector3 hitPosition)
    {
        if (!lightColorChanged)
        {
            Color color = new Color(Random.value, Random.value, Random.value, 1.0f);
            SetLightColorAndPopUp(hitPosition + popupOffset, color);
            lightColorChanged = true;
        }
        else
        {
            SetLightColorAndPopUp(hitPosition + popupOffset, Color.white);
            lightColorChanged = false;
        }
    }

    private void SetLightColorAndPopUp(Vector3 popupPosition, Color color)
    {
        HitPopup.Create(popupPosition, "*", new Vector3(0, 5, 0), color);
        foreach (Light light in lights)
        {
            light.color = color;
        }
    }
}