using UnityEngine;
using System;
using System.Collections.Generic;

// --- DATA CLASSES ---
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
    public string dateCompleted; // Keeping as string for JSON compatibility
}

// Used for In-Memory Storage (Full Data)
[Serializable]
public class AccountData
{
    // User Profile
    public string userId;
    public string userEmail;
    public string userName;
    public string userDob;
    public string avatarId;

    // Tokens
    public string accessToken;
    public string refreshToken;
    public DateTime accessTokenExpiry;

    // Game Data
    public List<AttemptData> attempts;
    public StatsData stats;
    public List<AchievementData> achievements;
}

// Used for Disk Storage (Tokens Only)
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

        // Load tokens immediately on startup
        LoadTokens();
    }

    // Called when Login/Google/FetchDetails returns full data
    public void SetAccountData(AccountData data)
    {
        CurrentAccount = data;
        SaveTokens(); // We only extract and save the tokens from this
    }

    // Called when we just refresh the token (data hasn't changed, just the key)
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

    // Helper: Do we have a token? (Used to start the fetching process)
    public bool HasTokens()
    {
        return CurrentAccount != null && !string.IsNullOrEmpty(CurrentAccount.accessToken);
    }

    // Helper: Is the user fully loaded? (Used to hide the Loading Screen)
    public bool IsLoggedIn()
    {
        // We check for userId to confirm the profile fetch finished
        return HasTokens() && !string.IsNullOrEmpty(CurrentAccount.userId);
    }

    // --- SAVE/LOAD LOGIC ---

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

                // Initialize AccountData with ONLY tokens. 
                // The rest (userId, stats) is null until we fetch it.
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