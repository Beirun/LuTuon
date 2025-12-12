using UnityEngine;

public class DragManager : MonoBehaviour
{
    public bool isStillDragging = false;
    public DragController[] dragControllers;

    void Start()
    {
        dragControllers = FindObjectsByType<DragController>(FindObjectsSortMode.None);
    }

    private void Update()
    {
        foreach (var controller in dragControllers)
        {
            if (controller.isDragging)
            {
                isStillDragging = true;
                return; 
            }
        }
        isStillDragging = false;
    }
}