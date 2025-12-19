using UnityEngine;
using UnityEngine.UI;

public class SFXPlayer : MonoBehaviour
{
    public AudioClip clickSound;
    public AudioClip blackPepperSound;
    public AudioClip chopSound;
    public AudioClip closePotLidSound;
    public AudioClip openPotLidSound;
    public AudioClip dropToPotSound;
    public AudioClip spoonMixSound;
    public AudioClip vegetableOilSound;
    public AudioClip waterSound;
    public AudioClip whiskEggSound;

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
    public void PlayBlackPepperSound()
    {
        if (AudioManager.Instance != null && blackPepperSound != null)
        {
            AudioManager.Instance.PlaySFX(blackPepperSound);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
    public void PlayChopSound()
    {
        if (AudioManager.Instance != null && chopSound != null)
        {
            AudioManager.Instance.PlaySFX(chopSound);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
    public void PlayClosePotLidSound()
    {
        if (AudioManager.Instance != null && closePotLidSound != null)
        {
            AudioManager.Instance.PlaySFX(closePotLidSound);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
    public void PlayOpenPotLidSound()
    {
        if (AudioManager.Instance != null && openPotLidSound != null)
        {
            AudioManager.Instance.PlaySFX(openPotLidSound);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
    public void PlayDropToPotSound()
    {
        if (AudioManager.Instance != null && dropToPotSound != null)
        {
            AudioManager.Instance.PlaySFX(dropToPotSound);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
    public void PlaySpoonMixSound()
    {
        if (AudioManager.Instance != null && spoonMixSound != null)
        {
            AudioManager.Instance.PlaySFX(spoonMixSound);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
    public void PlayVegeOilSound()
    {
        if (AudioManager.Instance != null && vegetableOilSound != null)
        {
            AudioManager.Instance.PlaySFX(vegetableOilSound);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
    public void PlayWaterSound()
    {
        if (AudioManager.Instance != null && waterSound != null)
        {
            AudioManager.Instance.PlaySFX(waterSound);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
    public void PlayWhiskEggSound()
    {
        if (AudioManager.Instance != null && whiskEggSound != null)
        {
            AudioManager.Instance.PlaySFX(whiskEggSound);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
    public void PlaySound(AudioClip sound)
    {
        if (AudioManager.Instance != null && clickSound != null)
        {
            AudioManager.Instance.PlaySFX(sound);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
    public void StopSound()
    {
        if (AudioManager.Instance != null && clickSound != null)
        {
            AudioManager.Instance.StopSFX();
        }
        else
        {
            Debug.LogWarning("AudioManager instance or clickSound not found!");
        }
    }
}