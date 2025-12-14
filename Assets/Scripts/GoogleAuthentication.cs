using Google;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GoogleAuthentication : MonoBehaviour
{
    [Header("Google Configuration")]
    private readonly string webClientId = "911883016735-9or0d1speu4llgcijd2ti0o2pfgb9pe6.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;

    [Header("UI Dependencies")]
    [SerializeField] Button loginButton;
    [SerializeField] LoginButtonController loginButtonController;
    [SerializeField] DialogManager dialogManager;
    [SerializeField] MessageManager messageManager;

    [Header("Backend")]
    [SerializeField] AuthManager authManager;

    void Awake()
    {
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true,
            UseGameSignIn = false,
            RequestEmail = true
        };
    }

    private void Start()
    {
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(OnSignIn);
        }
    }

    private void OnDestroy()
    {
        if (loginButton != null)
        {
            loginButton.onClick.RemoveListener(OnSignIn);
        }
    }

    public void OnSignIn()
    {
        loginButton.onClick.RemoveListener(OnSignIn);

        loginButton.interactable = false;

        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        Debug.Log("Starting Google Sign In...");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
            OnAuthenticationFinished,
            TaskScheduler.FromCurrentSynchronizationContext()
        );
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            loginButton.interactable = true;

            using (IEnumerator<System.Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.LogError("Got Error: " + error.Status + " " + error.Message);
                    messageManager.ShowMessage("Login Failed: " + error.Status);
                }
                else
                {
                    Debug.LogError("Got unexpected exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            loginButton.interactable = true;
            Debug.LogWarning("Google Login Cancelled");
        }
        else
        {
            Debug.Log("Google Token Received. Verifying with Backend...");

            authManager.Google(task.Result.Email, (success, error) =>
            {
                if (success)
                {
                    Debug.Log("Backend Login successful!");

                    dialogManager.CloseDialog("LoginDialog");
                    messageManager.ShowMessage("Login successful");
                    loginButtonController.Refresh();

                }
                else
                {
                    Debug.LogError("Backend Login failed: " + error);

                    loginButton.interactable = true;

                    messageManager.ShowMessage("Email has not been registered yet");

                    OnSignOut();
                }
            });
        }
        loginButton.onClick.AddListener(OnSignIn);

    }

    public void OnSignOut()
    {
        if (GoogleSignIn.DefaultInstance != null)
        {
            GoogleSignIn.DefaultInstance.SignOut();
        }
    }
}