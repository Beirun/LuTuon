using UnityEngine;
using UnityEngine.Audio; // Don't forget this!

public class AudioManager : MonoBehaviour
{
    // Singleton instance
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Settings")]
    [SerializeField] private AudioMixer mainMixer; // Drag your MainAudioMixer here
    // String names of the exposed parameters in the Audio Mixer
    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicAudioSource; // Drag your Music AudioSource here
    [SerializeField] private AudioSource sfxAudioSource;   // Drag your SFX AudioSource here

    void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate AudioManager instances
            return; // Stop further execution for this duplicate
        }

        // Assign Audio Sources if not already assigned in Inspector (good practice)
        if (musicAudioSource == null) musicAudioSource = transform.Find("Music AudioSource")?.GetComponent<AudioSource>();
        if (sfxAudioSource == null) sfxAudioSource = transform.Find("SFX AudioSource")?.GetComponent<AudioSource>();

        // Ensure Audio Sources are properly linked to the mixer groups
        if (musicAudioSource != null && musicAudioSource.outputAudioMixerGroup == null)
        {
            musicAudioSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("Music")[0];
        }
        if (sfxAudioSource != null && sfxAudioSource.outputAudioMixerGroup == null)
        {
            sfxAudioSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("SFX")[0];
        }

        LoadVolumeSettings(); // Load saved volumes when the game starts
    }

    // --- Music Control ---
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicAudioSource == null) { Debug.LogWarning("Music AudioSource not assigned!"); return; }
        if (clip == null) { Debug.LogWarning("No music clip provided!"); return; }

        if (musicAudioSource.clip != clip) // Only change if it's a different clip
        {
            musicAudioSource.clip = clip;
            musicAudioSource.loop = loop;
            musicAudioSource.Play();
        }
        else if (!musicAudioSource.isPlaying) // If same clip but paused/stopped
        {
            musicAudioSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.Stop();
        }
    }

    public void SetMusicVolume(float normalizedVolume) // 0 to 1 linear range
    {
        if (mainMixer == null) { Debug.LogWarning("Main Mixer not assigned!"); return; }
        // Convert linear 0-1 to logarithmic dB (-80 to 0)
        float mixerVolume = Mathf.Log10(Mathf.Max(normalizedVolume, 0.0001f)) * 20;
        mainMixer.SetFloat(MUSIC_VOLUME_PARAM, mixerVolume);
        PlayerPrefs.SetFloat("MusicVolume", normalizedVolume); // Save setting
    }

    public float GetMusicVolume()
    {
        if (mainMixer == null) return 0f;
        float mixerVolume;
        mainMixer.GetFloat(MUSIC_VOLUME_PARAM, out mixerVolume);
        return Mathf.Pow(10, mixerVolume / 20); // Convert dB back to linear 0-1
    }

    // --- SFX Control ---
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxAudioSource == null) { Debug.LogWarning("SFX AudioSource not assigned!"); return; }
        if (clip == null) { Debug.LogWarning("No SFX clip provided!"); return; }

        // PlayOneShot is great for SFX as it allows multiple sounds to overlap
        sfxAudioSource.PlayOneShot(clip, volumeScale);
    }

    public void SetSFXVolume(float normalizedVolume) // 0 to 1 linear range
    {
        if (mainMixer == null) { Debug.LogWarning("Main Mixer not assigned!"); return; }
        float mixerVolume = Mathf.Log10(Mathf.Max(normalizedVolume, 0.0001f)) * 20;
        mainMixer.SetFloat(SFX_VOLUME_PARAM, mixerVolume);
        PlayerPrefs.SetFloat("SFXVolume", normalizedVolume); // Save setting
    }

    public float GetSFXVolume()
    {
        if (mainMixer == null) return 0f;
        float mixerVolume;
        mainMixer.GetFloat(SFX_VOLUME_PARAM, out mixerVolume);
        return Mathf.Pow(10, mixerVolume / 20); // Convert dB back to linear 0-1
    }

    // --- Save/Load Settings ---
    private void LoadVolumeSettings()
    {
        // GetFloat will return the second parameter if key is not found
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", GetMusicVolume()); // Use current mixer value as default if no save
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", GetSFXVolume());   // Use current mixer value as default if no save

        SetMusicVolume(savedMusicVolume);
        SetSFXVolume(savedSFXVolume);
    }

    // Optional: Call this from a Quit button or OnApplicationQuit if needed
    void OnApplicationQuit()
    {
        PlayerPrefs.Save(); // Ensure all PlayerPrefs are written to disk
    }
}