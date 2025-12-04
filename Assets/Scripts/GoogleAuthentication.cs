using Google;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GoogleAuthentication : MonoBehaviour
{
    [Header("Google Configuration")]
    // Make sure this matches your Web Client ID in Google Cloud Console
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
        // Setup Google Configuration
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true, // Required to get the ID Token for backend verification
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

    // IMPORTANT: Always remove listeners to prevent memory leaks when changing scenes
    private void OnDestroy()
    {
        if (loginButton != null)
        {
            loginButton.onClick.RemoveListener(OnSignIn);
        }
    }

    public void OnSignIn()
    {
        // FIX 1: Prevent "Double Click" Crash
        // We disable the button immediately so the user cannot click it twice while the Google window is loading.
        loginButton.interactable = false;

        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        Debug.Log("Starting Google Sign In...");

        // FIX 2: Threading Issue
        // We use TaskScheduler.FromCurrentSynchronizationContext() to ensure the callback runs on the Main Thread.
        // Without this, accessing UI (like messageManager) inside the callback will crash Unity.
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
            OnAuthenticationFinished,
            TaskScheduler.FromCurrentSynchronizationContext()
        );
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            // If it failed, re-enable the button so they can try again
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
            // If they closed the window, re-enable the button so they can try again
            loginButton.interactable = true;
            Debug.LogWarning("Google Login Cancelled");
        }
        else
        {
            // Google Auth Successful
            Debug.Log("Google Token Received. Verifying with Backend...");

            // Pass the email (or preferably the idToken) to your backend
            authManager.Google(task.Result.Email, (success, error) =>
            {
                if (success)
                {
                    Debug.Log("Backend Login successful!");

                    // Close the UI
                    dialogManager.CloseDialog("LoginDialog");
                    messageManager.ShowMessage("Login successful");
                    loginButtonController.Refresh();

                    // Note: We do NOT re-enable the button here, because the login is done.
                }
                else
                {
                    Debug.LogError("Backend Login failed: " + error);

                    // Backend rejected the user, re-enable button to try again
                    loginButton.interactable = true;

                    messageManager.ShowMessage("Email has not been registered yet");

                    // Sign out of Google locally so they can select a different account next time
                    OnSignOut();
                }
            });
        }
    }

    public void OnSignOut()
    {
        if (GoogleSignIn.DefaultInstance != null)
        {
            GoogleSignIn.DefaultInstance.SignOut();
        }
    }
}