using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCanvasLoader : MonoBehaviour
{
    public static MenuCanvasLoader Instance { get; private set; }

    [SerializeField] GameObject mainMenuCanvas;
    [SerializeField] GameObject tutorialMenuCanvas;
    [SerializeField] GameObject standardMenuCanvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        ResolveCanvases(); 
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        ResolveCanvases(); 
    }

    void ResolveCanvases()
    {
        mainMenuCanvas = GetObject("MainMenuCanvas");
        tutorialMenuCanvas = GetObject("TutorialMenuCanvas");
        standardMenuCanvas = GetObject("StandardMenuCanvas");
    }

    public void ShowMainMenu() => SetActiveCanvas(mainMenuCanvas);
    public void ShowTutorialMenu() => SetActiveCanvas(tutorialMenuCanvas);
    public void ShowStandardMenu() => SetActiveCanvas(standardMenuCanvas);

    void SetActiveCanvas(GameObject target)
    {
        ResolveCanvases(); 
        if (!mainMenuCanvas || !tutorialMenuCanvas || !standardMenuCanvas || !target)
        {
            Debug.LogError("MenuCanvasLoader: Canvases not found in the current scene.");
            return;
        }

        mainMenuCanvas.SetActive(false);
        tutorialMenuCanvas.SetActive(false);
        standardMenuCanvas.SetActive(false);
        target.SetActive(true);
    }

    GameObject GetObject(string name)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            foreach (var root in scene.GetRootGameObjects())
            {
                var obj = FindRecursive(root.transform, name);
                if (obj != null) return obj;
            }
        }
        return null;
    }

    GameObject FindRecursive(Transform t, string name)
    {
        if (t.name == name) return t.gameObject;
        foreach (Transform c in t)
        {
            var r = FindRecursive(c, name);
            if (r != null) return r;
        }
        return null;
    }
}
