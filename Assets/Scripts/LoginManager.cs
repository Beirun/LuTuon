using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;

    public AuthManager authManager;

    [SerializeField] DialogManager dialogManager;
    [SerializeField] LoginButtonController loginButtonController; // drag in Inspector

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
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
                Debug.Log("Login successful!");
                dialogManager.CloseDialog("LoginDialog");
                loginButtonController.Refresh();
            }
            else
                Debug.LogError("Login failed: " + error);
        });
    }
}