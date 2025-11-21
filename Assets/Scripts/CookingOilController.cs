using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CookingOilController : DragController
{
    [Header("Water Objects")]
    public GameObject water;
    public GameObject pouringWater;
    public Color pouringColor = new Color(1f, 0.75f, 0f, 0.3f); // oil-like yellow tint
    List<Material> pouringMats = new List<Material>();
    List<Color> originalColors = new List<Color>();
    List<Material> mainWaterMats = new List<Material>();
    public LidController lid;

    public override void Start()
    {
        base.Start();

        // Collect pouring water materials
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

        // Collect main water materials (for color change that persists)
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
        if (highlighted != null && !lid.isClose)
        {
            Vector3 targetPos = highlighted.transform.position + new Vector3(-2.1f, 1.35f, 0f);
            Quaternion targetRot = Quaternion.Euler(-25f, 90f, -90f);
            StartCoroutine(AnimatePouring(targetPos, targetRot, 0.5f));
        }
        ClearHighlight();
    }

    IEnumerator AnimatePouring(Vector3 targetPos, Quaternion targetRot, float duration)
    {
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

        

        yield return StartCoroutine(AnimateWaterLevel(0.8f, 0.75f));
    }

    IEnumerator AnimateWaterLevel(float targetPosY, float duration)
    {
        water.SetActive(true);

        Vector3 waterStartPos = pouringWater.transform.position;
        pouringWater.transform.position += new Vector3(-0.325f, 0.5f, 0f);

        Vector3 fromPosition = water.transform.position;
        Vector3 toPosition = new Vector3(fromPosition.x, targetPosY, fromPosition.z);

        float elapsedTime = 0f;

        // Store initial colors for main water
        List<Color> startColors = new List<Color>();
        foreach (Material m in mainWaterMats)
        {
            Color startC = m.HasProperty("_BaseColor")
                ? m.GetColor("_BaseColor")
                : m.HasProperty("_Color")
                    ? m.GetColor("_Color")
                    : Color.white;
            startColors.Add(startC);
        }

        // Change all pouring materials to oil color instantly
        foreach (Material m in pouringMats)
        {
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", pouringColor);
            else if (m.HasProperty("_Color"))
                m.SetColor("_Color", pouringColor);
        }

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            // Lerp position
            if (water.transform.position.y < 0.8f)
                water.transform.position = Vector3.Lerp(fromPosition, toPosition, t);

            // Lerp color transition for main water
            for (int i = 0; i < mainWaterMats.Count; i++)
            {
                Material m = mainWaterMats[i];
                Color newColor = Color.Lerp(startColors[i], pouringColor, t);

                if (m.HasProperty("_BaseColor"))
                    m.SetColor("_BaseColor", newColor);
                else if (m.HasProperty("_Color"))
                    m.SetColor("_Color", newColor);

            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final state
        foreach (Material m in mainWaterMats)
        {
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", pouringColor);
            else if (m.HasProperty("_Color"))
                m.SetColor("_Color", pouringColor);
        }

        pouringWater.SetActive(false);
        pouringWater.transform.position = waterStartPos;

        // Reset pouring water color only
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
