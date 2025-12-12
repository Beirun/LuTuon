using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VinegarController : DragController
{
    [Header("Water Objects")]
    public GameObject water;
    public GameObject pouringWater;
    public Color pouringColor = new Color(1f, 0.75f, 0f, 0.3f); // oil-like yellow tint
    List<Material> pouringMats = new List<Material>();
    List<Color> originalColors = new List<Color>();
    List<Material> mainWaterMats = new List<Material>();
    public float targetWaterLevelY = 0.8f;
    public LidController lid;

    public override void Start()
    {
        base.Start();

        if (pouringWater)
        {
            Renderer[] renderers = pouringWater.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
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
        else
        {
            Debug.LogWarning("pouringWater not assigned!");
        }

        if (water)
        {
            Renderer[] waterRenderers = water.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in waterRenderers)
                mainWaterMats.Add(r.material);
        }
        else
        {
            Debug.LogWarning("water not assigned!");
        }
    }

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && (lid == null || !lid.isClose))
        {
            Vector3 targetPos = highlighted.transform.position + new Vector3(-1.7f, 1.475f, 0f);
            Quaternion targetRot = Quaternion.Euler(65f, 90f, 0f);
            StartCoroutine(AnimatePouring(targetPos, targetRot, 0.5f));
        }
        else StartCoroutine(ReturnToStart());
        ClearHighlight();
    }

    IEnumerator AnimatePouring(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        isPerforming = true;
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(fromPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        pouringWater.SetActive(true);
        pouringWater.transform.position = targetPos + new Vector3(1.4f, -1.6f, 0f);

        foreach (Material m in pouringMats)
        {
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", pouringColor);
            else if (m.HasProperty("_Color"))
                m.SetColor("_Color", pouringColor);
        }

        yield return StartCoroutine(AnimateWaterLevel(targetWaterLevelY, 0.75f));
    }
    IEnumerator AnimateWaterLevel(float targetPosY, float duration)
    {
        bool isWaterActive = water.activeInHierarchy;
        water.SetActive(true);
        Vector3 pwStartPos = pouringWater.transform.position;
        pouringWater.transform.position += new Vector3(-0.325f, 0.5f, 0f);

        Vector3 fromPos = water.transform.position;
        if (fromPos.y < 1f) targetPosY += 0.1f;
        Vector3 toPos = new Vector3(fromPos.x, targetPosY, fromPos.z);

        int count = mainWaterMats.Count;
        Color[] startColors = new Color[count];
        for (int i = 0; i < count; i++)
        {
            Material m = mainWaterMats[i];
            if (m.HasProperty("_BaseColor"))
                startColors[i] = m.GetColor("_BaseColor");
            else if (m.HasProperty("_Color"))
                startColors[i] = m.GetColor("_Color");
            else
                startColors[i] = Color.white;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);

            if (!isWaterActive)
            {
                water.transform.position = Vector3.Lerp(fromPos, toPos, smoothT);
            }

            float baseT = smoothT;
            for (int i = 0; i < mainWaterMats.Count; i++)
            {
                Material m = mainWaterMats[i];
                Color start = startColors[i];



                if (water.activeInHierarchy) baseT = smoothT * 0.03f;
                else baseT = smoothT;

                Color c = Color.Lerp(start, pouringColor, baseT);

                if (m.HasProperty("_BaseColor"))
                    m.SetColor("_BaseColor", c);
                else if (m.HasProperty("_Color"))
                    m.SetColor("_Color", c);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        pouringWater.SetActive(false);
        pouringWater.transform.position = pwStartPos;

        for (int i = 0; i < pouringMats.Count; i++)
        {
            Material m = pouringMats[i];
            Color c = originalColors[i];
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", c);
            else if (m.HasProperty("_Color"))
                m.SetColor("_Color", c);
        }

        yield return ReturnToStart();
        isFinished = true;
    }

}
