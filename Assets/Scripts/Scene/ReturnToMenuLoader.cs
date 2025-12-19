using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReturnToMenuLoader : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        LoadSceneAndShowMenu("MainMenuScene",
            () => MenuCanvasLoader.Instance.ShowMainMenu());
    }

    public void ReturnToTutorialMenu()
    {
        LoadSceneAndShowMenu("MainMenuScene",
            () => MenuCanvasLoader.Instance.ShowTutorialMenu());
    }

    public void ReturnToStandardMenu()
    {
        LoadSceneAndShowMenu("MainMenuScene",
            () => MenuCanvasLoader.Instance.ShowStandardMenu());
    }

    void LoadSceneAndShowMenu(string sceneName, System.Action menuAction)
    {
        if (SceneLoader.Instance == null)
        {
            Debug.LogError("ReturnToMenuLoader: SceneLoader not found.");
            return;
        }

        void Handler(Scene s, LoadSceneMode m)
        {
            SceneManager.sceneLoaded -= Handler;

            if (MenuCanvasLoader.Instance == null)
            {
                Debug.LogError("MenuCanvasLoader not found after scene load.");
                return;
            }

            MenuCanvasLoader.Instance.StartCoroutine(WaitForCanvases(menuAction));
        }

        SceneManager.sceneLoaded += Handler;
        SceneLoader.Instance.LoadSceneWithTransition(sceneName);
    }

    IEnumerator WaitForCanvases(System.Action callback)
    {
        while (MenuCanvasLoader.Instance == null)
        {
            yield return null;
        }
        callback();
    }
}
