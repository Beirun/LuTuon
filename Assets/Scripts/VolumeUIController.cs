using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeUIController : MonoBehaviour
{
    [Header("Music Volume UI")]
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;

    [Header("SFX Volume UI")]
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI sfxVolumeText;

    private const float DISPLAY_VOLUME_MIN = 0f; 
    private const float DISPLAY_VOLUME_MAX = 10f;

    void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager not found! Make sure it's in your initial scene and has DontDestroyOnLoad.");
            return;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = DISPLAY_VOLUME_MIN;
            musicVolumeSlider.maxValue = DISPLAY_VOLUME_MAX;
            musicVolumeSlider.wholeNumbers = true; 

            musicVolumeSlider.value = ConvertToDisplayVolume(AudioManager.Instance.GetMusicVolume());
            UpdateVolumeText(musicVolumeSlider.value, musicVolumeText);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = DISPLAY_VOLUME_MIN;
            sfxVolumeSlider.maxValue = DISPLAY_VOLUME_MAX;
            sfxVolumeSlider.wholeNumbers = true;

            sfxVolumeSlider.value = ConvertToDisplayVolume(AudioManager.Instance.GetSFXVolume());
            UpdateVolumeText(sfxVolumeSlider.value, sfxVolumeText);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    private void OnMusicSliderChanged(float displayValue) 
    {
        float normalizedValue = ConvertToNormalizedVolume(displayValue);
        AudioManager.Instance.SetMusicVolume(normalizedValue);
        UpdateVolumeText(displayValue, musicVolumeText); 
    }

    private void OnSFXSliderChanged(float displayValue) 
    {
        float normalizedValue = ConvertToNormalizedVolume(displayValue);
        AudioManager.Instance.SetSFXVolume(normalizedValue);
        UpdateVolumeText(displayValue, sfxVolumeText); 
    }

    private void UpdateVolumeText(float displayValue, TextMeshProUGUI volumeText)
    {
        if (volumeText != null)
        {
            volumeText.text = displayValue.ToString();
        }
    }

    private float ConvertToDisplayVolume(float normalizedVolume)
    {
        return Mathf.RoundToInt(normalizedVolume * DISPLAY_VOLUME_MAX);
    }

    private float ConvertToNormalizedVolume(float displayVolume)
    {
        if (DISPLAY_VOLUME_MAX == 0) return 0f;
        return displayVolume / DISPLAY_VOLUME_MAX;
    }
}