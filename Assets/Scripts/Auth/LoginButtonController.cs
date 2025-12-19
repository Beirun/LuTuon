using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginButtonController : MonoBehaviour
{
    [SerializeField] Button myButton;
    [SerializeField] TMP_Text buttonText;
    [SerializeField] TMP_Text emailText;
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

        UpdateButtonState();

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

    public void Refresh()
    {
        UpdateButtonState();
    }
}
