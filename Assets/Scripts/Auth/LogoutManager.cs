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
        logoutButton.interactable = false;

        AuthManager.Instance.Logout((success, error) =>
        {
            logoutButton.interactable = true;

            if (success)
            {
                Debug.Log("Logout successful!");
                SceneLoader.Instance.LoadSceneWithTransition("LoginMenuScene");

            }
            else
            {
                Debug.LogWarning("Server logout warning: " + error + ". Local session cleared.");
            }
        });
    }

    
}