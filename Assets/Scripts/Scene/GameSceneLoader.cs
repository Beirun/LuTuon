using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLoader : MonoBehaviour
{
    [Tooltip("The name of the loading scene (e.g., 'LoadingScreen').")]
    public string loadingSceneName = "LoadingScreen";

    public void LoadTargetScene(string targetSceneName)
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target Scene Name is not set in the SceneLoader script. Please specify which scene to load.");
            return;
        }

        LoadingSceneManager.sceneToLoad = targetSceneName;

        SceneManager.LoadScene(loadingSceneName);
    }
}
