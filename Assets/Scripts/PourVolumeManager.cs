using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PourVolumeManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject arrow;
    public Button button;
    float maxHeight = 367f;
    float bestHeight = 250f;
    float initialHeight = -13f;
    bool isStopped = false;
    bool isRunning = false;
    bool isPaused = false;
    int[] deductions = {-3,-2,-1,0,-3};
    PointManager manager;
    void Start()
    {
        manager = FindFirstObjectByType<PointManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator AnimateVolumePour(float duration)
    {
        arrow.SetActive(true);
        button.gameObject.SetActive(true);
        isStopped = false;
        isPaused = false;
        isRunning = true;
        float newDuration = duration * (maxHeight - initialHeight / bestHeight - initialHeight);
        Vector3 fromPos = new Vector3(arrow.transform.localPosition.x, -13f, arrow.transform.localPosition.z);
        Vector3 toPos = new Vector3(fromPos.x, maxHeight, fromPos.z);

        float elapsed = 0f;
        List<int> boundary = GetSequence(6,(int)initialHeight, (int)maxHeight);
        while(elapsed < newDuration)
        {
            if(isPaused) continue;
            float k = elapsed / duration;
            k = k * k * (3f - 2f * k);
            if(isStopped)
            {
                float currentHeight = arrow.transform.localPosition.y;
                for(int i = 0; i < boundary.Count - 1; i++)
                {
                    if(boundary[i] < currentHeight && boundary[i+1] > currentHeight)
                    {
                        manager.point += deductions[i];
                        break;
                    }
                }
                yield break;
            }
            elapsed += Time.deltaTime;
        }
        
    }

    public void StopPouring()
    {
        isRunning = false;
        isStopped = true;
    }
    public void PausePouring()
    {
        if(!isRunning) return;
        isPaused = true;
    }

    public void ResumePouring()
    {
        if(!isRunning) return;
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
