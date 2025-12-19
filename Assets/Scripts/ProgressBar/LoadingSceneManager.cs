using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; 

public class LoadingSceneManager : MonoBehaviour
{
    public static string sceneToLoad;

    [Header("UI Elements")]
    [Tooltip("Assign the UI Slider component here.")]
    public Slider progressBar; 

    [Tooltip("Assign the UI Text (or TextMeshPro Text) component here for progress percentage.")]
    public TextMeshProUGUI progressText; 

    [Tooltip("The minimum time in seconds the loading screen will be displayed, even if loading finishes faster.")]
    public float minimumLoadingTime = 2.0f;

    private float _loadingProgress;
    private bool _isLoadingComplete = false;

    void Start()
    {
        if (progressBar != null)
        {
            progressBar.value = 0;
        }
        if (progressText != null)
        {
            progressText.text = "0%";
        }

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneAsync());
        }
        else
        {
            Debug.LogError("No scene name provided for loading. Please set 'LoadingSceneManager.sceneToLoad' before loading this scene.");
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

            float target = Mathf.Clamp01(op.progress);
            _loadingProgress = Mathf.MoveTowards(_loadingProgress, target, Time.deltaTime);

            if (progressBar) progressBar.value = _loadingProgress;
            if (progressText) progressText.text = Mathf.RoundToInt(_loadingProgress * 100f) + "%";

            if (op.progress >= 0.9f && timer >= minimumLoadingTime)
            {
                _isLoadingComplete = true;
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }

}