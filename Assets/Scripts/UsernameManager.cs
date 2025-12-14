using System;
using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[Serializable]
public class UsernameRequest { public string newUsername; }

public class UsernameManager : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] Button submitButton;
    [SerializeField] MessageManager messageManager;
    [SerializeField] DialogManager dialogManager;
    private const string BaseUrl = "https://api.lutuon.app";

    void Awake()
    {
        if (submitButton != null)
            submitButton.onClick.AddListener(OnSubmitClicked);
    }

    void OnSubmitClicked()
    {
        string newUsername = usernameInput != null ? usernameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(newUsername))
        {
            Debug.LogWarning("Feedback is empty");
            messageManager.ShowMessage("Please enter a new username");
            return;
        }
        var acc = AccountManager.Instance.CurrentAccount;
        if (acc == null || string.IsNullOrEmpty(acc.accessToken))
        {
            Debug.LogWarning("User not logged in");
            messageManager.ShowMessage("You must be logged in to submit feedback");
            return;
        }
        StartCoroutine(ChangeUsernameCoroutine(newUsername, acc.accessToken));
    }

    private IEnumerator ChangeUsernameCoroutine(string newUsername, string token)
    {
        submitButton.onClick.RemoveListener(OnSubmitClicked);
        var reqData = new UsernameRequest { newUsername = newUsername };
        string json = JsonUtility.ToJson(reqData);

        using (UnityWebRequest request = new UnityWebRequest($"{BaseUrl}/feedbacks", "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Feedback failed: " + request.error);
                messageManager.ShowMessage("Failed to send feedback. Please try again later");
            }
            else
            {
                messageManager.ShowMessage("Username changed successfully");
                if (dialogManager != null)
                {
                    dialogManager.CloseDialogWithoutOverlay("Feedback");
                    dialogManager.OpenDialog("Settings");
                }
            }
            submitButton.onClick.AddListener(OnSubmitClicked);

        }
    }
}
