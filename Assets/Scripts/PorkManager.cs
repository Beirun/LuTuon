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
    private bool isLuspadRoutineStarted = false;
    private bool isLutoRoutineStarted = false;
    Coroutine luspadRoutine;

    void Update()
    {
        if (!isChangedToLuto && soyController.hasPoured && !isLutoRoutineStarted)
        {
            if(luspadRoutine != null)
            {
                StopCoroutine(luspadRoutine);
                luspadRoutine = null;
            }
            StartCoroutine(ChangeToLutoMaterial());
            isLutoRoutineStarted = true;
        }
        if (!isChangedToLuspad && porkController.isPlaced && !isChangedToLuto && !isLuspadRoutineStarted)
        {
            luspadRoutine = StartCoroutine(ChangeToLuspadMaterial());
            isLuspadRoutineStarted = true;
        }
    }

    IEnumerator ChangeToLutoMaterial()
    {
        Debug.LogWarning("Changing to Luto Material");
        yield return new WaitForSeconds(3f);

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
        Debug.LogWarning("Changing to Luspad Material");
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
