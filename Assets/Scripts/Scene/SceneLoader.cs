using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; 

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; } 

    [SerializeField] private Animator fadePanelAnimator; 
    [SerializeField] private Canvas fadePanelCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); 
        }
    }


    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void LoadSceneWithTransition(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        if (fadePanelCanvas != null)
        {
            fadePanelCanvas.sortingOrder = 1;
            fadePanelCanvas.gameObject.SetActive(true);
        }

        if (fadePanelAnimator != null)
        {
            fadePanelAnimator.SetTrigger("FadeOut");
            yield return new WaitForSeconds(1f); 
        }
        else
        {
            Debug.LogWarning("FadePanel Animator not assigned. Loading scene without transition.");
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; 

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        if (fadePanelAnimator != null)
        {
            fadePanelAnimator.SetTrigger("FadeIn");
            yield return new WaitForSeconds(fadePanelAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        if (fadePanelCanvas != null)
        {
            fadePanelCanvas.sortingOrder = 0;
        }
    }

    public void LoadSceneWithTransition(int sceneIndex)
    {
        StartCoroutine(LoadSceneRoutine(sceneIndex));
    }

    private IEnumerator LoadSceneRoutine(int sceneIndex)
    {
        if (fadePanelCanvas != null)
        {
            fadePanelCanvas.sortingOrder = 1;
            fadePanelCanvas.gameObject.SetActive(true);
        }

        if (fadePanelAnimator != null)
        {
            fadePanelAnimator.SetTrigger("FadeOut");
            yield return new WaitForSeconds(1f); 
        }
        else
        {
            Debug.LogWarning("FadePanel Animator not assigned. Loading scene without transition.");
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false; 

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        if (fadePanelAnimator != null)
        {
            fadePanelAnimator.SetTrigger("FadeIn");
            yield return new WaitForSeconds(fadePanelAnimator.GetCurrentAnimatorStateInfo(0).length);
        }

        if (fadePanelCanvas != null)
        {
            fadePanelCanvas.sortingOrder = 0; 
        }
    }
}
