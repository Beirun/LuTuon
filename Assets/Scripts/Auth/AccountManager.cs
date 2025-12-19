using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class AttemptData
{
    public string foodId;
    public string foodName;
    public int highestPoint;
    public int numberOfAttempts;
    public bool tutorialUnlock;
}

[Serializable]
public class StatsData
{
    public int totalAttempts;
    public int totalPoints;
    public int totalAchievements;

}

[Serializable]
public class AchievementData
{
    public string achievementId;
    public string achievementName;
    public int progress;
    public string dateCompleted; 
}

[Serializable]
public class AccountData
{
    public string userId;
    public string userEmail;
    public string userName;
    public string userDob;
    public string avatarId;

    public string accessToken;
    public string refreshToken;
    public DateTime accessTokenExpiry;

    public List<AttemptData> attempts;
    public StatsData stats;
    public List<AchievementData> achievements;
    public bool isFirstTimeLogin;
}

[Serializable]
public class TokenData
{
    public string accessToken;
    public string refreshToken;
    public long expiryTicks;
}

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }
    public AccountData CurrentAccount { get; private set; }
    private List<AchievementNames> newAchievement = new List<AchievementNames>();

    private const string PREFS_KEY = "Lutuon_UserTokens";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadTokens();
    }

    public void SetAccountData(AccountData data)
    {
        CurrentAccount = data;
        SaveTokens(); 
    }
    public List<AchievementNames> GetNewAchievements()
    {
        return newAchievement;
    }
    public void AddNewAchievement(AchievementNames achievement)
    {
        newAchievement.Add(achievement);
    }
    public void RemoveNewAchievementAt(int index)
    {
        if (index >= 0 && index < newAchievement.Count)
        {
            newAchievement.RemoveAt(index);
        }
    }

    public void SetAchievements(List<AchievementData> achievements)
    {
        CurrentAccount.achievements = achievements;
    }

    public void SetAttemptData(List<AttemptData> attempts)
    {
        CurrentAccount.attempts = attempts;
    }

    public void SetStatsData(StatsData stats)
    {
        CurrentAccount.stats = stats;
    }

    public void UpdateTokens(string newAccess, string newRefresh, DateTime newExpiry)
    {
        if (CurrentAccount == null) CurrentAccount = new AccountData();

        CurrentAccount.accessToken = newAccess;
        if (!string.IsNullOrEmpty(newRefresh)) CurrentAccount.refreshToken = newRefresh;
        CurrentAccount.accessTokenExpiry = newExpiry;

        SaveTokens();
    }

    public void ClearAccountData()
    {
        CurrentAccount = null;
        PlayerPrefs.DeleteKey(PREFS_KEY);
        PlayerPrefs.Save();
    }

    public bool HasTokens()
    {
        return CurrentAccount != null && !string.IsNullOrEmpty(CurrentAccount.accessToken);
    }

    public bool IsLoggedIn()
    {
        return HasTokens() && !string.IsNullOrEmpty(CurrentAccount.userId);
    }


    private void SaveTokens()
    {
        if (CurrentAccount != null)
        {
            TokenData t = new TokenData
            {
                accessToken = CurrentAccount.accessToken,
                refreshToken = CurrentAccount.refreshToken,
                expiryTicks = CurrentAccount.accessTokenExpiry.Ticks
            };
            string json = JsonUtility.ToJson(t);
            PlayerPrefs.SetString(PREFS_KEY, json);
            PlayerPrefs.Save();
        }
    }

    private void LoadTokens()
    {
        if (PlayerPrefs.HasKey(PREFS_KEY))
        {
            try
            {
                string json = PlayerPrefs.GetString(PREFS_KEY);
                TokenData t = JsonUtility.FromJson<TokenData>(json);

                CurrentAccount = new AccountData
                {
                    accessToken = t.accessToken,
                    refreshToken = t.refreshToken,
                    accessTokenExpiry = new DateTime(t.expiryTicks)
                };
                Debug.Log("Tokens loaded from disk.");
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load tokens: " + e.Message);
                ClearAccountData();
            }
        }
    }
}