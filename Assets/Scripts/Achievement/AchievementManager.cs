using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class AchievementNames
{
    public string achievementId;
    public Sprite achievementIcon;
    public string achievementName;
}

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
        public int maxProgress;
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
    public Canvas MainMenu;
    private Dictionary<string, int> indexMap;
    private Dictionary<string, AchievementNames> achievementNameMap;
    private const string BaseUrl = "https://api.lutuon.app/game";

    private void Awake()
    {
        indexMap = new Dictionary<string, int>();
        for (int i = 0; i < achievements.Count; i++)
        {
            var g = achievements[i];
            if (!string.IsNullOrEmpty(g.achievementId) && !indexMap.ContainsKey(g.achievementId))
                indexMap[g.achievementId] = i;
        }

        achievementNameMap = achievementNames
            .Where(x => !string.IsNullOrEmpty(x.achievementId))
            .ToDictionary(x => x.achievementId, x => x);
    }

    private void Start()
    {
        StartCoroutine(FetchAchievement());
    }

    private IEnumerator ShowNewAchievement()
    {
        while (AccountManager.Instance.GetNewAchievements().Count > 0)
        {
            // Wait until MainMenu canvas is active
            while (MainMenu != null && !MainMenu.gameObject.activeInHierarchy)
                yield return null;

            var achievement = AccountManager.Instance.GetNewAchievements()[0];
            achievementText.text = achievement.achievementName;
            achievementIcon.sprite = achievement.achievementIcon;

            yield return Move(parent.anchoredPosition.y, visibleY);

            // Pause interval if MainMenu becomes inactive
            float elapsed = 0f;
            while (elapsed < interval)
            {
                if (MainMenu == null || !MainMenu.gameObject.activeInHierarchy)
                {
                    yield return null;
                    continue;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return Move(parent.anchoredPosition.y, hiddenOffset);

            // Remove the achievement from AccountManager
            AccountManager.Instance.RemoveNewAchievementAt(0);
        }
    }

    private IEnumerator Move(float startY, float endY)
    {
        float t = 0f;
        while (t < 1f)
        {
            // Pause movement if MainMenu is inactive
            if (MainMenu != null && !MainMenu.gameObject.activeInHierarchy)
            {
                yield return null;
                continue;
            }

            t += Time.deltaTime * speed / Mathf.Abs(endY - startY);
            var p = parent.anchoredPosition;
            p.y = Mathf.Lerp(startY, endY, t);
            parent.anchoredPosition = p;
            yield return null;
        }
    }

    public IEnumerator FetchAchievement()
    {
        if (AccountManager.Instance == null)
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

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to fetch achievements: {request.error}");
                yield break;
            }

            var res = JsonUtility.FromJson<AchievementResponse>(request.downloadHandler.text);
            if (res?.achievements == null) yield break;

            var currentAccount = AccountManager.Instance.CurrentAccount;
            var oldAchievements = currentAccount.achievements ?? new List<AchievementData>();
            var oldMap = oldAchievements.ToDictionary(a => a.achievementId, a => a);

            // Update achievements in AccountManager
            AccountManager.Instance.SetAchievements(res.achievements.ToList());

            foreach (var a in currentAccount.achievements)
            {
                if (string.IsNullOrEmpty(a.achievementId) || !indexMap.TryGetValue(a.achievementId, out int idx))
                    continue;

                var ach = achievements[idx];
                if (ach.slider == null || ach.progress == null) continue;

                float v = ach.maxProgress > 0 ? Mathf.Clamp01((float)a.progress / ach.maxProgress) : 0f;
                ach.slider.value = v;
                ach.progress.text = $"{a.progress}/{ach.maxProgress}";

                if (oldMap.TryGetValue(a.achievementId, out var oldA))
                {
                    if (a.progress != oldA.progress && a.progress == ach.maxProgress)
                    {
                        if (achievementNameMap.TryGetValue(a.achievementId, out var info))
                            AccountManager.Instance.AddNewAchievement(info);
                    }
                }
            }
        }

        yield return ShowNewAchievement();
    }
}
