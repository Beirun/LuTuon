using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Settings")]
    [SerializeField] private AudioMixer mainMixer;
    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;  

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (musicAudioSource == null) musicAudioSource = transform.Find("Music AudioSource")?.GetComponent<AudioSource>();
        if (sfxAudioSource == null) sfxAudioSource = transform.Find("SFX AudioSource")?.GetComponent<AudioSource>();

        if (musicAudioSource != null && musicAudioSource.outputAudioMixerGroup == null)
        {
            musicAudioSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("Music")[0];
        }
        if (sfxAudioSource != null && sfxAudioSource.outputAudioMixerGroup == null)
        {
            sfxAudioSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("SFX")[0];
        }

        LoadVolumeSettings();
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicAudioSource == null) { Debug.LogWarning("Music AudioSource not assigned!"); return; }
        if (clip == null) { Debug.LogWarning("No music clip provided!"); return; }

        if (musicAudioSource.clip != clip)
        {
            musicAudioSource.clip = clip;
            musicAudioSource.loop = loop;
            musicAudioSource.Play();
        }
        else if (!musicAudioSource.isPlaying)
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

    public void SetMusicVolume(float normalizedVolume)
    {
        if (mainMixer == null) { Debug.LogWarning("Main Mixer not assigned!"); return; }
        float mixerVolume = Mathf.Log10(Mathf.Max(normalizedVolume, 0.0001f)) * 20;
        mainMixer.SetFloat(MUSIC_VOLUME_PARAM, mixerVolume);
        PlayerPrefs.SetFloat("MusicVolume", normalizedVolume); 
    }

    public float GetMusicVolume()
    {
        if (mainMixer == null) return 0f;
        float mixerVolume;
        mainMixer.GetFloat(MUSIC_VOLUME_PARAM, out mixerVolume);
        return Mathf.Pow(10, mixerVolume / 20); 
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxAudioSource == null) { Debug.LogWarning("SFX AudioSource not assigned!"); return; }
        if (clip == null) { Debug.LogWarning("No SFX clip provided!"); return; }

        sfxAudioSource.PlayOneShot(clip, volumeScale);
    }

    public void SetSFXVolume(float normalizedVolume)
    {
        if (mainMixer == null) { Debug.LogWarning("Main Mixer not assigned!"); return; }
        float mixerVolume = Mathf.Log10(Mathf.Max(normalizedVolume, 0.0001f)) * 20;
        mainMixer.SetFloat(SFX_VOLUME_PARAM, mixerVolume);
        PlayerPrefs.SetFloat("SFXVolume", normalizedVolume); 
    }

    public float GetSFXVolume()
    {
        if (mainMixer == null) return 0f;
        float mixerVolume;
        mainMixer.GetFloat(SFX_VOLUME_PARAM, out mixerVolume);
        return Mathf.Pow(10, mixerVolume / 20); 
    }

    private void LoadVolumeSettings()
    {
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", GetMusicVolume()); 
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", GetSFXVolume()); 

        SetMusicVolume(savedMusicVolume);
        SetSFXVolume(savedSFXVolume);
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save(); 
    }
}