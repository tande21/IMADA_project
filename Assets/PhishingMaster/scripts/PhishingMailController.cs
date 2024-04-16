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
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a single mail object.
/// Handles movement and hit detection.
/// </summary>
public class PhishingMailController : MonoBehaviour
{
    public bool isPhish;
    public Vector4 phishArea;
    public GameObject deathVFX;
    public GameObject biggerExplosionVFX;
    public GameObject keyboardFlamesVFX;
    public GameObject noPhishButtonPrefab;
    // used to get image in EndScene
    [System.NonSerialized]
    public string fileName;

    public static float LIFE_TIME_DEFAULT = 30f;

    private float despawnTimer;
    private bool hasStopped;

    HitDetectable m_HitDetectable;
    PhishingMailManager m_PhishingMailManager;

    private GameObject noPhishButton;
    private bool mailIsHittable = true;
    private bool noPhishIsHittable = true;

    // Waypoints
    public GameObject[] waypoints;
    int current = 0;
    public float speed1 = 2;
    public float speed2 = 2;
    float WPradius = 0.001f;
    float maxDistance;
    float speedMultiplier = 1;

    void Start()
    {
        m_PhishingMailManager = FindObjectOfType<PhishingMailManager>();
        DebugUtility.HandleErrorIfNullFindObject<PhishingMailManager, PhishingMailController>(m_PhishingMailManager, this);
        m_HitDetectable = GetComponent<HitDetectable>();
        DebugUtility.HandleErrorIfNullGetComponent<HitDetectable, PhishingMailController>(m_HitDetectable, this, gameObject);
        m_HitDetectable.onHitDetected += OnHit;

        m_PhishingMailManager.RegisterEnemy(this);

        despawnTimer = LIFE_TIME_DEFAULT;
        if (QualitySettings.names[QualitySettings.GetQualityLevel()].Equals("Low"))
        {
            biggerExplosionVFX = deathVFX;
        }

        // Waypoints
        maxDistance = Vector3.Distance(waypoints[current].transform.position, transform.position);
    }

    private void Update()
    {
        if (!hasStopped)
        {
            despawnTimer -= Time.deltaTime;
            if (despawnTimer < 0)
            {
                HitPopup.Create(transform.position, LocalizationManager.Instance.GetStringTableEntry("strings_game_scene", "time_up").GetLocalizedString(), new Vector3(1, 1, 3), DataManager.kitOrange);
                DestroyWrong();
            }

            m_PhishingMailManager.UpdateRemainingLifeTime(despawnTimer);

            // Waypoints
            UpdatePosition();

        }
    }

    private void DestroyPhishCorrect(bool bonus)
    {
        var vfx = Instantiate(biggerExplosionVFX, transform.position, biggerExplosionVFX.transform.rotation);
        Destroy(vfx, 4f);
        DestroyCorrect(bonus);
    }

    private void DestroyLegitCorrect(bool bonus)
    {
        var vfx = Instantiate(biggerExplosionVFX, transform.position, biggerExplosionVFX.transform.rotation);
        Destroy(vfx, 4f);
        DestroyCorrect(bonus);
    }

    private void DestroyLegitWrong()
    {
        var vfx = Instantiate(deathVFX, transform.position, deathVFX.transform.rotation);
        Destroy(vfx, 4f);
        DestroyWrong();
    }

    private void DestroyPhishWrong()
    {
        var vfx = Instantiate(keyboardFlamesVFX);
        Destroy(vfx, 10f);
        DestroyWrong();
    }

    private void DestroyCorrect(bool bonus)
    {
        m_PhishingMailManager.UnregisterEnemy(this, true, bonus, Mathf.Max(LIFE_TIME_DEFAULT - despawnTimer, 0));
        Destroy(noPhishButton.gameObject, 0f);
        Destroy(gameObject, 0f);
    }

    private void DestroyWrong()
    {
        m_PhishingMailManager.UnregisterEnemy(this, false, Mathf.Max(LIFE_TIME_DEFAULT - despawnTimer, 0));
        Destroy(noPhishButton.gameObject, 0f);
        Destroy(gameObject, 0f);
    }

    private void UpdatePosition()
    {
        if (Vector3.Distance(waypoints[current].transform.position, transform.position) < WPradius)
        {
            if (current < waypoints.Length - 1)
            {
                // switch to next waypoint
                current++;

                // update current maxDistance and reset speedMultiplier
                maxDistance = Vector3.Distance(waypoints[current].transform.position, transform.position);
                speedMultiplier = 1;
            }
        }
        // slow speed if last waypoint
        if (current == waypoints.Length - 1)
        {
            speedMultiplier = speed2 * Vector3.Distance(waypoints[current].transform.position, transform.position) / maxDistance;
        }
        else
        {
            speedMultiplier = speed1;
        }
        this.transform.LookAt(waypoints[current].transform.position);
        transform.position = Vector3.MoveTowards(transform.position, waypoints[current].transform.position, Time.deltaTime * speedMultiplier);
    }

