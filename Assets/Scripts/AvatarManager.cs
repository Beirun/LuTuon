using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

[System.Serializable]
public class UpdateAvatarRequest
{
    public string avatarId;
}
[System.Serializable]
public class ErrorResponse
{
    public string error;
    public string message;
}
public class AvatarManager : MonoBehaviour
{
    [System.Serializable]
    public class AvatarEntry
    {
        public Button btn;
        public string uuid;
        public Image icon;
    }

    public Image playIcon;
    public Image currentAvatarImage;
    public Image currentProfileAvatarImage;
    public Button okButton;
    public List<AvatarEntry> entries = new List<AvatarEntry>();
    public string apiBaseUrl = "https://api.lutuon.app";

    int pendingIndex = -1;

    void Start()
    {
        if (entries == null || entries.Count == 0)
            return;

        for (int i = 0; i < entries.Count; i++)
        {
            int idx = i;
            var el = entries[idx];
            if (el.btn != null)
                el.btn.onClick.AddListener(() => SelectAvatar(idx));
        }

        if (okButton != null)
            okButton.onClick.AddListener(ConfirmAvatar);

        InitCurrentAvatar();
    }

    void InitCurrentAvatar()
    {
        var accMgr = AccountManager.Instance;

        if (accMgr == null || !accMgr.IsLoggedIn())
        {
            var el = entries[0];
            if (currentAvatarImage != null && currentProfileAvatarImage != null && el.icon != null)
            {
                currentAvatarImage.sprite = el.icon.sprite;
                currentProfileAvatarImage.sprite = el.icon.sprite;
            }
            return;
        }

        var acc = accMgr.CurrentAccount;
        if (acc == null || string.IsNullOrEmpty(acc.avatarId) || currentAvatarImage == null || currentProfileAvatarImage == null)
            return;

        for (int i = 0; i < entries.Count; i++)
        {
            var el = entries[i];
            if (el.uuid == acc.avatarId && el.icon != null)
            {
                currentAvatarImage.sprite = el.icon.sprite;
                currentProfileAvatarImage.sprite = el.icon.sprite;
                return;
            }
        }
    }

    void SelectAvatar(int i)
    {
        if (i < 0 || i >= entries.Count)
            return;

        var el = entries[i];
        if (el.icon == null || playIcon == null)
            return;

        playIcon.sprite = el.icon.sprite;
        pendingIndex = i;
    }

    void ConfirmAvatar()
    {
        var accMgr = AccountManager.Instance;
        if (pendingIndex < 0 || pendingIndex >= entries.Count)
            return;

        var el = entries[pendingIndex];
        if (el.icon == null || currentAvatarImage == null || currentProfileAvatarImage == null)
            return;

        currentAvatarImage.sprite = el.icon.sprite;
        currentProfileAvatarImage.sprite = el.icon.sprite;

        if (accMgr == null || !accMgr.IsLoggedIn())
            return;

        accMgr.CurrentAccount.avatarId = el.uuid;

        // Send PUT request to update avatar on server
        StartCoroutine(UpdateAvatarOnServer(el.uuid, accMgr.CurrentAccount.accessToken));
    }

    

    private IEnumerator UpdateAvatarOnServer(string avatarId, string token)
    {
        Debug.Log($"avatarId : {avatarId}");
        var reqData = new UpdateAvatarRequest { avatarId = avatarId };
        string json = JsonUtility.ToJson(reqData);

        using (UnityWebRequest request = UnityWebRequest.Put($"{apiBaseUrl}/game/avatar", json))
        {
            request.method = "PUT";
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            string responseText = request.downloadHandler.text;

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Avatar update failed: " + request.error);

                if (!string.IsNullOrEmpty(responseText))
                {
                    ErrorResponse err = JsonUtility.FromJson<ErrorResponse>(responseText);
                    if (err != null && !string.IsNullOrEmpty(err.error))
                        Debug.LogError("Server error: " + err.error);
                    else
                        Debug.LogError("Server response: " + responseText);
                }
            }
            else
            {
                Debug.Log("Avatar updated successfully on server");
            }
        }
    }

}
