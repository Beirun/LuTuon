using Google;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable] public class LoginRequest { public string email; public string password; }
[Serializable] public class GoogleRequest { public string email; }
[Serializable] public class RefreshRequest { public string refreshToken; }
[Serializable] public class AccessTokenOnly { public string accessToken; }

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

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }
    private const string BaseUrl = "https://api.lutuon.app/game";
    private Coroutine autoRefreshCoroutine;
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
    private void Start()
    {
        if (AccountManager.Instance.HasTokens())
        {
            var acc = AccountManager.Instance.CurrentAccount;

            if (DateTime.UtcNow > acc.accessTokenExpiry)
            {
                Debug.Log("Saved token expired. Refreshing...");
                StartCoroutine(RefreshToken((success, error) =>
                {
                    if (success)
                    {
                        StartCoroutine(FetchUserData((dataSuccess, dataError) => {
                            if (!dataSuccess) Debug.LogError("Failed to fetch profile after refresh: " + dataError);
                        }));
                    }
                    else
                    {
                        Debug.LogWarning("Session expired. Logging out.");
                        AccountManager.Instance.ClearAccountData();
                    }
                }));
            }
            else
            {
                Debug.Log("Saved token valid. Fetching user data...");
                StartCoroutine(FetchUserData((success, error) => {
                    if (!success) Debug.LogError("Failed to fetch profile: " + error);
                }));
            }
        }
    }

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

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessSuccessfulLogin(request.downloadHandler.text);
                callback(true, null);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public void Google(string email, Action<bool, string> callback)
    {
        StartCoroutine(GoogleCoroutine(email, callback));
    }

    private IEnumerator GoogleCoroutine(string email, Action<bool, string> callback)
    {
        var reqData = new GoogleRequest { email = email };
        string json = JsonUtility.ToJson(reqData);

        using (UnityWebRequest request = new UnityWebRequest($"{BaseUrl}/google", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessSuccessfulLogin(request.downloadHandler.text);
                callback(true, null);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    public IEnumerator FetchUserData(Action<bool, string> callback)
    {
        string token = AccountManager.Instance.CurrentAccount.accessToken;

        using (UnityWebRequest request = UnityWebRequest.Get($"{BaseUrl}/profile"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessSuccessfulLogin(request.downloadHandler.text);
                callback(true, null);
            }
            else
            {
                if (request.responseCode == 401) AccountManager.Instance.ClearAccountData();
                callback(false, request.error);
            }
        }
    }

    private void ProcessSuccessfulLogin(string jsonResponse)
    {
        var res = JsonUtility.FromJson<LoginResponse>(jsonResponse);

        string accessTok = string.IsNullOrEmpty(res.accessToken) ? AccountManager.Instance.CurrentAccount.accessToken : res.accessToken;
        string refreshTok = string.IsNullOrEmpty(res.refreshToken) ? AccountManager.Instance.CurrentAccount.refreshToken : res.refreshToken;
        DateTime expiry = DateTime.UtcNow.AddHours(1);

        AccountManager.Instance.SetAccountData(new AccountData
        {
            userId = res.user.userId,
            userEmail = res.user.userEmail,
            userName = res.user.userName,
            userDob = res.user.userDob,
            avatarId = res.user.avatarId,
            accessToken = accessTok,
            refreshToken = refreshTok,
            accessTokenExpiry = expiry,
            attempts = new List<AttemptData>(res.attempts ?? Array.Empty<AttemptData>()),
            stats = res.stats,
            achievements = new List<AchievementData>(res.achievements ?? Array.Empty<AchievementData>())
        });

        if (autoRefreshCoroutine != null) StopCoroutine(autoRefreshCoroutine);
        autoRefreshCoroutine = StartCoroutine(AutoRefreshCoroutine());
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

            if (request.result == UnityWebRequest.Result.Success)
            {
                var tokenWrapper = JsonUtility.FromJson<AccessTokenOnly>(request.downloadHandler.text);

                AccountManager.Instance.UpdateTokens(
                    tokenWrapper.accessToken,
                    acc.refreshToken, 
                    DateTime.UtcNow.AddHours(1)
                );

                callback(true, null);
            }
            else
            {
                callback(false, request.error);
            }
        }
    }

    private IEnumerator AutoRefreshCoroutine()
    {
        while (AccountManager.Instance.HasTokens())
        {
            var acc = AccountManager.Instance.CurrentAccount;
            float secondsToExpiry = (float)(acc.accessTokenExpiry - DateTime.UtcNow).TotalSeconds;

            if (secondsToExpiry > 60)
                yield return new WaitForSeconds(secondsToExpiry - 60);

            bool success = false;
            yield return RefreshToken((s, e) => { success = s; });

            if (!success)
            {
                Debug.LogWarning("Auto-refresh failed. Clearing session.");
                AccountManager.Instance.ClearAccountData();
                yield break;
            }
        }
    }

    public void Logout(Action<bool, string> callback)
    {
        if (autoRefreshCoroutine != null) StopCoroutine(autoRefreshCoroutine);
        StartCoroutine(LogoutCoroutine(callback));
    }

    private IEnumerator LogoutCoroutine(Action<bool, string> callback)
    {
        var acc = AccountManager.Instance.CurrentAccount;
        if (acc == null)
        {
            AccountManager.Instance.ClearAccountData();
            callback(true, null);
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

            AccountManager.Instance.ClearAccountData();
            try
            {
                GoogleSignIn.DefaultInstance.SignOut();

            }catch(Exception e)
            {
                Debug.LogWarning("Google SignOut failed: " + e.Message);
            }

            if (request.result == UnityWebRequest.Result.Success)
                callback(true, null);
            else
                callback(false, request.error);
        }
    }
}