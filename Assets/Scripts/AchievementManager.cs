using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class AchievementManager : MonoBehaviour
{
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
    public float speed = 300f;
    public float visibleY = 0f;
    public float hiddenOffset = -200f;
    public TMP_Text achievementText;
    public Image achievementIcon;
    Dictionary<string, int> indexMap;

    private void Start()
    {
        ShowNewAchievement();
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
        //if(AccountManager.Instance == null || AccountManager.Instance.CurrentAccount == null) return;   
        //var ds = AccountManager.Instance.CurrentAccount.achievements;
        //if (ds == null || achievements.Count == 0 || indexMap.Count == 0) return;
        //foreach (var a in ds)
        //{
        //    if (a == null || string.IsNullOrEmpty(a.achievementId)) continue;
        //    if (!indexMap.TryGetValue(a.achievementId, out int idx)) continue;
        //    var ach = achievements[idx];
        //    if (!ach.slider) continue;
        //    if (ach.slider.value >= 1f)
        //    {
        //        // Show achievement unlocked panel
        //        var names = achievementNames.Find(x => x.achievementId == a.achievementId);
        //        if (names != null && achievementUnlockPanel != null)
        //        {
        //            var panel = achievementUnlockPanel.GetComponent<AchievementUnlockPanel>();
        //            if (panel != null)
        //            {
        //                panel.ShowAchievement(names.achievementIcon, names.achievementName);
        //            }
        //        }
        //        a.isNew = false; // Mark as shown
        //    }
        //}
    }

    IEnumerator NewAchievement()
    {
        while (true)
        {
            achievementText.text = achievementNames[0].achievementName;
            achievementIcon.sprite = achievementNames[0].achievementIcon;
            yield return Move(parent.anchoredPosition.y, visibleY);
            yield return new WaitForSeconds(interval);
            yield return Move(parent.anchoredPosition.y, hiddenOffset);
            yield return new WaitForSeconds(1f);
        }
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
}
