using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class AttemptData
{
    public string foodId;
    public string foodName;
    public int highestPoint;
    public bool tutorialUnlock;
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
    public List<AttemptData> attempts; // new
}


public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }
    public AccountData CurrentAccount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetAccountData(AccountData data)
    {
        CurrentAccount = data;
    }

    public void ClearAccountData()
    {
        CurrentAccount = null;
    }

    public bool IsLoggedIn()
    {
        return CurrentAccount != null && !string.IsNullOrEmpty(CurrentAccount.accessToken);
    }
}
