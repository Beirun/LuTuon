using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;

    public AuthManager authManager;
    [SerializeField] MessageManager messageManager;

    [SerializeField] DialogManager dialogManager;
    [SerializeField] LoginButtonController loginButtonController;
    [SerializeField] TMP_Text emailText;

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);

        // Check if we are auto-logging in via saved tokens
        StartCoroutine(CheckAutoLoginStatus());
    }

    private IEnumerator CheckAutoLoginStatus()
    {
        // Wait a frame to ensure AccountManager has loaded tokens from disk
        yield return null;

        if (AccountManager.Instance.HasTokens())
        {
            Debug.Log("Tokens found. Waiting for background data fetch...");

            // You could show a "Loading..." spinner here if you have one
            // loadingSpinner.SetActive(true);

            // Wait until the user data (userId) is actually populated
            // This happens after AuthManager.Start() -> FetchUserData() finishes
            while (!AccountManager.Instance.IsLoggedIn())
            {
                // If the fetch failed (e.g. token was bad and cleared), stop waiting
                if (!AccountManager.Instance.HasTokens())
                {
                    Debug.Log("Auto-login failed (tokens invalid). Showing Login UI.");
                    // loadingSpinner.SetActive(false);
                    yield break;
                }
                yield return null;
            }

            if (emailText != null)
            {
                    var acc = AccountManager.Instance.CurrentAccount;
                    emailText.text = acc != null ? acc.userEmail : "";
                    Debug.LogWarning("test");
            }
            Debug.Log("Auto-login complete. Hiding Login UI.");
            // loadingSpinner.SetActive(false);

            // Dismiss the Login Dialog automatically
            dialogManager.CloseDialog("LoginDialog");
            loginButtonController.Refresh();
        }
    }

    private void OnLoginClicked()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Email and Password cannot be empty");
            return;
        }

        authManager.Login(email, password, (success, error) =>
        {
            if (success)
            {
                messageManager.ShowMessage("Login successful");
                Debug.Log("Login successful!");
                dialogManager.CloseDialog("LoginDialog");
                loginButtonController.Refresh();
            }
            else
            {
                messageManager.ShowMessage("Incorrect Email or Password");
                Debug.LogError("Login failed: " + error);
            }
        });
    }
}