using UnityEngine;

public class SoySauceManager : MonoBehaviour
{
    SoySauceController controller;
    public GameObject panWater;
    public GameObject bowlWater;

    void Start()
    {
        if (controller == null) controller = FindFirstObjectByType<SoySauceController>();
    }

    void Update()
    {
        if (controller == null) return;

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
