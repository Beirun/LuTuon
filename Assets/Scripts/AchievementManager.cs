using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    [Serializable]
    public class AchievementResponse
    {
        public AchievementData[] achievements;
    }
    [Serializable]
    public class AchievementObject
    {
        public string achievementId;
        public Slider slider;
        public TMP_Text progress;
    }

    [Serializable]
    public class AchievementNames
    {
        public string achievementId;
        public Sprite achievementIcon;
        public string achievementName;
    }

    public List<AchievementObject> achievements = new List<AchievementObject>();
    public List<AchievementNames> achievementNames = new List<AchievementNames>();
    public int interval = 3;
    public RectTransform parent;
    private float speed = 300f;
    private float visibleY = -22.5f;
    private float hiddenOffset = 92;
    public TMP_Text achievementText;
    public Image achievementIcon;
    Dictionary<string, int> indexMap;
    private const string BaseUrl = "https://api.lutuon.app/game";
    private void Start()
    {
        StartCoroutine(FetchAchievement());
    }

    void Awake()
    {
        indexMap = new Dictionary<string, int>();
        for (int i = 0; i < achievements.Count; i++)
        {
            var g = achievements[i];
            if (!string.IsNullOrEmpty(g.achievementId) && !indexMap.ContainsKey(g.achievementId))
                indexMap[g.achievementId] = i;
        }
    }

    void Update()
    {
        UpdateAchievement();
    }

    void UpdateAchievement()
    {
        if(AccountManager.Instance == null || AccountManager.Instance.CurrentAccount == null) return;   
        var ds = AccountManager.Instance.CurrentAccount.achievements;
        if (ds == null || achievements.Count == 0 || indexMap.Count == 0) return;

        foreach (var a in ds)
        {
            if (a == null || string.IsNullOrEmpty(a.achievementId)) continue;
            if (!indexMap.TryGetValue(a.achievementId, out int idx)) continue;

            var ach = achievements[idx];
            if (!ach.slider || !ach.progress) continue;

            var t = ach.progress.text;
            var parts = t.Split('/');
            if (parts.Length != 2) continue;

            if (!float.TryParse(parts[1], out float max)) continue;
            if (max <= 0f) continue;

            float v = Mathf.Clamp01(a.progress / max);
            ach.slider.value = v;
        }
    }

    void ShowNewAchievement()
    {

        StartCoroutine(NewAchievement());
        
    }

    IEnumerator NewAchievement()
    {
        achievementText.text = achievementNames[0].achievementName;
        achievementIcon.sprite = achievementNames[0].achievementIcon;
        yield return Move(parent.anchoredPosition.y, visibleY);
        yield return new WaitForSeconds(interval);
        yield return Move(parent.anchoredPosition.y, hiddenOffset);
    }
    IEnumerator Move(float startY, float endY)
    {
        var t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed / Mathf.Abs(endY - startY);
            var p = parent.anchoredPosition;
            p.y = Mathf.Lerp(startY, endY, t);
            parent.anchoredPosition = p;
            yield return null;
        }
    }
    public IEnumerator FetchAchievement()
    {
        if(AccountManager.Instance == null)
        {
            Debug.LogError("AccountManager instance not found");
            yield break;
        }
        string token = AccountManager.Instance.CurrentAccount.accessToken;

        using (UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/profile/achievements"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                var res = JsonUtility.FromJson<AchievementResponse>(jsonResponse);
                
                AccountManager.Instance.SetAchievements(res.achievements.ToList());
            }
        }
    }
}
