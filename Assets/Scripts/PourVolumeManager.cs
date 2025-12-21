using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PourVolumeManager : MonoBehaviour
{
    public GameObject arrow;
    public GameObject bar;
    public Button button;
    float maxHeight = 167f;
    float bestHeight = 50f;
    float initialHeight = -213f;
    bool isStopped = true;
    bool isRunning = false;
    bool isPaused = false;
    int[] deductions = { -3, -2, -1, 0, -3 };
    PointManager manager;

    List<GameObject> overlays = new List<GameObject>();

    void Start()
    {
        manager = FindFirstObjectByType<PointManager>();

        Canvas c = FindFirstObjectByType<Canvas>();
        string[] overlayNames = { "BlackOverlay", "ExtraOverlay", "EndOverlay", "TutorialOverlay" };
        foreach (var name in overlayNames)
        {
            Transform t = FindDeepChild(c.transform, name);
            if (t != null)
                overlays.Add(t.gameObject);
        }
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    public IEnumerator ShowVolumePour(float duration)
    {
        Debug.LogWarning("Pour");
        arrow.SetActive(true);
        bar.SetActive(true);
        if (manager != null) button.gameObject.SetActive(true);
        isStopped = false;
        isPaused = false;
        isRunning = true;
        Vector3 fromPos = new Vector3(arrow.transform.localPosition.x, initialHeight, arrow.transform.localPosition.z);
        Vector3 toPos = new Vector3(fromPos.x, maxHeight, fromPos.z);

        float elapsed = 0f;
        List<int> boundary = GetSequence(6, (int)initialHeight, (int)maxHeight);

        while (elapsed < duration)
        {
            // Automatically pause if any overlay is active
            bool overlayActive = false;
            foreach (var overlay in overlays)
            {
                if (overlay && overlay.activeSelf)
                {
                    overlayActive = true;
                    break;
                }
            }
            isPaused = overlayActive;

            if (!isPaused)
            {
                float k = elapsed / duration;
                k = k * k * (3f - 2f * k);
                arrow.transform.localPosition = Vector3.Lerp(fromPos, toPos, k);
                float currentHeight = arrow.transform.localPosition.y;

                if (manager == null)
                {
                    if (currentHeight > bestHeight)
                    {
                        EndPour();
                        yield break;
                    }
                }

                if (isStopped)
                {
                    for (int i = 0; i < boundary.Count - 1; i++)
                    {
                        if (boundary[i] < currentHeight && boundary[i + 1] > currentHeight)
                        {
                            manager.point += deductions[i];
                            break;
                        }
                    }
                    EndPour();
                    yield break;
                }

                elapsed += Time.deltaTime;
            }

            yield return null;
        }

        if (manager != null)
            manager.point += deductions[4];

        EndPour();
    }

    void EndPour()
    {
        arrow.SetActive(false);
        bar.SetActive(false);
        if (manager != null) button.gameObject.SetActive(false);
        isRunning = false;
        isStopped = true;
    }

    public void StopPouring()
    {
        isRunning = false;
        isStopped = true;
    }

    public void PausePouring()
    {
        if (!isRunning) return;
        isPaused = true;
    }

    public void ResumePouring()
    {
        if (!isRunning) return;
        isPaused = false;
    }

    public List<int> GetSequence(int n, int a1, int an)
    {
        int diffNum = an - a1;
        int denom = n - 1;
        int d = diffNum / denom;
        var res = new List<int>(n);
        for (int i = 0; i < n; i++)
            res.Add(a1 + i * d);
        return res;
    }
}