    public void SetPhishingMailTexture(Texture2D texture, bool isPhish, Vector4 phishArea, string fileName)
    {
        this.GetComponent<Renderer>().material.mainTexture = texture;
        this.isPhish = isPhish;
        this.phishArea = phishArea;
        noPhishButton = Instantiate(noPhishButtonPrefab);
        noPhishButton.GetComponent<HitDetectable>().onHitDetected += OnNoPhishButtonHit;

        //Adjust size of object to match texture size
        float ratio = (float)texture.width / texture.height;
        if (ratio > 16f / 9f)
        {
            ratio = 16f / 9f;
        }
        Vector3 newScale = transform.localScale;
        newScale.Scale(new Vector3(ratio, 1, 1));
        transform.localScale = newScale;
        this.fileName = texture.name;
    }

    private Vector3 noPhishPopupSpawnOffset = new Vector3(0, 4, 0);

    /// <summary>
    /// Handles destruction of mail on NoPhishButton hit.
    /// Notifies PhishingMailManager about destruction and correct decision.
    /// </summary>
    /// <param name="hitPosition">Position of the hit.</param>
    void OnNoPhishButtonHit(Vector3 hitPosition)
    {
        if (noPhishIsHittable)
        {
            if (!isPhish)
            {
                HitPopup.Create(hitPosition + noPhishPopupSpawnOffset, LocalizationManager.Instance.GetStringTableEntry("strings_game_scene", "correct_popup").GetLocalizedString(), new Vector3(1, 1, 1), DataManager.kitGreen);
                DestroyLegitCorrect(false);
            }
            else
            {
                HitPopup.Create(hitPosition + noPhishPopupSpawnOffset, LocalizationManager.Instance.GetStringTableEntry("strings_game_scene", "wrong_popup").GetLocalizedString(), new Vector3(1, 1, 1), DataManager.kitRed);
                DestroyPhishWrong();
            }
        }
    }

    /// <summary>
    /// Handles destruction of mail on hit.
    /// Notifies PhishingMailManager about destruction and correct decision.
    /// </summary>
    /// <param name="hitPosition">Position of the hit used to calculate bonus points.</param>
    void OnHit(Vector3 hitPosition)
    {
        if (mailIsHittable)
        {
            Vector3 localHitPosition = this.transform.InverseTransformPoint(hitPosition);
            Vector2 relativeTexturePosition = new Vector2(
                0.5f - Mathf.Min(Mathf.Max(localHitPosition.x, -0.5f), 0.5f),
                0.5f - Mathf.Min(Mathf.Max(localHitPosition.y, -0.5f), 0.5f));

            if (isPhish)
            {
                // check for right area
                if (relativeTexturePosition.y >= phishArea[1] && relativeTexturePosition.y <= phishArea[3] && relativeTexturePosition.x >= phishArea[0] && relativeTexturePosition.x <= phishArea[2])
                {
                    HitPopup.Create(hitPosition, LocalizationManager.Instance.GetStringTableEntry("strings_game_scene", "perfect_popup").GetLocalizedString(), new Vector3(1, 1, 1), DataManager.kitGreen);
                    if (DataManager.GetLevels()[DataManager.currentLevel].showPoints)
                    {
                        HitPopup.Create(new Vector3(hitPosition.x, hitPosition.y + 3, hitPosition.z), "+" + PhishingMailManager.perfectBonusPoints, new Vector3(0, 5, 0), DataManager.kitGreen);
                    }
                    DestroyPhishCorrect(true);
                }
                else
                {
                    HitPopup.Create(hitPosition, LocalizationManager.Instance.GetStringTableEntry("strings_game_scene", "correct_popup").GetLocalizedString(), new Vector3(1, 1, 1), DataManager.kitGreen);
                    DestroyPhishCorrect(false);
                }
            }
            else
            {
                HitPopup.Create(hitPosition, LocalizationManager.Instance.GetStringTableEntry("strings_game_scene", "wrong_popup").GetLocalizedString(), new Vector3(1, 1, 1), DataManager.kitRed);
                DestroyLegitWrong();
            }
        }
    }

    public void StopMail()
    {
        hasStopped = true;
    }

    public void ContinueMail()
    {
        hasStopped = false;
    }

    public void SetMailHittable(bool active)
    {
        mailIsHittable = active;
    }

    public void SetNoPhishHittable(bool active)
    {
        noPhishIsHittable = active;
    }
}
