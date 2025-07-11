using UnityEngine;
using UnityEngine.UI; // For Button component if using UI Button

public class SFXPlayer : MonoBehaviour
{
    public AudioClip clickSound;

    // If this script is on a UI Button's GameObject
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(PlayButtonClickSound);
        }
    }

    public void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null && clickSound != null)
        {
            AudioManager.Instance.PlaySFX(clickSound);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
}