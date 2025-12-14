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
        // drag controllers
        dragControllers = FindObjectsByType<DragController>(FindObjectsSortMode.None);
        for (int i = 0; i < dragControllers.Length; i++)
            originalDisabled[dragControllers[i]] = dragControllers[i].isDisabled;

        // single canvas overlays
        Canvas c = FindFirstObjectByType<Canvas>();
        overlays = new GameObject[2];
        overlays[0] = c.transform.Find("BlackOverlay")?.gameObject;
        overlays[1] = c.transform.Find("EndOverlay")?.gameObject;
    }

    void Update()
    {
        // dragging check
        isStillDragging = false;
        for (int i = 0; i < dragControllers.Length; i++)
        {
            if (dragControllers[i] && dragControllers[i].isDragging)
            {
                isStillDragging = true;
                break;
            }
        }

        // overlay check
        bool overlayActive =
            (overlays[0] && overlays[0].activeSelf) ||
            (overlays[1] && overlays[1].activeSelf);

        if (overlayActive == wasOverlayActive) return;
        wasOverlayActive = overlayActive;

        // apply disable / restore
        for (int i = 0; i < dragControllers.Length; i++)
        {
            var dc = dragControllers[i];
            if (!dc) continue;

            dc.isDisabled = overlayActive ? true : originalDisabled[dc];
        }
    }
}
