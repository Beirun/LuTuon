using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameButton : MonoBehaviour
{
    [SerializeField] Button myButton;
    [SerializeField] TMP_Text buttonText; // assign the Button's Text in the Inspector
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (myButton == null)
            myButton = GetComponent<Button>();

        if (myButton == null)
        {
            Debug.LogWarning("LoginButtonController: No Button component found.");
            return;

        }

        bool loggedIn = AccountManager.Instance != null && AccountManager.Instance.IsLoggedIn();
        if (buttonText != null)
            buttonText.text = loggedIn ? AccountManager.Instance.CurrentAccount.userName : "Player";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
