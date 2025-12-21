using UnityEngine;

public class SoySauceManager : MonoBehaviour
{
    SoySauceController controller;
    ChickenController chickenController;
    public GameObject panWater;
    public GameObject bowlWater;

    void Start()
    {
        if (controller == null) controller = FindFirstObjectByType<SoySauceController>();
        if (chickenController == null) chickenController = FindFirstObjectByType<ChickenController>();
    }

    void Update()
    {

        if (controller == null) return;
        if(chickenController == null) return;
        if(chickenController.isInPot && !controller.highlightTags.Contains("Pan")) controller.highlightTags.Add("Pan");

        if (controller.highlighted == null) return;
        if(controller.highlighted.tag == "Pan")
        {
            controller.water = panWater;
            controller.targetWaterLevelY = 0.8f;
            controller.InitializedMaterials();
        }
        else if(controller.highlighted.tag == "ChickenBowl")
        {
            controller.water = bowlWater;
            controller.targetWaterLevelY = 0.2f;
            controller.InitializedMaterials();
        }
    }
}
