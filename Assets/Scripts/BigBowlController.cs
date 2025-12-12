using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BigBowlController : DragController
{
    [Header("Water Objects")]
    public GameObject water;
    public GameObject pouringWater;
    public Color pouringColor = new Color(1f, 0.75f, 0f, 0.3f);
    List<Material> pouringMats = new List<Material>();
    List<Color> originalColors = new List<Color>();
    List<Material> mainWaterMats = new List<Material>();
    public float targetWaterLevelY = 0.8f;
    public LidController lid;
    public ChickenController chickenController;

    public override void Start()
    {
        base.Start();

        if (pouringWater)
        {
            Renderer[] rs = pouringWater.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in rs)
            {
                Material m = r.material;
                pouringMats.Add(m);

                Color c = m.HasProperty("_BaseColor")
                    ? m.GetColor("_BaseColor")
                    : m.HasProperty("_Color")
                        ? m.GetColor("_Color")
                        : Color.white;

                originalColors.Add(c);
            }
        }

        if (water)
        {
            Renderer[] wr = water.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in wr) mainWaterMats.Add(r.material);
        }
    }

    public override void Update()
    {
        if (chickenController && !chickenController.isInPot) return;
        base.Update();
    }

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted && (lid == null || !lid.isClose))
        {
            Vector3 pos = highlighted.transform.position + new Vector3(1.3f, 1.8f, -0.8f);
            Quaternion rot = Quaternion.Euler(-165f, 125f, 180f);
            StartCoroutine(AnimatePouring(pos, rot, 0.5f));
        }
        else StartCoroutine(ReturnToStart());
        ClearHighlight();
    }

    IEnumerator AnimatePouring(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        isPerforming = true;
        Vector3 fp = transform.position;
        Quaternion fr = transform.rotation;
        float e = 0f;

        while (e < duration)
        {
            float t = e / duration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(fp, targetPos, t);
            transform.rotation = Quaternion.Slerp(fr, targetRot, t);
            e += Time.deltaTime;
            yield return null;
        }

        pouringWater.SetActive(true);
        pouringWater.transform.position = targetPos + new Vector3(-0.6f, -2.8f, 0.3f);

        foreach (Material m in pouringMats)
        {
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", pouringColor);
            else if (m.HasProperty("_Color")) m.SetColor("_Color", pouringColor);
        }

        yield return StartCoroutine(AnimateWaterLevel(targetWaterLevelY, 0.75f));

    }

    IEnumerator AnimateWaterLevel(float targetY, float duration)
    {
        bool wasActive = water.activeInHierarchy;
        water.SetActive(true);

        Vector3 startPourPos = pouringWater.transform.position;
        pouringWater.transform.position += new Vector3(-0.325f, 0.5f, 0f);

        Vector3 fp = water.transform.position;
        Vector3 tp = new Vector3(fp.x, targetY, fp.z);

        int ct = mainWaterMats.Count;
        Color[] startCols = new Color[ct];
        for (int i = 0; i < ct; i++)
        {
            Material m = mainWaterMats[i];
            if (m.HasProperty("_BaseColor")) startCols[i] = m.GetColor("_BaseColor");
            else if (m.HasProperty("_Color")) startCols[i] = m.GetColor("_Color");
            else startCols[i] = Color.white;
        }

        float e = 0f;
        while (e < duration)
        {
            float t = e / duration;
            float smooth = t * t * (3f - 2f * t);

            if (water.transform.position.y < 1f)
                water.transform.position = Vector3.Lerp(fp, tp, smooth);

            float colorT = smooth;
            if (water.transform.position.y > 1f) colorT = smooth * 0.23f;
            else if (wasActive) colorT = smooth * 0.73f;

            for (int i = 0; i < ct; i++)
            {
                Material m = mainWaterMats[i];
                Color c = Color.Lerp(startCols[i], pouringColor, colorT);

                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
                else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            }

            e += Time.deltaTime;
            yield return null;
        }

        pouringWater.SetActive(false);
        pouringWater.transform.position = startPourPos;
        DisableAllChildren(); 

        for (int i = 0; i < pouringMats.Count; i++)
        {
            Material m = pouringMats[i];
            Color c = originalColors[i];
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            else if (m.HasProperty("_Color")) m.SetColor("_Color", c);
        }

        yield return ReturnToStart();
        isFinished = true;
    }

    void DisableAllChildren()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform c = transform.GetChild(i);
            c.gameObject.SetActive(false);
        }
    }
}
