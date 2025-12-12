using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    [Serializable]
    public class AchievementObject
    {
        public string achievementId;
        public Slider slider;
        public TMP_Text progress;
    }

    public List<AchievementObject> achievements = new List<AchievementObject>();
    Dictionary<string, int> indexMap;

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

}
