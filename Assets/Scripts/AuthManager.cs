using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class LoginRequest { public string email; public string password; }


[Serializable]
public class LoginResponse
{
    public string accessToken;
    public string refreshToken;
    public User user;
    public AttemptData[] attempts;
    public StatsData stats;
    public AchievementData[] achievements;
}

[Serializable]
public class User
{
    public string userId;
    public string userEmail;
    public string userName;
    public string userDob;
    public string avatarId;
}

[Serializable]
public class RefreshRequest { public string refreshToken; }

public class AuthManager : MonoBehaviour
{
    private const string BaseUrl = "https://api.lutuon.app/game";
    private Coroutine autoRefreshCoroutine;

    public void Login(string email, string password, Action<bool, string> callback)
    {
        StartCoroutine(LoginCoroutine(email, password, callback));
    }

    private IEnumerator LoginCoroutine(string email, string password, Action<bool, string> callback)
    {
        var reqData = new LoginRequest { email = email, password = password };
        string json = JsonUtility.ToJson(reqData);

        using (UnityWebRequest request = new UnityWebRequest($"{BaseUrl}/login", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                callback(false, request.error);
            }
            else
            {
                var res = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                DateTime expiry = DateTime.UtcNow.AddHours(1);

                AccountManager.Instance.SetAccountData(new AccountData
                {
                    userId = res.user.userId,
                    userEmail = res.user.userEmail,
                    userName = res.user.userName,
                    userDob = res.user.userDob,
                    avatarId = res.user.avatarId,
                    accessToken = res.accessToken,
                    refreshToken = res.refreshToken,
                    accessTokenExpiry = expiry,
                    attempts = new List<AttemptData>(res.attempts ?? Array.Empty<AttemptData>()),
                    stats = res.stats,
                    achievements = new List<AchievementData>(res.achievements ?? Array.Empty<AchievementData>())
                });

                if (autoRefreshCoroutine != null) StopCoroutine(autoRefreshCoroutine);
                autoRefreshCoroutine = StartCoroutine(AutoRefreshCoroutine());

                callback(true, null);
            }
        }
    }

    private IEnumerator AutoRefreshCoroutine()
    {
        while (AccountManager.Instance.IsLoggedIn())
        {
            var acc = AccountManager.Instance.CurrentAccount;
            float secondsToExpiry = (float)(acc.accessTokenExpiry - DateTime.UtcNow).TotalSeconds;

            if (secondsToExpiry > 60)
                yield return new WaitForSeconds(secondsToExpiry - 60);

            bool success = false;
            string error = null;
            yield return RefreshToken((s, e) => { success = s; error = e; });

            if (!success)
            {
                Debug.LogWarning("Auto-refresh failed: " + error);
                AccountManager.Instance.ClearAccountData();
                yield break;
            }
        }
    }

    public IEnumerator RefreshToken(Action<bool, string> callback)
    {
        var acc = AccountManager.Instance.CurrentAccount;
        if (acc == null || string.IsNullOrEmpty(acc.refreshToken))
        {
            callback(false, "No refresh token available");
            yield break;
        }

        var reqData = new RefreshRequest { refreshToken = acc.refreshToken };
        string json = JsonUtility.ToJson(reqData);

        using (UnityWebRequest request = new UnityWebRequest($"{BaseUrl}/refresh", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                callback(false, request.error);
            }
            else
            {
                // refresh endpoint only returns accessToken
                var tokenWrapper = JsonUtility.FromJson<AccessTokenOnly>(request.downloadHandler.text);
                acc.accessToken = tokenWrapper.accessToken;
                acc.accessTokenExpiry = DateTime.UtcNow.AddHours(1);
                AccountManager.Instance.SetAccountData(acc);
                callback(true, null);
            }
        }
    }

    [Serializable]
    private class AccessTokenOnly { public string accessToken; }

    public void Logout(Action<bool, string> callback)
    {
        if (autoRefreshCoroutine != null) StopCoroutine(autoRefreshCoroutine);
        StartCoroutine(LogoutCoroutine(callback));
    }

    private IEnumerator LogoutCoroutine(Action<bool, string> callback)
    {
        var acc = AccountManager.Instance.CurrentAccount;
        if (acc == null || string.IsNullOrEmpty(acc.refreshToken))
        {
            callback(false, "No user logged in");
            yield break;
        }

        var reqData = new RefreshRequest { refreshToken = acc.refreshToken };
        string json = JsonUtility.ToJson(reqData);

        using (UnityWebRequest request = new UnityWebRequest($"{BaseUrl}/logout", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                callback(false, request.error);
            }
            else
            {
                AccountManager.Instance.ClearAccountData();
                callback(true, null);
            }
        }
    }
}
