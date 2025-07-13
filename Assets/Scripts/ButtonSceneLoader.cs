using UnityEngine;
using UnityEngine.UI;

public class ButtonSceneLoader : MonoBehaviour
{
    [SerializeField] public string sceneName; // Set this in the Inspector
    [SerializeField] public bool withTransition = false;
    private Button targetButton;

    void Start()
    {
        // Optional: Automatically get the Button if not assigned
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
