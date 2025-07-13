// SceneLoader.cs
// Attach this script to a GameObject (e.g., a Button) in your Main Menu or other scenes.
// This script provides a method to easily transition to the loading scene and specify the target scene.

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLoader : MonoBehaviour
{
    [Tooltip("The name of the loading scene (e.g., 'LoadingScreen').")]
    public string loadingSceneName = "LoadingScreen"; // Ensure this matches your loading scene's name

    /// <summary>
    /// Call this method from a UI Button's OnClick() event.
    /// It sets the target scene and then loads the loading scene.
    /// </summary>
    public void LoadTargetScene(string targetSceneName)
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("Target Scene Name is not set in the SceneLoader script. Please specify which scene to load.");
            return;
        }

        // Set the static variable in LoadingSceneManager to tell it which scene to load.
        LoadingSceneManager.sceneToLoad = targetSceneName;

        // Load the dedicated loading scene.
        SceneManager.LoadScene(loadingSceneName);
    }
}
