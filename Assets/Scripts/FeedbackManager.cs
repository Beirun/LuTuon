using System;
using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[Serializable]
public class FeedbackRequest { public string feedbackMessage; }

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] TMP_InputField feedbackInput;
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
        string msg = feedbackInput != null ? feedbackInput.text.Trim() : "";
        if (string.IsNullOrEmpty(msg))
        {
            Debug.LogWarning("Feedback is empty");
            messageManager.ShowMessage("Please enter feedback before submitting");
            return;
        }
        var acc = AccountManager.Instance.CurrentAccount;
        if (acc == null || string.IsNullOrEmpty(acc.accessToken))
        {
            Debug.LogWarning("User not logged in");
            messageManager.ShowMessage("You must be logged in to submit feedback");
            return;
        }
        StartCoroutine(SendFeedbackCoroutine(msg, acc.accessToken));
    }

    private IEnumerator SendFeedbackCoroutine(string message, string token)
    {
        submitButton.onClick.RemoveListener(OnSubmitClicked);

        var reqData = new FeedbackRequest { feedbackMessage = message };
        string json = JsonUtility.ToJson(reqData);

        using (UnityWebRequest request = new UnityWebRequest($"{BaseUrl}/feedbacks", "POST"))
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
                Debug.Log("Feedback sent successfully");
                messageManager.ShowMessage("Feedback sent successfully");
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
