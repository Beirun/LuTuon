using System;
using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;

[Serializable]
public class UsernameRequest { public string newUsername; }

public class UsernameManager : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] Button submitButton;
    [SerializeField] MessageManager messageManager;
    [SerializeField] DialogManager dialogManager;
    [SerializeField] List<TMP_Text> usernameFields;
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
            Debug.LogWarning("Username is empty");
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
        submitButton.interactable = false;
        var reqData = new UsernameRequest { newUsername = newUsername };
        string json = JsonUtility.ToJson(reqData);

        using (UnityWebRequest request = UnityWebRequest.Put($"{BaseUrl}/game/username", json))
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
                string serverMessage = request.downloadHandler.text;

                try
                {
                    var errorData = JsonUtility.FromJson<ServerErrorResponse>(serverMessage);
                    serverMessage = errorData.error;
                }
                catch
                {
                    // fallback to raw text if parsing fails
                }

                Debug.LogError("Username change failed: " + serverMessage);
                messageManager.ShowMessage(serverMessage);
            }
            else
            {
                var acc = AccountManager.Instance.CurrentAccount;
                if (acc != null)
                {
                    AccountManager.Instance.CurrentAccount.userName = newUsername;
                }
                messageManager.ShowMessage("Username updated successfully");
                if (dialogManager != null)
                {
                    dialogManager.CloseAllDialogs();
                }
                foreach(var fields in usernameFields)
                {
                    fields.text = newUsername;
                }
                usernameInput.text = "";
            }
            submitButton.onClick.AddListener(OnSubmitClicked);
            submitButton.interactable = true;
        }
    }
}
