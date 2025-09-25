using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StandardButtonBinder : MonoBehaviour
{
    [SerializeField] Button myButton;

    private void Start()
    {
        if (myButton == null)
            myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.AddListener(() =>
            {
                if (MenuCanvasLoader.Instance != null)
                {
                    MenuCanvasLoader.Instance.ShowStandardMenu();
                }
                else
                {
                    Debug.LogWarning("MenuCanvasLoader instance not found.");
                }
            });
        }
        else
        {
            Debug.LogWarning("BackButtonBinder: No Button component found.");
        }
    }
}
