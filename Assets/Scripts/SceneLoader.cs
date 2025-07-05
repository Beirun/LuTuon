using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Required for Coroutines

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; } // Singleton pattern

    [SerializeField] private Animator fadePanelAnimator; // Assign your FadePanel's Animator here
    [SerializeField] private Canvas fadePanelCanvas; // Assign the Canvas component of your FadePanel here

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Ensures this GameObject (and its children like FadePanel) persists across scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    // Public function to start the scene transition by scene name
    public void LoadSceneWithTransition(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // Ensure the fade panel canvas is in front before starting fade out
        if (fadePanelCanvas != null)
        {
            fadePanelCanvas.sortingOrder = 1; // Set to a high order to be on top
            // Optionally, ensure the canvas is active if it might be inactive
            fadePanelCanvas.gameObject.SetActive(true);
        }

        // 1. Trigger Fade Out animation
        if (fadePanelAnimator != null)
        {
            fadePanelAnimator.SetTrigger("FadeOut");
            // Wait for the fade out animation to complete.
            // You might want to get the actual length of the "FadeOut" animation clip
            // for more precise timing, but 1 second is a common placeholder.
            yield return new WaitForSeconds(1f); // Wait for the fade out to complete
        }
        else
        {
            Debug.LogWarning("FadePanel Animator not assigned. Loading scene without transition.");
        }

        // 2. Load the new scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // Prevent scene from activating until fade out is done

        // Wait until the new scene is fully loaded but not yet activated
        while (!asyncLoad.isDone)
        {
            // You can use asyncLoad.progress to show a loading bar if desired
            if (asyncLoad.progress >= 0.9f) // Scene is loaded, but not yet activated
            {
                // Allow scene activation only after the fade out is complete
                // and you're ready to show the new scene
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // 3. Trigger Fade In animation for the new scene
        if (fadePanelAnimator != null)
        {
            fadePanelAnimator.SetTrigger("FadeIn");
            // Wait for the fade in animation to complete
            // GetCurrentAnimatorStateInfo(0).length gives the length of the current state,
            // which should be your "FadeIn" animation if it's the next state.
            yield return new WaitForSeconds(fadePanelAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        // After the entire transition is complete, set the canvas sorting order back to default
        if (fadePanelCanvas != null)
        {
            fadePanelCanvas.sortingOrder = 0; // Set back to default or a lower order
            // Optionally, deactivate the canvas if it's only meant for transitions
            // fadePanelCanvas.gameObject.SetActive(false);
        }
    }

    // Public function to start the scene transition by scene index
    public void LoadSceneWithTransition(int sceneIndex)
    {
        StartCoroutine(LoadSceneRoutine(sceneIndex));
    }

    private IEnumerator LoadSceneRoutine(int sceneIndex)
    {
        // Ensure the fade panel canvas is in front before starting fade out
        if (fadePanelCanvas != null)
        {
            fadePanelCanvas.sortingOrder = 1; // Set to a high order to be on top
            fadePanelCanvas.gameObject.SetActive(true);
        }

        // 1. Trigger Fade Out animation
        if (fadePanelAnimator != null)
        {
            fadePanelAnimator.SetTrigger("FadeOut");
            yield return new WaitForSeconds(1f); // Wait for the fade out to complete
        }
        else
        {
            Debug.LogWarning("FadePanel Animator not assigned. Loading scene without transition.");
        }

        // 2. Load the new scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false; // Prevent scene from activating until fade out is done

        // Wait until the new scene is fully loaded but not yet activated
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // 3. Trigger Fade In animation for the new scene
        if (fadePanelAnimator != null)
        {
            fadePanelAnimator.SetTrigger("FadeIn");
            yield return new WaitForSeconds(fadePanelAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        // After the entire transition is complete, set the canvas sorting order back to default
        if (fadePanelCanvas != null)
        {
            fadePanelCanvas.sortingOrder = 0; // Set back to default or a lower order
            // fadePanelCanvas.gameObject.SetActive(false);
        }
    }
}
