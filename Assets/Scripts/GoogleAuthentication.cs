using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Google;
using System.Threading.Tasks;
using UnityEngine.Networking;

public class GoogleAuthentication : MonoBehaviour
{
    private GoogleSignInConfiguration configuration;
    public string webClientId = "911883016735-e40ase5r6ufitng11ku8uofoagg01032.apps.googleusercontent.com";
    [SerializeField] AuthManager authManager;
    [SerializeField] DialogManager dialogManager;
    [SerializeField] LoginButtonController loginButtonController; // drag in Inspector



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
                    loginButtonController.Refresh();
                }
                else
                    Debug.LogError("Google Login failed: " + error);
            });
        }
    }

    public void OnSignOut()
    {
        GoogleSignIn.DefaultInstance.SignOut();
    }

}