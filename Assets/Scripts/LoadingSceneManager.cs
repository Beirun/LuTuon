// LoadingSceneManager.cs
// Attach this script to an empty GameObject in your dedicated Loading Scene.
// Make sure to assign the Slider and TextMeshProUGUI components in the Inspector.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // Required if you are using TextMeshPro. If not, use 'UnityEngine.UI.Text' instead.

public class LoadingSceneManager : MonoBehaviour
{
    // Public static string to hold the name of the scene to be loaded.
    // This allows other scripts to set the target scene before loading the Loading Scene.
    public static string sceneToLoad;

    [Header("UI Elements")]
    [Tooltip("Assign the UI Slider component here.")]
    public Slider progressBar; // Reference to the UI Slider.

    [Tooltip("Assign the UI Text (or TextMeshPro Text) component here for progress percentage.")]
    public TextMeshProUGUI progressText; // Reference to the UI Text for percentage. Use 'Text' if not using TextMeshPro.

    [Tooltip("The minimum time in seconds the loading screen will be displayed, even if loading finishes faster.")]
    public float minimumLoadingTime = 2.0f; // Optional: Minimum time to display the loading screen

    private float _loadingProgress;
    private bool _isLoadingComplete = false;

    void Start()
    {
        // Ensure the progress bar and text are initialized correctly.
        if (progressBar != null)
        {
            progressBar.value = 0;
        }
        if (progressText != null)
        {
            progressText.text = "0%";
        }

        // Start the asynchronous loading operation.
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneAsync());
        }
        else
        {
            Debug.LogError("No scene name provided for loading. Please set 'LoadingSceneManager.sceneToLoad' before loading this scene.");
            // Optionally, load a default scene or show an error message to the user.
            // For example: SceneManager.LoadScene("MainMenu");
        }
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;
        float timer = 0f;

        while (!op.isDone)
        {
            timer += Time.deltaTime;

            // Use raw progress (0–0.9) and map to 0–100
            float target = Mathf.Clamp01(op.progress);
            _loadingProgress = Mathf.MoveTowards(_loadingProgress, target, Time.deltaTime);

            if (progressBar) progressBar.value = _loadingProgress;
            if (progressText) progressText.text = Mathf.RoundToInt(_loadingProgress * 100f) + "%";

            // Allow activation only after min time and full load (progress >= 0.9)
            if (op.progress >= 0.9f && timer >= minimumLoadingTime)
            {
                _isLoadingComplete = true;
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }

}