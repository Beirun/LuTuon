using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarManager : MonoBehaviour
{
    public Slider bar;

    public bool isFinished = false; // true when progress completes
    public bool isRunning  = false;  // true while progress is active

    Coroutine runRoutine;
    bool paused;
    float t;
    float duration;
    Transform target;
    float offset;

    DragManager dragManager;

    private void Start()
    {
        dragManager = FindFirstObjectByType<DragManager>();
    }

    void Awake()
    {
        if (bar) bar.gameObject.SetActive(false);
    }

    public void StartProgress(Transform followTarget, float d, float yOffset = 0.9f)
    {
        if (!bar) return;

        StopProgress();

        target = followTarget;
        duration = d;
        offset = yOffset;
        paused = false;
        t = 0f;
        isFinished = false;
        isRunning = true;

        bar.value = 0f;
        bar.gameObject.SetActive(true);

        runRoutine = StartCoroutine(Run());
    }

    public void Pause()
    {
        paused = true;
    }

    public void Resume()
    {
        paused = false;
    }

    public void StopProgress()
    {
        if (runRoutine != null)
        {
            StopCoroutine(runRoutine);
            runRoutine = null;
        }

        if (bar)
        {
            bar.value = 0f;
            bar.gameObject.SetActive(false);
        }

        isFinished = false;
    }

    IEnumerator Run()
    {
        if (dragManager != null)
        {
            dragManager.DisableAllDragging();
        }

        Camera cam = Camera.main;

        while (t < duration)
        {
            if (target)
            {
                Vector3 p = target.position - cam.transform.up * offset;
                Vector3 screen = cam.WorldToScreenPoint(p);
                if (screen.z < 0) screen = cam.WorldToScreenPoint(target.position);
                bar.transform.position = screen;
            }

            if (!paused)
            {
                t += Time.deltaTime;
                bar.value = Mathf.Clamp01(t / duration);
            }

            yield return null;
        }

        bar.value = 1f;
        bar.gameObject.SetActive(false);

        if (dragManager != null)
        {
            dragManager.RestoreDraggingState();
        }

        isFinished = true;
        isRunning = false;
        runRoutine = null;
    }
}
