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
        // Start the actual scene loading operation in the background.
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);

        // Prevent the scene from activating immediately after loading.
        // This allows us to control when the scene fully appears.
        operation.allowSceneActivation = false;

        float timer = 0f;

        // Loop while the scene is loading or the minimum loading time has not passed.
        while (!operation.isDone || timer < minimumLoadingTime)
        {
            // Update the timer.
            timer += Time.deltaTime;

            // Calculate the loading progress.
            // The operation.progress goes from 0.0 to 0.9 when allowSceneActivation is false.
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Smoothly interpolate the progress bar value to avoid choppy updates.
            _loadingProgress = Mathf.MoveTowards(_loadingProgress, targetProgress, Time.deltaTime);

            // Update the UI elements.
            if (progressBar != null)
            {
                progressBar.value = _loadingProgress;
            }
            if (progressText != null)
            {
                progressText.text = Mathf.RoundToInt(_loadingProgress * 100f) + "%";
            }

            // If the scene is loaded and the minimum time has passed, allow activation.
            if (_loadingProgress >= 1f && timer >= minimumLoadingTime)
            {
                _isLoadingComplete = true; // Mark as complete
                operation.allowSceneActivation = true; // Allow the scene to fully load
            }

            yield return null; // Wait for the next frame.
        }
    }
}