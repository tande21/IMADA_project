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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the circular progress bar.
/// </summary>
public class ProgressBarCircle : MonoBehaviour
{
    public Text progressText;
    public Image progressImage;


    private float currentValue;
    private float targetValue;
    private float difference;
    private float speed;
    private float timeToDespawn = 0.1f;
    private float timeTillCompletion = 0;
    private float remainingTime = 0;

    // Update is called once per frame
    void Update()
    {
        if (currentValue < 100)
        {
            if (remainingTime > 0)
            {
                UpdateTarget(Math.Min(Math.Max(((timeTillCompletion - remainingTime) / timeTillCompletion) * 100, currentValue), 99f));
                remainingTime -= Time.deltaTime;
            }
            currentValue += Math.Min(targetValue, speed * Time.deltaTime);
            progressText.text = ((int)currentValue).ToString() + "%";
            if (targetValue < 100)
            {
                speed = (targetValue - currentValue) / 2f;
            }
            else
            {
                speed = Math.Max(targetValue - currentValue, difference / 0.3f);
            }
        }
        else
        {
            progressText.text = "100%";
            timeToDespawn -= Time.deltaTime;
            if (timeToDespawn < 0f)
            {
                gameObject.SetActive(false);
            }
        }

        progressImage.fillAmount = currentValue / 100;
    }

    public void Reset()
    {
        currentValue = 0f;
        targetValue = 0f;
        speed = 0f;
        timeToDespawn = 0.1f;
    }

    private void UpdateTarget(float percent)
    {
        targetValue = percent;
        difference = targetValue - currentValue;
    }

    public void SetComplete()
    {
        this.SetTargetProgress(100);
    }

    /// <summary>
    /// Set the percentage that the progress bar will go to in the next time.
    /// Use this or SetEstimatedTimeTillCompletion().
    /// </summary>
    /// <param name="progressPercent">Target percentage.</param>
    public void SetTargetProgress(float progressPercent)
    {
        this.remainingTime = 0;
        this.timeTillCompletion = 0;
        UpdateTarget(progressPercent);
    }

    /// <summary>
    /// Set the estimated time how long it should take to fill the progress bar.
    /// This will then automatically increase the target percentage to 99% during that time.
    /// Use this or SetTargetProgress().
    /// </summary>
    /// <param name="seconds">Target time in seconds.</param>
    public void SetEstimatedTimeTillCompletion(float seconds)
    {
        this.timeTillCompletion += seconds;
        this.remainingTime = seconds;
    }
}
