using UnityEngine;
using UnityEngine.UI;

public class ButtonSceneLoader : MonoBehaviour
{
    [SerializeField] public string sceneName; // Set this in the Inspector
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
                if (SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadScene(sceneName);
                else
                    Debug.LogWarning("SceneController instance not found.");
            });
        }
    }
}
