using UnityEngine;

public class MenuCanvasLoader : MonoBehaviour
{
    [SerializeField] GameObject mainMenuCanvas;
    [SerializeField] GameObject tutorialMenuCanvas;
    [SerializeField] GameObject standardMenuCanvas;

    public void ShowMainMenu()
    {
        SetActiveCanvas(mainMenuCanvas);
    }

    public void ShowTutorialMenu()
    {
        SetActiveCanvas(tutorialMenuCanvas);
    }

    public void ShowStandardMenu()
    {
        SetActiveCanvas(standardMenuCanvas);
    }

    void SetActiveCanvas(GameObject target)
    {
        if (!mainMenuCanvas || !tutorialMenuCanvas || !standardMenuCanvas)
        {
            Debug.LogError("MenuCanvasLoader: Assign all canvases in the Inspector.");
            return;
        }

        mainMenuCanvas.SetActive(false);
        tutorialMenuCanvas.SetActive(false);
        standardMenuCanvas.SetActive(false);
        target.SetActive(true);
    }
}
