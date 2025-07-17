// PlacementController.cs
// This script handles placing the draggable object onto a highlighted surface.
// It listens for the end of a drag operation to perform the placement.

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Draggable), typeof(HighlightController))]
public class PlacementController : MonoBehaviour
{
    [Tooltip("The vertical offset to apply when placing the object on a surface.")]
    public Vector3 placementOffset = new Vector3(0f, 1.5f, 0f);

    private Draggable draggable;
    private HighlightController highlightController;

    void Awake()
    {
        // Get references to the other components on this GameObject
        draggable = GetComponent<Draggable>();
        highlightController = GetComponent<HighlightController>();
    }

    void OnEnable()
    {
        // Subscribe to the drag end event
        Draggable.OnDragEnd += HandleDragEnd;
    }

    void OnDisable()
    {
        // Unsubscribe from the event to prevent memory leaks
        Draggable.OnDragEnd -= HandleDragEnd;
    }

    /// <summary>
    /// This method is called when the drag operation concludes.
    /// </summary>
    /// <param name="position">The final position of the dragged object.</param>
    private void HandleDragEnd(Vector3 position)
    {
        GameObject highlightedObject = highlightController.CurrentlyHighlightedObject;

        // Check if an object was highlighted when the drag ended
        if (highlightedObject != null)
        {
            // Calculate the target position on top of the highlighted object
            Vector3 targetPosition = highlightedObject.transform.position + placementOffset;
            StartCoroutine(AnimatePlacement(targetPosition));
        }
        else
        {
            // If nothing is highlighted, just finalize the drag, allowing the object to fall
            draggable.FinalizeDrag();
        }
    }

    /// <summary>
    /// Coroutine to animate the object moving from its lifted position to the final placement spot.
    /// </summary>
    /// <param name="targetPosition">The world position to place the object at.</param>
    IEnumerator AnimatePlacement(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        // The target rotation can be customized, here it resets to no rotation.
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);

        float duration = 0.25f;
        float elapsedTime = 0f;

        // Animate the placement over the specified duration
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // SmoothStep for a smoother animation

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position and rotation are set precisely
        transform.position = targetPosition;
        transform.rotation = targetRotation;

        // Finalize the drag by restoring the Rigidbody's physics properties
        draggable.FinalizeDrag();
    }
}
