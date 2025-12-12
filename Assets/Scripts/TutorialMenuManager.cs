using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMenuManager : MonoBehaviour
{
    [Serializable]
    public class GameplayLock
    {
        public string foodId;
        public GameObject foodLock;
    }

    public List<GameplayLock> GameplayLocks = new List<GameplayLock>();
    Dictionary<string, int> indexMap;

    void Awake()
    {
        indexMap = new Dictionary<string, int>();
        for (int i = 0; i < GameplayLocks.Count; i++)
        {
            var g = GameplayLocks[i];
            if (!string.IsNullOrEmpty(g.foodId) && !indexMap.ContainsKey(g.foodId))
                indexMap[g.foodId] = i;
        }
    }

    void Update()
    {
        UpdateTutorialMenu();
    }

    void UpdateTutorialMenu()
    {
        var attempts = AccountManager.Instance.CurrentAccount.attempts;
        if (attempts == null || GameplayLocks.Count == 0 || indexMap.Count == 0) return;

        foreach (var a in attempts)
        {
            if (a == null || string.IsNullOrEmpty(a.foodId)) continue;
            if (!indexMap.TryGetValue(a.foodId, out int idx)) continue;

            int next = idx + 1;
            if (next < GameplayLocks.Count)
            {
                var obj = GameplayLocks[next].foodLock;
                if (obj) obj.SetActive(!a.tutorialUnlock);
            }
        }
    }
}
