using System.Collections;
using UnityEngine;

public class PorkManager : MonoBehaviour
{

    [Header("Materials")]
    public Material luspad;
    public GameObject pork;
    public SoySauceController soyController;
    public PorkController porkController;
    public Material porkLuto;
    private bool isChangedToLuto = false;
    private bool isChangedToLuspad = false;

    void Update()
    {
        if (!isChangedToLuto && soyController.isFinished)
        {
            StartCoroutine(ChangeToLutoMaterial());
        }
        if (!isChangedToLuspad && porkController.isFinished)
        {
            StartCoroutine(ChangeToLuspadMaterial());
        }
    }

    IEnumerator ChangeToLutoMaterial()
    {
        yield return new WaitForSeconds(8f);

        var mrs = pork.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < mrs.Length; i++)
        {
            var mr = mrs[i];
            if (mr == null) continue;

            var mats = mr.materials;
            for (int j = 0; j < mats.Length; j++) mats[j] = porkLuto;
            mr.materials = mats;
        }
        isChangedToLuto = true;
    }
    IEnumerator ChangeToLuspadMaterial()
    {
        yield return new WaitForSeconds(8f);
        var mrs = pork.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < mrs.Length; i++)
        {
            var mr = mrs[i];
            if (mr == null) continue;

            var mats = mr.materials;
            for (int j = 0; j < mats.Length; j++) mats[j] = luspad;
            mr.materials = mats;
        }
        isChangedToLuspad = true;
    }
}
