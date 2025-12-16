using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] TMP_Text buttonText;
    [SerializeField] TMP_Text profileText;
    [SerializeField] TMP_Text avatarText;
    [SerializeField] TMP_Text achievementsText;
    [SerializeField] TMP_Text attemptsText;
    [SerializeField] TMP_Text pointsText;

    void Start()
    {
        UpdatePlayerUI();
    }

    void UpdatePlayerUI()
    {
        var accMgr = AccountManager.Instance;
        bool loggedIn = accMgr != null && accMgr.IsLoggedIn();
        var acc = loggedIn ? accMgr.CurrentAccount : null;

        if (buttonText) buttonText.text = loggedIn ? acc.userName : "Player";
        if (profileText) profileText.text = loggedIn ? acc.userName : "Player";
        if (avatarText) avatarText.text = loggedIn ? acc.userName : "Player";

        if (achievementsText)
        {
            if (loggedIn && acc.achievements != null)
            {
                achievementsText.text = acc.stats.totalAchievements.ToString();
            }
        }

        if (attemptsText)
        {
            int attempts = loggedIn && acc.stats != null ? acc.stats.totalAttempts : 0;
            attemptsText.text = attempts.ToString();
        }

        if (pointsText)
        {
            int points = loggedIn && acc.stats != null ? acc.stats.totalPoints : 0;
            pointsText.text = points.ToString();
        }
    }
}
