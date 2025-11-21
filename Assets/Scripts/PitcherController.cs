using UnityEngine;
using System.Collections;

public class PitcherController : DragController
{
    [Header("Water Objects")]
    public GameObject water;
    public GameObject pouringWater;

    Renderer[] waterRenderers;
    Color[] startColors;
    Color targetColor = Color.black;
    Color targetSpecColor = new Color(0.5f, 0.5f, 0.5f); // #A4A4A4
    public LidController lid;

    public override void Start()
    {
        base.Start();
        if (water)
        {
            waterRenderers = water.GetComponentsInChildren<Renderer>(true);
            startColors = new Color[waterRenderers.Length];
            for (int i = 0; i < waterRenderers.Length; i++)
            {
                Material m = waterRenderers[i].material;
                startColors[i] = m.HasProperty("_BaseColor")
                    ? m.GetColor("_BaseColor")
                    : m.HasProperty("_Color")
                        ? m.GetColor("_Color")
                        : Color.white;
            }
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
        yield return StartCoroutine(AnimateWaterLevel(1.55f, 3f));
    }

    IEnumerator AnimateWaterLevel(float targetPosY, float duration)
    {
        water.SetActive(true);
        Vector3 fromPosition = water.transform.position;
        Vector3 toPosition = new Vector3(fromPosition.x, targetPosY, fromPosition.z);

        // Capture current starting colors at the start of animation (so transitions are smooth even if reused)
        Color[] currentColors = new Color[waterRenderers.Length];
        Color[] currentSpecColors = new Color[waterRenderers.Length];
        for (int i = 0; i < waterRenderers.Length; i++)
        {
            Material m = waterRenderers[i].material;
            currentColors[i] = m.HasProperty("_BaseColor")
                ? m.GetColor("_BaseColor")
                : m.HasProperty("_Color")
                    ? m.GetColor("_Color")
                    : Color.white;
            currentSpecColors[i] = m.HasProperty("_SpecColor")
                ? m.GetColor("_SpecColor")
                : Color.white;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);
            water.transform.position = Vector3.Lerp(fromPosition, toPosition, t);

            // Smoothly blend diffuse and specular colors
            for (int i = 0; i < waterRenderers.Length; i++)
            {
                Material m = waterRenderers[i].material;
                Color newColor = Color.Lerp(currentColors[i], targetColor, t);
                Color newSpecColor = Color.Lerp(currentSpecColors[i], targetSpecColor, t);

                if (m.HasProperty("_BaseColor"))
                    m.SetColor("_BaseColor", newColor);
                else if (m.HasProperty("_Color"))
                    m.SetColor("_Color", newColor);

                if (m.HasProperty("_SpecColor"))
                    m.SetColor("_SpecColor", newSpecColor);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Final ensure
        for (int i = 0; i < waterRenderers.Length; i++)
        {
            Material m = waterRenderers[i].material;
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", targetColor);
            else if (m.HasProperty("_Color"))
                m.SetColor("_Color", targetColor);
            if (m.HasProperty("_SpecColor"))
                m.SetColor("_SpecColor", targetSpecColor);
        }

        pouringWater.SetActive(false);
        yield return ReturnToStart();
        isFinished = true;
    }
}
