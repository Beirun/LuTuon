using System.Collections.Generic;
using UnityEngine;

public class SpoonManager : MonoBehaviour
{
    List<DragController> controllers = new List<DragController>();
    SpoonController spoon;

    void Start()
    {
        controllers.AddRange(
            FindObjectsByType<DragController>(FindObjectsSortMode.None)
        );

        spoon = FindFirstObjectByType<SpoonController>();

        if (spoon == null)
        {
            Debug.LogError("SpoonController not found");
            enabled = false;
        }
    }

    void Update()
    {
        bool anyInPot = false;

        for (int i = 0; i < controllers.Count; i++)
        {
            if (controllers[i] != null && controllers[i].isInPot)
            {
                anyInPot = true;
                break;
            }
        }

        spoon.isDisabled = !anyInPot;
    }
}
