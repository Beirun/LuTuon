using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

[Serializable]
public class AttemptRequest
{
    public string foodId;
    public int attemptPoint;
    public string attemptDate;    
    public string attemptDuration;
    public string attemptType;
}

public class AttemptManager : MonoBehaviour
{
    const string BaseUrl = "https://api.lutuon.app";
    private string attemptDate;

    void Start()
    {
        attemptDate = DateTime.UtcNow.ToString("o"); 
    }

    public void SendAttempt(string foodId, int point, string type)
    {
        var acc = AccountManager.Instance.CurrentAccount;
        if (acc == null || string.IsNullOrEmpty(acc.accessToken))
        {
            Debug.LogWarning("User not logged in");
            return;
        }

        if (string.IsNullOrEmpty(foodId) || string.IsNullOrEmpty(type))
        {
            Debug.LogError("Invalid args");
            return;
        }

        var req = new AttemptRequest
        {
            foodId = foodId,
            attemptPoint = point,
            attemptDate = attemptDate,         
            attemptDuration = DateTime.UtcNow.ToString("o"),
            attemptType = type
        };

        var json = JsonUtility.ToJson(req);
        StartCoroutine(Post(json, acc.accessToken));
    }

    private IEnumerator Post(string json, string token)
    {
        var body = Encoding.UTF8.GetBytes(json);

        using (var r = new UnityWebRequest($"{BaseUrl}/game/attempts", "POST"))
        {
            r.uploadHandler = new UploadHandlerRaw(body);
            r.downloadHandler = new DownloadHandlerBuffer();
            r.SetRequestHeader("Content-Type", "application/json");
            r.SetRequestHeader("Authorization", "Bearer " + token);
            r.timeout = 10;

            yield return r.SendWebRequest();

            if (r.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Attempt failed: " + r.error);
                yield break;
            }

            Debug.Log("Attempt created: " + r.downloadHandler.text);
        }
    }
}
