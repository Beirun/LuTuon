using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PitcherController : DragController
{
    [Header("Water Objects")]
    public GameObject water;
    public GameObject pouringWater;

    public Color pouringColor = new Color(1f, 0.75f, 0f, 0.3f);
    public float targetWaterLevelY = 1.55f;
    public float pouringDuration = 0.5f;
    public float waterDuration = 3f;
    public LidController lid;

    List<Material> pouringMats = new List<Material>();
    List<Color> originalPourColors = new List<Color>();
    List<Material> mainWaterMats = new List<Material>();

    public override void Start()
    {
        base.Start();

        // Collect pouring-water materials
        if (pouringWater)
        {
            Renderer[] rens = pouringWater.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in rens)
            {
                Material m = r.material;
                pouringMats.Add(m);

                Color c = m.HasProperty("_BaseColor")
                    ? m.GetColor("_BaseColor")
                    : m.HasProperty("_Color")
                        ? m.GetColor("_Color")
                        : Color.white;

                originalPourColors.Add(c);
            }
        }

        // Collect main-water materials for color blending
        if (water)
        {
            Renderer[] rens = water.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in rens)
                mainWaterMats.Add(r.material);
        }
    }

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && (lid == null || !lid.isClose))
        {
            Vector3 pos = highlighted.transform.position + new Vector3(-2.1f, 1.35f, 0f);
            Quaternion rot = Quaternion.Euler(-25f, 90f, -90f);
            StartCoroutine(AnimatePouring(pos, rot, pouringDuration));
        }
        else StartCoroutine(ReturnToStart());

        ClearHighlight();
    }

    IEnumerator AnimatePouring(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        isPerforming = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float t = 0f;

        while (t < duration)
        {
            float k = t / duration;
            k = k * k * (3f - 2f * k);

            transform.position = Vector3.Lerp(startPos, targetPos, k);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, k);
            t += Time.deltaTime;

            yield return null;
        }

        pouringWater.SetActive(true);
        pouringWater.transform.position = targetPos + new Vector3(1.9f, -1.4f, 0f);

        // Apply pouring color
        for (int i = 0; i < pouringMats.Count; i++)
        {
            Material m = pouringMats[i];
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", pouringColor);
            else if (m.HasProperty("_Color"))
                m.SetColor("_Color", pouringColor);
        }

        yield return StartCoroutine(AnimateWaterLevel(targetWaterLevelY, waterDuration));
    }

    IEnumerator AnimateWaterLevel(float targetPosY, float duration)
    {
        bool wasActive = water.activeInHierarchy;
        water.SetActive(true);

        Vector3 fromPos = water.transform.position;
        Vector3 toPos = new Vector3(fromPos.x, targetPosY, fromPos.z);

        int count = mainWaterMats.Count;
        Color[] startColors = new Color[count];

        for (int i = 0; i < count; i++)
        {
            Material m = mainWaterMats[i];
            startColors[i] = m.HasProperty("_BaseColor")
                ? m.GetColor("_BaseColor")
                : m.HasProperty("_Color")
                    ? m.GetColor("_Color")
                    : Color.white;
        }

        float t = 0f;

        while (t < duration)
        {
            float k = t / duration;
            k = k * k * (3f - 2f * k);

            // Water elevation
            if (!wasActive)
                water.transform.position = Vector3.Lerp(fromPos, toPos, k);

            // Smooth transition based on Vinegar logic
            float colorT = k;

            if (wasActive && targetWaterLevelY < 1f) colorT = k * 0.23f;
            else if (!wasActive && water.transform.position.y < 1f) colorT = k * 0.73f;

            for (int i = 0; i < count; i++)
            {
                Material m = mainWaterMats[i];
                Color c = Color.Lerp(startColors[i], pouringColor, colorT);

                if (m.HasProperty("_BaseColor"))
                    m.SetColor("_BaseColor", c);
                else if (m.HasProperty("_Color"))
                    m.SetColor("_Color", c);
            }

            t += Time.deltaTime;
            yield return null;
        }

        // Reset pouring-water colors
        for (int i = 0; i < pouringMats.Count; i++)
        {
            Material m = pouringMats[i];
            Color c = originalPourColors[i];

            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", c);
            else if (m.HasProperty("_Color"))
                m.SetColor("_Color", c);
        }

        pouringWater.SetActive(false);
        yield return ReturnToStart();
        isFinished = true;
    }
}
