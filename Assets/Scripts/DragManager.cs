using UnityEngine;

public class DragManager : MonoBehaviour
{
    public bool isStillDragging = false;
    public DragController[] dragControllers;

    void Start()
    {
        // Find all active DragControllers (and subclasses)
        dragControllers = FindObjectsByType<DragController>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        foreach (var controller in dragControllers)
        {
            // 3. If we find even ONE that is dragging...
            if (controller.isDragging)
            {
                isStillDragging = true;
                return; 
            }
        }
        isStillDragging = false;
    }
}