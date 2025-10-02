using System;
using UnityEngine;

#if UNITY_ANDROID || UNITY_IOS
public class HighlightController : MonoBehaviour
{
    [Header("Highlight Settings")]
    public string hightlightTag = "Choppingboard";
    public string outlineLayerName = "Outline";
    public float highlightDistance = 0.12f;
    public float sphereCastRadius = 0.15f;  
    public float originForwardOffset = 0.12f; 
    public float originUpOffset = 0.12f;     

    private int outlineLayer;
    private int previousLayer;
    [HideInInspector]
    public GameObject highlighted;

    void Awake()
    {
        outlineLayer = LayerMask.NameToLayer(outlineLayerName);
        if (outlineLayer < 0)
            Debug.LogError($"Layer '{outlineLayerName}' not found. Add it in Project Settings > Tags and Layers.");
    }

 
    public void HighlightBehind(Vector3 position, Camera cam)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) { ClearHighlight(); return; }

        Vector3 dir = cam.transform.forward.normalized; // direction away from camera
        Vector3 origin = position + dir * originForwardOffset + Vector3.up * originUpOffset;

        // 1) SphereCastAll to catch targets even if floating / slightly offset
        RaycastHit[] hits = Physics.SphereCastAll(origin, sphereCastRadius, dir, highlightDistance);
        if (hits != null && hits.Length > 0)
        {
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var h in hits)
            {
                var obj = h.collider.gameObject;

                // skip self and children
                if (obj == this.gameObject || obj.transform.IsChildOf(transform)) continue;
                if (!obj.CompareTag(hightlightTag)) continue;

                if (obj != highlighted) { ClearHighlight(); SetHighlight(obj); }
                return;
            }
        }

        // 2) Fallback: raycast with a downward bias to hit ground-level objects behind the dragged one
        Vector3 biasDir = (dir + Vector3.down * 0.45f).normalized;
        origin = position + Vector3.up * (originUpOffset + 0.25f); // higher origin for downward bias
        if (Physics.Raycast(origin, biasDir, out RaycastHit hit2, highlightDistance * 1.5f))
        {
            var obj = hit2.collider.gameObject;
            if (obj != this.gameObject && !obj.transform.IsChildOf(transform) && obj.CompareTag(hightlightTag))
            {
                if (obj != highlighted) { ClearHighlight(); SetHighlight(obj); }
                return;
            }
        }

        // nothing valid found
        ClearHighlight();
    }



    void SetHighlight(GameObject obj)
    {
        highlighted = obj;
        previousLayer = highlighted.layer;
        highlighted.layer = outlineLayer;
    }

    public void ClearHighlight()
    {
        if (highlighted != null)
        {
            highlighted.layer = previousLayer;
            highlighted = null;
        }
    }
}
#endif
