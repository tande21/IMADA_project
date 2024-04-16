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
using TMPro;

/// <summary>
/// 3D-Text pop-up.
/// Can be used to display a text message for a short period of time.
/// </summary>
public class HitPopup : MonoBehaviour
{
    public static HitPopup Create(Vector3 spawnPosition, string text, Color textColor)
    {
        return Create(spawnPosition, text, Vector3.zero, textColor);
    }

    /// <summary>
    /// Create a text pop-up.
    /// </summary>
    /// <param name="spawnPosition">Position of the text.</param>
    /// <param name="text">The text.</param>
    /// <param name="moveSpeed">Direction and speed of the text.</param>
    /// <param name="textColor">Text color.</param>
    /// <returns></returns>
    public static HitPopup Create(Vector3 spawnPosition, string text, Vector3 moveSpeed, Color textColor)
    {
        Transform hitPopupTransform = Instantiate(GameAssets.i.HitPopupPrefab, spawnPosition, Quaternion.identity);
        HitPopup hitPopup = hitPopupTransform.GetComponent<HitPopup>();
        hitPopup.Setup(text, moveSpeed, textColor);

        return hitPopup;
    }

    private const float DISAPPEAR_TIMER_MAX = 3f;

    private TextMeshPro textMesh;
    private Vector3 moveSpeed;
    private float disappearTimer;
    private Color textColor;

    private void Awake()
    {
        textMesh = transform.GetComponent<TextMeshPro>();
    }

    public void Setup(string text, Vector3 moveSpeed, Color textColor)
    {
        this.textMesh.SetText(text);
        this.moveSpeed = moveSpeed;
        this.textColor = textColor;
        textMesh.color = this.textColor;
        this.disappearTimer = DISAPPEAR_TIMER_MAX;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveSpeed * Time.deltaTime;

        if (disappearTimer > DISAPPEAR_TIMER_MAX * 0.5f)
        { //First half of lifetime, increase size
            float increaseScaleSpeed = 0.1f;
            transform.localScale += Vector3.one * increaseScaleSpeed * Time.deltaTime;
        }
        else
        { //Second half of lifetime, decrease size
            float decreaseScaleSpeed = 0.25f;
            transform.localScale -= Vector3.one * decreaseScaleSpeed * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            //Start disappearing
            float disappearSpeed = 1f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a < 0)
            { //invisible
                Destroy(gameObject);
            }
        }
    }
}
