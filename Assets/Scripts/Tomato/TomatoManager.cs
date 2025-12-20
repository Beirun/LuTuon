using System.Collections.Generic;
using UnityEngine;

public class TomatoManager : MonoBehaviour
{


    [Header("Parts")]
    public List<GameObject> parts;
    [Header("Chopped")]
    public List<GameObject> chopped;


    [Header("Knife Controller")]
    public KnifeController knifeController;

    [Header("Parent Controller")]
    public TomatoController controller;

    [Header("Chopped Controller")]
    public ChoppedTomatoController choppedController;

    void Start()
    {
        knifeController = FindFirstObjectByType<KnifeController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.isPlaced)
        {

            Vector3 objectPos = parts[0].transform.position;
            for (int i = 1; i < parts.Count; i++)
            {
                parts[i].transform.position = objectPos;
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
                }
                for (int k = 0; k < chopped.Count; k++)
                {
                    chopped[k].SetActive(true);
                }
                controller.enabled = false;
                knifeController.cutsMade = 0;
                controller.isPlaced = false;
                choppedController.startPos = controller.startPos + new Vector3(-0.12f, 0f, 0f);
                choppedController.startRot = Quaternion.Euler(0f, 83.3f, 0f);
                StartCoroutine(choppedController.ReturnToStart());
            }
        }

    }
}