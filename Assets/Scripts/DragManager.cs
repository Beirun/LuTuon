using UnityEngine;
using System.Collections.Generic;

public class DragManager : MonoBehaviour
{
    public bool isStillDragging = false;

    DragController[] dragControllers;
    GameObject[] overlays;

    Dictionary<DragController, bool> originalDisabled = new Dictionary<DragController, bool>();
    bool wasOverlayActive;

    void Awake()
    {
        dragControllers = FindObjectsByType<DragController>(FindObjectsSortMode.None);
        for (int i = 0; i < dragControllers.Length; i++)
            originalDisabled[dragControllers[i]] = dragControllers[i].isDisabled;

        Canvas c = FindFirstObjectByType<Canvas>();
        overlays = new GameObject[2];
        overlays[0] = GameObject.Find("Canvas").transform.Find("BlackOverlay").gameObject;
        overlays[1] = GameObject.Find("Canvas").transform.Find("EndOverlay").gameObject;
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

        bool overlayActive =
            (overlays[0] && overlays[0].activeSelf) ||
            (overlays[1] && overlays[1].activeSelf);

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
        }
    }

    public void DisableAllDragging()
    {
        for (int i = 0; i < dragControllers.Length; i++)
        {
            var dc = dragControllers[i];
            if (!dc) continue;

            dc.isDisabled = true;
        }
    }

    public void RestoreDraggingState()
    {
        for (int i = 0; i < dragControllers.Length; i++)
        {
            var dc = dragControllers[i];
            if (!dc) continue;

            if (originalDisabled.ContainsKey(dc))
                dc.isDisabled = originalDisabled[dc];
        }
    }
}
