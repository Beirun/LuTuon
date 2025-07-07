using UnityEngine;
using UnityEngine.UI;

public class ButtonSFXPlayer : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlayClickSound);
    }

    void PlayClickSound()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayClick();
    }
}