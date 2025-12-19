using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CulturalInfoManager : MonoBehaviour
{
    public TextAsset txt;
    public TMP_Text target;
    public float interval = 60f;
    public RectTransform parent;     
    public float hiddenOffset = -300f;
    public float visibleY = 0f;
    public float speed = 300f;        

    List<string> lines = new List<string>();
    System.Random rng = new System.Random();

    void Start()
    {
        if (txt == null || target == null || parent == null) return;
        LoadLines();
        if (lines.Count == 0) return;

        var p = parent.anchoredPosition;
        p.y = hiddenOffset;
        parent.anchoredPosition = p;

        StartCoroutine(Run());
    }

    void LoadLines()
    {
        try
        {
            var raw = txt.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            lines.AddRange(raw);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    IEnumerator Run()
    {
        while (true)
        {
            var i = rng.Next(lines.Count);
            target.text = lines[i];
            yield return Move(parent.anchoredPosition.y, visibleY);
            yield return new WaitForSeconds(interval);
            yield return Move(parent.anchoredPosition.y, hiddenOffset);
            yield return new WaitForSeconds(1.5f);
        }
    }

    IEnumerator Move(float startY, float endY)
    {
        var t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed / Mathf.Abs(endY - startY);
            var p = parent.anchoredPosition;
            p.y = Mathf.Lerp(startY, endY, t);
            parent.anchoredPosition = p;
            yield return null;
        }
    }
}
