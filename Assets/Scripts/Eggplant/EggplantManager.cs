using System.Collections;
using UnityEngine;

public class EggplantManager : MonoBehaviour
{
    public bool isFinished = false;
    bool isModified = false;

    public EggplantController controller;
    public ForkController forkController;
    public GameObject eggplant;

    [Header("Eggplant Texture")]
    public Material[] peeledEggplantMaterials;
    public Material cookedEggPlantMaterial;
    public Material cookedEggPlantWithOnionMaterial;
    public Material cookedEggPlantWithTomatoMaterial;
    public Material cookedEggPlantWithBothMaterial;
    public ChoppedTomatoController tomato;
    public DicedOnionsController onion;
    public MeshRenderer mesh;
    public EggplantTouchManager touchManager;

    public ProgressBarManager progressBar;

    void Update()
    {
        if (controller.isPlaced && !isModified)
        {
            isModified = true;
            controller.enabled = false;

            progressBar.StartProgress(eggplant.transform, 2.5f);
            StartCoroutine(ChangeMaterial());
        }
    }

    IEnumerator ChangeMaterial()
    {
        yield return new WaitForSeconds(2.5f);

        if (onion != null && tomato != null)
        {
            if (tomato.isPlaced)
            {
                if (onion.isInPot) mesh.material = cookedEggPlantWithBothMaterial;
                else mesh.material = cookedEggPlantWithTomatoMaterial;
            }
            else mesh.material = cookedEggPlantWithOnionMaterial;
        }
        else mesh.material = cookedEggPlantMaterial;

        controller.newTargetPos = new Vector3(-2.894f, 0.362f, 0.628f);
        controller.highlightTags.Remove("Grill");
        controller.highlightTags.Add("EggplantPlate");

        controller.startPos = controller.transform.position;
        controller.startRot = controller.transform.rotation;
        controller.isPlaced = false;
        controller.enabled = true;

        isFinished = true;
    }

    public void PauseCooking()
    {
        progressBar.Pause();
    }

    public void ResumeCooking()
    {
        progressBar.Resume();
    }

    public void Peel()
    {
        StartCoroutine(StartPeeling());
    }

    IEnumerator StartPeeling()
    {
        touchManager.isPerforming = true;
        touchManager.HideButton();

        for (int i = 0; i < peeledEggplantMaterials.Length; i++)
        {
            mesh.material = peeledEggplantMaterials[i];
            yield return new WaitForSeconds(0.5f);
        }

        touchManager.isFinished = true;
        touchManager.isPerforming = false;
        eggplant.tag = "PeeledEggplant";
    }
}
