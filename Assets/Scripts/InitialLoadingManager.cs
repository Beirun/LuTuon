using UnityEngine;
using UnityEngine.UI; // Required for Slider and Text components
using UnityEngine.SceneManagement; // Required for scene management
using TMPro; // Required for TextMeshProUGUI
using System.Collections; // Required for Coroutines

public class InitialLoadingManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI loadingText; // Assign your LoadingText TMP component
    [SerializeField] private Slider progressBar; // Assign your ProgressBar Slider component

    [Header("Loading Settings")]
    [SerializeField] private string sceneToLoadName = "LoginMenuScene"; // The name of the scene to load AFTER this loading screen
    [SerializeField] private float minLoadingTime = 2.0f; // Minimum time the loading screen will be displayed

    private AsyncOperation asyncOperation;
    private float loadStartTime;

    void Start()
    {
        // Start loading the target scene as soon as this LoadingScene is active
        loadStartTime = Time.time;
        StartCoroutine(LoadTargetSceneAsync());
    }

    private IEnumerator LoadTargetSceneAsync()
    {
        // Start loading the scene in the background
        asyncOperation = SceneManager.LoadSceneAsync(sceneToLoadName);

        // Prevent the scene from activating immediately when loaded
        asyncOperation.allowSceneActivation = false;

        // Loop while the scene is not yet fully loaded and activated
        while (!asyncOperation.isDone)
        {
            // Calculate actual loading progress (0.0 to 0.9)
            float actualProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            // Calculate elapsed time for minimum loading duration
            float elapsedTime = Time.time - loadStartTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / minLoadingTime);

            // Combine actual progress with minimum time progress for a smoother bar
            // The progress bar will only advance as fast as the slower of the two
            float displayProgress = Mathf.Max(actualProgress, normalizedTime);

            // Update the UI elements
            if (progressBar != null)
            {
                progressBar.value = displayProgress;
            }
            if (loadingText != null)
            {
                loadingText.text = (displayProgress * 100).ToString("F0") + "%";
            }

            // Check if the scene is loaded enough and minimum time has passed
            if (asyncOperation.progress >= 0.9f && elapsedTime >= minLoadingTime)
            {
                // Allow the scene to activate
                asyncOperation.allowSceneActivation = true;
            }

            yield return null; // Wait for the next frame
        }
    }
}
