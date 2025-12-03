using UnityEngine;
using UnityEngine.UI;

public class LogoutManager : MonoBehaviour
{
    private Button logoutButton;


    private void Start()
    {
        if(logoutButton == null)
            logoutButton = GetComponent<Button>();

        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogoutClicked);
        }
    }

    private void OnLogoutClicked()
    {
        // 1. Disable button to prevent spamming while request is processing
        logoutButton.interactable = false;

        // 2. Call the Singleton AuthManager
        AuthManager.Instance.Logout((success, error) =>
        {
            // Re-enable button (just in case)
            logoutButton.interactable = true;

            if (success)
            {
                Debug.Log("Logout successful!");
                SceneLoader.Instance.LoadSceneWithTransition("LoginMenuScene");

            }
            else
            {
                // Note: AuthManager clears local data even if the server request fails.
                // So we usually treat "fail" as "success" for the UI (the user is logged out locally).
                Debug.LogWarning("Server logout warning: " + error + ". Local session cleared.");
            }
        });
    }

    
}