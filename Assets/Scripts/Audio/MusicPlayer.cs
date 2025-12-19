using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip sceneMusicClip;

    void Start()
    {
        if (AudioManager.Instance != null && sceneMusicClip != null)
        {
            AudioManager.Instance.PlayMusic(sceneMusicClip);
        }
        else
        {
            Debug.LogWarning("AudioManager instance or sceneMusicClip not found!");
        }
    }
}