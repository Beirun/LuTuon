using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public TMP_Text txt;
    public bool autoStart = true;

    float t;
    bool running;

    void Start()
    {
        if (!txt) txt = GetComponent<TMP_Text>();
        ResetTimer();
        if (autoStart) StartTimer();
    }

    void Update()
    {
        if (!running) return;

        t += Time.deltaTime;
        UpdateText();
    }

    public void StartTimer()
    {
        running = true;
    }

    public void StopTimer()
    {
        running = false;
    }

    public void ResetTimer()
    {
        t = 0f;
        UpdateText();
    }

    void UpdateText()
    {
        int totalSeconds = Mathf.FloorToInt(t);
        int seconds = totalSeconds % 60;
        int minutes = (totalSeconds / 60) % 1000;

        if (minutes >= 100)
            txt.text = $"{minutes:000}:{seconds:00}";
        else
            txt.text = $"{minutes:00}:{seconds:00}";
    }
}
