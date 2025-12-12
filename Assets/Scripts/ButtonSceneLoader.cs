using UnityEngine;
using UnityEngine.UI;

public class ButtonSceneLoader : MonoBehaviour
{
    [SerializeField] public string sceneName;
    [SerializeField] public bool withTransition = false;
    private Button targetButton;

    void Start()
    {
        if (targetButton == null)
            targetButton = GetComponent<Button>();

        if (targetButton != null)
        {
            targetButton.onClick.AddListener(() =>
            {
                if (withTransition && SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadSceneWithTransition(sceneName);
                else if (SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadScene(sceneName);
                else
                    Debug.LogWarning("SceneController instance not found.");
            });
        }
    }
}
