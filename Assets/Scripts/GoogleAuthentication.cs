using Google;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoogleAuthentication : MonoBehaviour
{
    private GoogleSignInConfiguration configuration;
    private string webClientId = "911883016735-9or0d1speu4llgcijd2ti0o2pfgb9pe6.apps.googleusercontent.com";
    [SerializeField] AuthManager authManager;
    [SerializeField] DialogManager dialogManager;
    [SerializeField] LoginButtonController loginButtonController; // drag in Inspector
    [SerializeField] MessageManager messageManager;
    public Button loginButton;



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

    public void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
            OnAuthenticationFinished, TaskScheduler.Default);



    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<System.Exception> enumerator =
                task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error =
                        (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.LogError("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    Debug.LogError("Got unexpected exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            Debug.LogError("Cancelled");
        }
        else
        {
            authManager.Google(task.Result.Email, (success, error) =>
            {
                if (success)
                {
                    Debug.Log("Google Login successful!");
                    dialogManager.CloseDialog("LoginDialog");
                    messageManager.ShowMessage("Login successful");
                    loginButtonController.Refresh();
                }
                else
                {
                    Debug.LogError("Google Login failed: " + error);
                    messageManager.ShowMessage("Email has not been registered yet");
                    OnSignOut();
                }
            });
        }
    }

    public void OnSignOut()
    {
        GoogleSignIn.DefaultInstance.SignOut();
    }

}