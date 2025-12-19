using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookedMeatManager : MonoBehaviour
{
    [Header("Parts")]
    public List<GameObject> parts;
    [Header("Chopped")]
    public List<GameObject> chopped;


    [Header("Knife Controller")]
    public KnifeController knifeController;

    [Header("Parent Controller")]
    public MeatController controller;

    [Header("Chopped Controller")]
    public ChoppedMeatController choppedController;
    bool isSet = false;


    void Update()
    {
        if (!isSet && !controller.isPlaced && controller.highlightTags.Contains("Grill"))
        {
            choppedController.startPos = controller.startPos;
            isSet = true;
        }
        if (controller.isPlaced && controller.highlightTags.Contains("Choppingboard"))
        {

            Vector3 objectPos = parts[0].transform.position;
            Quaternion rot = parts[0].transform.rotation;
            for (int i = 1; i < parts.Count; i++)
            {
                parts[i].transform.position = objectPos;
                parts[i].transform.rotation = rot;
                if (knifeController.cutsMade == i)
                {
                    for (int j = 0; j < parts.Count; j++)
                    {
                        parts[j].SetActive(j == i);
                    }
                    for (int k = 0; k < chopped.Count; k++)
                    {
                        chopped[k].SetActive(k < i);
                    }
                }

            }
            if (knifeController.cutsMade == 4)
            {
                for (int j = 0; j < parts.Count; j++)
                {
                    parts[j].SetActive(false);
                    chopped[j].SetActive(true);
                }
                controller.enabled = false;
                knifeController.cutsMade = 0;
                controller.isPlaced = false;
                StartCoroutine(choppedController.ReturnToStart());
            }
        }
    }
}
