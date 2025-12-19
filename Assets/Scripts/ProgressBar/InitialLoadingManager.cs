using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; 
using System.Collections;

public class InitialLoadingManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI loadingText; 
    [SerializeField] private Slider progressBar; 

    [Header("Loading Settings")]
    [SerializeField] private string sceneToLoadName = "LoginMenuScene"; 
    [SerializeField] private float minLoadingTime = 2.0f; 

    private AsyncOperation asyncOperation;
    private float loadStartTime;

    void Start()
    {
        loadStartTime = Time.time;
        StartCoroutine(LoadTargetSceneAsync());
    }

    private IEnumerator LoadTargetSceneAsync()
    {
        asyncOperation = SceneManager.LoadSceneAsync(sceneToLoadName);

        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            float actualProgress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            float elapsedTime = Time.time - loadStartTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / minLoadingTime);

            float displayProgress = Mathf.Max(actualProgress, normalizedTime);

            if (progressBar != null)
            {
                progressBar.value = displayProgress;
            }
            if (loadingText != null)
            {
                loadingText.text = (displayProgress * 100).ToString("F0") + "%";
            }

            if (asyncOperation.progress >= 0.9f && elapsedTime >= minLoadingTime)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
