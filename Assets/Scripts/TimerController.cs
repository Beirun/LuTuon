using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    public TMP_Text timerText;
    public bool isFinished = false;

    float t;
    float target;
    float speed;
    [HideInInspector]
    public bool running = false;
    bool pulsing;
    [HideInInspector]
    public bool paused = false;

    void Update()
    {
        if (!running) return;
        if (paused) return;

        t += Time.deltaTime * speed;

        if (t >= target)
        {
            t = target;
            if (!pulsing)
                StartCoroutine(PulseThenHide());
        }

        int m = Mathf.FloorToInt(t / 60f);
        int s = Mathf.FloorToInt(t % 60f);
        timerText.text = $"{m:00}:{s:00}";
    }

    public void StartTimer(float minutes, float fastFactor = 2f)
    {
        if (running)
        {
            Resume();
            return;
        }
        target = minutes * 60f;
        speed = fastFactor * 60f;
        t = 0f;

        timerText.text = "00:00";
        isFinished = false;
        running = true;
        paused = false;
        pulsing = false;
        gameObject.SetActive(true);
    }

    public void Pause()
    {
        if (!running) return;
        paused = true;
    }

    public void Resume()
    {
        if (!running) return;
        paused = false;
    }

    IEnumerator PulseThenHide()
    {
        pulsing = true;

        Vector3 baseScale = timerText.transform.localScale;
        float dur = 0.25f;

        for (int i = 0; i < 2; i++)
        {
            float e = 0f;
            while (e < 1f)
            {
                e += Time.deltaTime / dur;
                float s = Mathf.Lerp(1f, 1.2f, e);
                timerText.transform.localScale = baseScale * s;
                yield return null;
            }

            e = 0f;
            while (e < 1f)
            {
                e += Time.deltaTime / dur;
                float s = Mathf.Lerp(1.2f, 1f, e);
                timerText.transform.localScale = baseScale * s;
                yield return null;
            }
        }

        timerText.transform.localScale = baseScale;
        gameObject.SetActive(false);
        running = false;
        isFinished = true;
    }
}
