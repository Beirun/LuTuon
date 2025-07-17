// HighlightController.cs
// This script works with Draggable to highlight objects below it.
// It listens to drag events to perform its logic.

using UnityEngine;

public class HighlightController : MonoBehaviour
{
    [Tooltip("The tag assigned to objects that can be highlighted.")]
    public string highlightableTag = "Highlightable";
    [Tooltip("The name of the layer used for the outline effect.")]
    public string outlineLayerName = "Outline";
    [Tooltip("The maximum distance to check for highlightable objects below.")]
    public float highlightDistance = 5f;

    private GameObject currentlyHighlighted;
    private int previousLayer;
    private int outlineLayer;

    // A public property to let other scripts know what is currently highlighted.
    public GameObject CurrentlyHighlightedObject => currentlyHighlighted;

    void Start()
    {
        // Cache the layer index for efficiency.
        outlineLayer = LayerMask.NameToLayer(outlineLayerName);
    }

    void OnEnable()
    {
        // Subscribe to the drag events from the Draggable script
        Draggable.OnDrag += HandleDrag;
        Draggable.OnDragEnd += HandleDragEnd;
    }

    void OnDisable()
    {
        // Unsubscribe from the events to prevent memory leaks
        Draggable.OnDrag -= HandleDrag;
        Draggable.OnDragEnd -= HandleDragEnd;
    }

    /// <summary>
    /// Called continuously while an object is being dragged.
    /// </summary>
    /// <param name="draggedObjectPosition">The current position of the dragged object.</param>
    private void HandleDrag(Vector3 draggedObjectPosition)
    {
        HighlightNearbyObject(draggedObjectPosition);
    }

    /// <summary>
    /// Called when the drag operation ends.
    /// </summary>
    /// <param name="draggedObjectPosition">The final position of the dragged object.</param>
    private void HandleDragEnd(Vector3 draggedObjectPosition)
    {
        ClearHighlight();
    }

    /// <summary>
    /// Casts a ray downwards to find and highlight objects.
    /// </summary>
    /// <param name="position">The position to cast the ray from.</param>
    void HighlightNearbyObject(Vector3 position)
    {
        Ray ray = new Ray(position, Vector3.down);
        RaycastHit hit;

        // Perform the raycast
        if (Physics.Raycast(ray, out hit, highlightDistance))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Check if the hit object is highlightable and not the object being dragged
            if (hitObject.CompareTag(highlightableTag) && hitObject != this.gameObject)
            {
                // If we hit a new highlightable object, update the highlight
                if (hitObject != currentlyHighlighted)
                {
                    ClearHighlight(); // Clear previous highlight first
                    SetHighlight(hitObject);
                }
            }
            else
            {
                // If we hit something not highlightable, clear any existing highlight
                ClearHighlight();
            }
        }
        else
        {
            // If the raycast hits nothing, clear any existing highlight
            ClearHighlight();
        }
    }

    /// <summary>
    /// Applies the highlight effect to a GameObject.
    /// </summary>
    /// <param name="objToHighlight">The GameObject to highlight.</param>
    void SetHighlight(GameObject objToHighlight)
    {
        currentlyHighlighted = objToHighlight;
        previousLayer = currentlyHighlighted.layer;
        currentlyHighlighted.layer = outlineLayer;
    }

    /// <summary>
    /// Clears the current highlight effect.
    /// </summary>
    void ClearHighlight()
    {
        if (currentlyHighlighted != null)
        {
            // Restore the object's original layer
            currentlyHighlighted.layer = previousLayer;
            currentlyHighlighted = null;
        }
    }
}
