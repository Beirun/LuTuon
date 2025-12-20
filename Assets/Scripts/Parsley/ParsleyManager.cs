using System.Collections.Generic;
using UnityEngine;

public class ParsleyManager : MonoBehaviour
{


    [Header("Parts")]
    public List<GameObject> parts;
    [Header("Chopped")]
    public List<GameObject> chopped;


    [Header("Knife Controller")]
    public KnifeController knifeController;

    [Header("Parent Controller")]
    public ParsleyController controller;

    [Header("Chopped Controller")]
    public ChoppedParsleyController choppedController;
    [Header("Knife Config")]
    public int cuts = 4;
    public List<float> cutsX = new();
    void Start()
    {
        knifeController = FindFirstObjectByType<KnifeController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.isPlaced && knifeController.cutsMade > 0)
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
            if (knifeController.cutsMade == cuts)
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
                choppedController.startPos = controller.startPos;
                choppedController.startRot = Quaternion.Euler(39.209f, 96f, 0f);
                StartCoroutine(choppedController.ReturnToStart());
            }
        }else if (controller.isPlaced)
        {
            knifeController.numberOfCuts = cuts;
            knifeController.cutsX = cutsX;
        }
    }
}