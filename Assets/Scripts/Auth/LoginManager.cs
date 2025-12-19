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
    public ImageEntranceAnimator entranceAnimator;
    [SerializeField] MessageManager messageManager;

    [SerializeField] DialogManager dialogManager;
    [SerializeField] LoginButtonController loginButtonController;
    [SerializeField] TMP_Text emailText;

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        authManager = AuthManager.Instance;

        StartCoroutine(CheckAutoLoginStatus());
    }

    private IEnumerator CheckAutoLoginStatus()
    {
        yield return null;

        if (AccountManager.Instance.HasTokens())
        {
            Debug.Log("Tokens found. Waiting for background data fetch...");

            // loadingSpinner.SetActive(true);

            while (!AccountManager.Instance.IsLoggedIn())
            {
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
            // loadingSpinner.SetActive(false);

            dialogManager.CloseDialog("LoginDialog");
            loginButtonController.Refresh();

        }
        entranceAnimator.StartEntranceAnimation();
    }

    private void OnLoginClicked()
    {
        loginButton.onClick.RemoveListener(OnLoginClicked);
        loginButton.interactable = false;
        string email = emailInput.text;
        string password = passwordInput.text;
            
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            messageManager.ShowMessage("Email and Password cannot be empty");
            loginButton.onClick.AddListener(OnLoginClicked);
            loginButton.interactable = true;
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
                messageManager.ShowMessage(error);
                Debug.LogError("Login failed: " + error);
            }
        });
        loginButton.onClick.AddListener(OnLoginClicked);
        loginButton.interactable = true;
    }
}