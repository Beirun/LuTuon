using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginButtonController : MonoBehaviour
{
    [SerializeField] Button myButton;
    [SerializeField] TMP_Text buttonText; // assign the Button's Text in the Inspector
    [SerializeField] TMP_Text emailText; // assign the Button's Text in the Inspector
    [SerializeField] DialogManager dialogManager;

    private void Start()
    {
        if (myButton == null)
            myButton = GetComponent<Button>();

        if (myButton == null)
        {
            Debug.LogWarning("LoginButtonController: No Button component found.");
            return;
        }

        // Update button label and listener on start
        UpdateButtonState();

        // Optionally listen for account changes if your AccountManager has an event system
        // Otherwise call UpdateButtonState() manually after login/logout
    }

    private void UpdateButtonState()
    {
        bool loggedIn = AccountManager.Instance != null && AccountManager.Instance.IsLoggedIn();
        if (buttonText != null)
            buttonText.text = loggedIn ? "Play" : "Login";
        if (emailText != null)
        {
            var acc = AccountManager.Instance.CurrentAccount;
            emailText.text = acc != null ? acc.userEmail : "";
            Debug.LogWarning("test");
        }

        myButton.onClick.RemoveAllListeners();

        if (loggedIn)
        {
            myButton.onClick.AddListener(() =>
            {
                SceneLoader.Instance.LoadSceneWithTransition("MainMenuScene");
            });
        }
        else
        {
            myButton.onClick.AddListener(() =>
            {
                dialogManager.OpenDialog("LoginDialog");
            });
        }
    }

    // Call this public method from AccountManager after login/logout to refresh button state
    public void Refresh()
    {
        UpdateButtonState();
    }
}
