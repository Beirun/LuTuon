using System;
using System.Collections.Generic; // Required for List<>
using UnityEngine;

#if UNITY_ANDROID || UNITY_IOS
public class HighlightController : MonoBehaviour
{
    [Header("Highlight Settings")]
    // Changed from single string to List<string>
    public List<string> highlightTags = new() { "Choppingboard" };
    public string outlineLayerName = "Outline";
    public float maxDistance = 10f;

    private int outlineLayer;
    private int previousLayer;
    [HideInInspector] public GameObject highlighted;

    public virtual void Awake()
    {
        outlineLayer = LayerMask.NameToLayer(outlineLayerName);
        if (outlineLayer < 0)
            Debug.LogError($"Layer '{outlineLayerName}' not found. Add it in Project Settings > Tags and Layers.");
    }

    public void HighlightAtTouch(Vector2 screenPos, Camera cam, GameObject draggedObj)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) { ClearHighlight(); return; }

        Ray ray = cam.ScreenPointToRay(screenPos);

        // sphere radius = 0.25f tolerance around the ray
        RaycastHit[] hits = Physics.SphereCastAll(ray, 0.25f, maxDistance);
        if (hits.Length > 0)
        {
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var h in hits)
            {
                GameObject obj = h.collider.gameObject;

                // skip dragged object and its children
                if (obj == draggedObj || obj.transform.IsChildOf(draggedObj.transform)) continue;

                // NEW LOGIC: Check if the list contains the current object's tag
                if (!highlightTags.Contains(obj.tag)) continue;

                if (obj != highlighted)
                {
                    ClearHighlight();
                    SetHighlight(obj);
                }
                return;
            }
        }

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