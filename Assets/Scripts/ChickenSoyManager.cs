using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChickenSoyManager : MonoBehaviour
{
    public GameObject chicken;
    public GameObject water;

    List<Material> mats = new List<Material>();
    Color[] startColors;
    Color target = new Color(0.6132076f, 0.4878468f, 0.4830455f);
    bool started;

    void Start()
    {
        if (!chicken) return;
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
        if (!started && water && water.activeInHierarchy)
        {
            started = true;
            StartCoroutine(DoTransition());
        }
    }

    IEnumerator DoTransition()
    {

        float d = 10f;
        float t = 0f;

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
    }
}
