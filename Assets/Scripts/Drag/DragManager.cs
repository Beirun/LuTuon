using UnityEngine;
using System.Collections.Generic;

public class DragManager : MonoBehaviour
{
    public bool isStillDragging = false;

    DragController[] dragControllers;
    List<GameObject> overlays = new List<GameObject>();

    Dictionary<DragController, bool> originalDisabled = new Dictionary<DragController, bool>();
    bool wasOverlayActive;
    public bool areDisabled = false;

    void Awake()
    {
        dragControllers = FindObjectsByType<DragController>(FindObjectsSortMode.None);
        for (int i = 0; i < dragControllers.Length; i++)
            originalDisabled[dragControllers[i]] = dragControllers[i].isDisabled;

        Canvas c = FindFirstObjectByType<Canvas>();

        string[] overlayNames = { "BlackOverlay", "ExtraOverlay", "EndOverlay", "TutorialOverlay" };
        foreach (var name in overlayNames)
        {
            Transform t = FindDeepChild(c.transform, name);
            if (t != null)
                overlays.Add(t.gameObject);
        }
    }

    // Recursive search for child by name
    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    void Update()
    {
        isStillDragging = false;
        for (int i = 0; i < dragControllers.Length; i++)
        {
            if (dragControllers[i] && dragControllers[i].isDragging)
            {
                isStillDragging = true;
                break;
            }
        }

        bool overlayActive = false;
        foreach (var overlay in overlays)
        {
            if (overlay && overlay.activeSelf)
            {
                overlayActive = true;
                break;
            }
        }

        if (overlayActive == wasOverlayActive) return;
        wasOverlayActive = overlayActive;

        ApplyOverlayState(overlayActive);
    }

    void ApplyOverlayState(bool overlayActive)
    {
        for (int i = 0; i < dragControllers.Length; i++)
        {
            var dc = dragControllers[i];
            if (!dc) continue;

            dc.isDisabled = overlayActive ? true : originalDisabled[dc];
            areDisabled = overlayActive;
        }
    }

    public void DisableAllDragging()
    {
        areDisabled = true;
        for (int i = 0; i < dragControllers.Length; i++)
        {
            var dc = dragControllers[i];
            if (!dc) continue;

            dc.isDisabled = true;
        }
    }

    public void RestoreDraggingState()
    {
        areDisabled = false;
        for (int i = 0; i < dragControllers.Length; i++)
        {
            var dc = dragControllers[i];
            if (!dc) continue;

            if (originalDisabled.ContainsKey(dc))
                dc.isDisabled = originalDisabled[dc];
        }
    }
}
