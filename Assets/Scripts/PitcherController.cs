using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitcherController : DragController
{
    [Serializable]
    public class WaterOpacityCheck
    {
        public Color color;
        public float multiplier;
    }
    [Header("Water Objects")]
    public GameObject water;
    public GameObject pouringWater;

    public Color pouringColor = new Color(1f, 0.75f, 0f, 0.3f);
    public float targetWaterLevelY = 1.55f;
    public float pouringDuration = 0.5f;
    public float waterDuration = 3f;
    public LidController lid;

    List<Material> pouringMats = new List<Material>();
    List<Color> originalColors = new List<Color>();
    List<Material> mainWaterMats = new List<Material>();

    [Header("Water Color Check")]
    public List<WaterOpacityCheck> waterOpacityChecks = new List<WaterOpacityCheck>();

    public override void Start()
    {
        base.Start();

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

                originalColors.Add(c);
            }
        }

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

        for (int i = 0; i < pouringMats.Count; i++)
        {
            Material m = pouringMats[i];
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", pouringColor);
            else if (m.HasProperty("_Color"))
                m.SetColor("_Color", pouringColor);
        }

        Coroutine cr;
        cr = StartCoroutine(AnimateWaterLevel(targetWaterLevelY, 5f));
        yield return pourManager.ShowVolumePour(5f);
        StopCoroutine(cr);
        pouringWater.SetActive(false);
        yield return ReturnToStart();
        isFinished = true;
        isDisabled = true;
    }

    IEnumerator AnimateWaterLevel(float targetPosY, float duration)
    {
       
        bool isWaterActive = water.activeInHierarchy;
        water.SetActive(true);
        Vector3 waterStartPos = pouringWater.transform.position;
        //pouringWater.transform.position += new Vector3(-0.325f, 0.5f, 0f);

        Vector3 fromPosition = water.transform.position;
        //if (fromPosition.y > targetPosY) targetPosY = fromPosition.y;
        //if (water.transform.position.y < 1f) targetPosY += 0.05f;
        //if (!isWaterActive) targetPosY = 0.8f;
        Vector3 toPosition = new Vector3(fromPosition.x, targetPosY, fromPosition.z);

        float elapsedTime = 0f;

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

        foreach (Material m in pouringMats)
        {
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", pouringColor);
            else if (m.HasProperty("_Color"))
                m.SetColor("_Color", pouringColor);
        }

        while (elapsedTime < duration)
        {
            float rawT = elapsedTime / duration;
            float smoothT = rawT * rawT * (3f - 2f * rawT);

            if (!isWaterActive || targetWaterLevelY == 1.55f) 
                water.transform.position = Vector3.Lerp(fromPosition, toPosition, smoothT);

            float bestScore = 0f;
            float bestMultiplier = 1f;

            for (int i = 0; i < waterOpacityChecks.Count; i++)
            {
                Color refC = waterOpacityChecks[i].color;

                Color curC = startColors[0];

                float dist =
                    Mathf.Abs(curC.r - refC.r) +
                    Mathf.Abs(curC.g - refC.g) +
                    Mathf.Abs(curC.b - refC.b) +
                    Mathf.Abs(curC.a - refC.a);

                float score = 1f - Mathf.Clamp01(dist / 4f);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMultiplier = waterOpacityChecks[i].multiplier;
                }
            }

            float colorT = smoothT * bestMultiplier;
            if (!isWaterActive) colorT = smoothT;

            for (int i = 0; i < mainWaterMats.Count; i++)
            {
                Material m = mainWaterMats[i];
                Color newColor = Color.Lerp(startColors[i], pouringColor, colorT);

                if (m.HasProperty("_BaseColor"))
                    m.SetColor("_BaseColor", newColor);
                else if (m.HasProperty("_Color"))
                    m.SetColor("_Color", newColor);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }


        pouringWater.SetActive(false);
        pouringWater.transform.position = waterStartPos;

        for (int i = 0; i < pouringMats.Count; i++)
        {
            Material m = pouringMats[i];
            Color c = originalColors[i];
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", c);
            else if (m.HasProperty("_Color"))
                m.SetColor("_Color", c);
        }

    }
}
