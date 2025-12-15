using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChickenSoyManager : MonoBehaviour
{
    public GameObject chicken;
    public GameObject water;
    public GameObject panWater;

    List<Material> mats = new List<Material>();
    Color[] startColors;
    
    Color target = new Color(0.6132076f, 0.4878468f, 0.4830455f);
    bool started;
    bool isFinished = false;
    ChickenController chickenController;
    ChoppedGarlicController garlicController;
    Coroutine chickenroutine;
    float t = 0f;

    void Start()
    {
        if (!chicken) return;
        if (chickenController == null) chickenController = FindFirstObjectByType<ChickenController>();
        if (chickenController != null) chickenController.isDisabled = true;
        if(garlicController == null) garlicController = FindFirstObjectByType<ChoppedGarlicController>(FindObjectsInactive.Include);
        InitializeMaterials();
    }

    public void InitializeMaterials()
    {
        mats.Clear();
        Renderer[] r = chicken.GetComponentsInChildren<Renderer>(true);
        foreach (var e in r)
        {
            Material m = e.material;
            mats.Add(m);
        }

        startColors = new Color[mats.Count];
        for (int i = 0; i < mats.Count; i++)
        {
            Material m = mats[i];
            if (m.HasProperty("_BaseColor"))
                startColors[i] = m.GetColor("_BaseColor");
            else if (m.HasProperty("_Color"))
                startColors[i] = m.GetColor("_Color");
            else
                startColors[i] = Color.white;
        }
    }

    void Update()
    {
        if (!started && ((water && water.activeInHierarchy) || (panWater && panWater.activeInHierarchy)))
        {
            started = true;

            chickenroutine = StartCoroutine(DoTransition());
        }
        if(started && !isFinished && chickenroutine == null)
        {
            InitializeMaterials();
            chickenroutine = StartCoroutine(DoTransition());
        }
        if (chickenController == null) return;

        if ((chickenController.isDragging || chickenController.isPerforming) && chickenroutine != null)
        {
            StopCoroutine(chickenroutine);
            chickenroutine = null;
        }
        if(garlicController.isPlaced && chickenController.isDisabled) { chickenController.isDisabled = false; }
    }

    IEnumerator DoTransition()
    {

        float d = 30f;

        while (t < d)
        {
            float k = t / d;
            for (int i = 0; i < mats.Count; i++)
            {
                Material m = mats[i];
                Color c = Color.Lerp(startColors[i], target, k);

                if (m.HasProperty("_BaseColor"))
                    m.SetColor("_BaseColor", c);
                else if (m.HasProperty("_Color"))
                    m.SetColor("_Color", c);
            }

            t += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < mats.Count; i++)
        {
            Material m = mats[i];
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", target);
            else if (m.HasProperty("_Color"))
                m.SetColor("_Color", target);
        }
        isFinished = true;
        chickenController.isDisabled = false;
    }
}
