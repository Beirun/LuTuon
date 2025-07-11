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

    private const float DISPLAY_VOLUME_MIN = 0f; // Minimum display volume (e.g., 0)
    private const float DISPLAY_VOLUME_MAX = 10f; // Maximum display volume (e.g., 10)

    void Start()
    {
        // Check if AudioManager exists
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager not found! Make sure it's in your initial scene and has DontDestroyOnLoad.");
            return;
        }

        // Initialize Music Slider
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = DISPLAY_VOLUME_MIN;
            musicVolumeSlider.maxValue = DISPLAY_VOLUME_MAX;
            musicVolumeSlider.wholeNumbers = true; // Ensure snapping to whole numbers

            // Get current volume from AudioManager (0-1 range) and convert to display range (0-10)
            musicVolumeSlider.value = ConvertToDisplayVolume(AudioManager.Instance.GetMusicVolume());
            UpdateVolumeText(musicVolumeSlider.value, musicVolumeText);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        // Initialize SFX Slider
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = DISPLAY_VOLUME_MIN;
            sfxVolumeSlider.maxValue = DISPLAY_VOLUME_MAX;
            sfxVolumeSlider.wholeNumbers = true; // Ensure snapping to whole numbers

            // Get current volume from AudioManager (0-1 range) and convert to display range (0-10)
            sfxVolumeSlider.value = ConvertToDisplayVolume(AudioManager.Instance.GetSFXVolume());
            UpdateVolumeText(sfxVolumeSlider.value, sfxVolumeText);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    private void OnMusicSliderChanged(float displayValue) // This value is now 0-10
    {
        // Convert the 0-10 display value back to 0-1 for the AudioManager
        float normalizedValue = ConvertToNormalizedVolume(displayValue);
        AudioManager.Instance.SetMusicVolume(normalizedValue);
        UpdateVolumeText(displayValue, musicVolumeText); // Pass the displayValue directly
    }

    private void OnSFXSliderChanged(float displayValue) // This value is now 0-10
    {
        // Convert the 0-10 display value back to 0-1 for the AudioManager
        float normalizedValue = ConvertToNormalizedVolume(displayValue);
        AudioManager.Instance.SetSFXVolume(normalizedValue);
        UpdateVolumeText(displayValue, sfxVolumeText); // Pass the displayValue directly
    }

    // Helper to update the text display next to the slider
    private void UpdateVolumeText(float displayValue, TextMeshProUGUI volumeText)
    {
        if (volumeText != null)
        {
            // The displayValue is already an integer from 0-10 due to slider.wholeNumbers
            // and the range defined. No further clamping/rounding needed for display.
            volumeText.text = displayValue.ToString();
        }
    }

    // Converts AudioManager's 0-1 normalized volume to the UI's 0-10 display volume
    private float ConvertToDisplayVolume(float normalizedVolume)
    {
        return Mathf.RoundToInt(normalizedVolume * DISPLAY_VOLUME_MAX);
    }

    // Converts UI's 0-10 display volume to AudioManager's 0-1 normalized volume
    private float ConvertToNormalizedVolume(float displayVolume)
    {
        // Ensure we don't divide by zero if DISPLAY_VOLUME_MAX is 0, though it shouldn't be.
        if (DISPLAY_VOLUME_MAX == 0) return 0f;
        return displayVolume / DISPLAY_VOLUME_MAX;
    }
}