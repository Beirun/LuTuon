using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeatManager : MonoBehaviour
{
    public bool isFinished = false;
    bool isModified = false;

    public GameObject eggplant;

    [Header("Eggplant Texture")]
    public Material[] peeledEggplantMaterials;
    public Material cookedEggPlantMaterial;
    public MeshRenderer mesh;
    public EggplantTouchManager touchManager;

    public ProgressBarManager progressBar;

    [Header("Parts")]
    public List<GameObject> parts;
    [Header("Chopped")]
    public List<GameObject> chopped;


    [Header("Knife Controller")]
    public KnifeController knifeController;

    [Header("Parent Controller")]
    public CucumberController controller;

    [Header("Chopped Controller")]
    public ChoppedGreenChiliController choppedController;

    void Start()
    {
        knifeController = FindFirstObjectByType<KnifeController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.isPlaced && !isModified)
        {
            isModified = true;
            controller.enabled = false;

            progressBar.StartProgress(eggplant.transform, 2.5f);
            // StartCoroutine(ChangeMaterial());
        }
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
                if (knifeController.cutsMade == 4)
                {
                    for (int j = 0; j < parts.Count; j++)
                    {
                        parts[j].SetActive(false);
                    }
                    controller.enabled = false;
                    knifeController.cutsMade = 0;
                    controller.isPlaced = false;
                    choppedController.startPos = controller.startPos;
                    StartCoroutine(choppedController.ReturnToStart());
                }
            }
        }

    }
    // IEnumerator ChangeMaterial()
    // {
    //     yield return new WaitForSeconds(2.5f);

    //     mesh.material = cookedEggPlantMaterial;

    //     controller.newTargetPos = new Vector3(-2.894f, 0.362f, 0.628f);
    //     controller.highlightTags.Remove("Grill");
    //     controller.highlightTags.Add("EggplantPlate");

    //     controller.startPos = controller.transform.position;
    //     controller.startRot = controller.transform.rotation;
    //     controller.isPlaced = false;
    //     controller.enabled = true;

    //     isFinished = true;
    // }

    // public void PauseCooking()
    // {
    //     progressBar.Pause();
    // }

    // public void ResumeCooking()
    // {
    //     progressBar.Resume();
    // }

    // public void Peel()
    // {
    //     StartCoroutine(StartPeeling());
    // }

    // IEnumerator StartPeeling()
    // {
    //     touchManager.isPerforming = true;
    //     touchManager.HideButton();

    //     for (int i = 0; i < peeledEggplantMaterials.Length; i++)
    //     {
    //         mesh.material = peeledEggplantMaterials[i];
    //         yield return new WaitForSeconds(0.5f);
    //     }

    //     touchManager.isFinished = true;
    //     touchManager.isPerforming = false;
    //     eggplant.tag = "PeeledEggplant";
    // }
}

