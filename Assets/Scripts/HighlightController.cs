using UnityEngine;
using System.Collections.Generic;

#if UNITY_ANDROID || UNITY_IOS
public class HighlightController : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Tooltip("The tag assigned to objects that can be highlighted.")]
    public string highlightableTag = "Highlightable";

    [Tooltip("The name of the layer used for the outline effect.")]
    public string outlineLayerName = "Outline";

    [Tooltip("The maximum distance to check for highlightable objects below.")]
    public float highlightDistance = 5f;

    [Tooltip("Direction to check for highlightable objects.")]
    public Vector3 checkDirection = Vector3.down;

    [Tooltip("Check for highlights automatically during drag.")]
    public bool autoCheckDuringDrag = true;

    [Header("Advanced Settings")]
    [Tooltip("Offset from object position when checking for highlights.")]
    public Vector3 checkOffset = Vector3.zero;

    [Tooltip("Use sphere cast instead of raycast for broader detection.")]
    public bool useSphereCast = false;

    [Tooltip("Radius for sphere cast if enabled.")]
    public float sphereCastRadius = 0.5f;

    // Internal state
    private GameObject currentlyHighlighted;
    private int previousLayer;
    private int outlineLayer;

    // Events
    public delegate void HighlightAction(GameObject highlightedObject);
    public static event HighlightAction OnHighlightStart;
    public static event HighlightAction OnHighlightEnd;

    void Start()
    {
        outlineLayer = LayerMask.NameToLayer(outlineLayerName);

        if (outlineLayer == -1)
        {
            Debug.LogWarning($"Outline layer '{outlineLayerName}' not found! Please create it in the layer settings.");
        }
    }

    public void CheckForHighlightableObject(Vector3 position)
    {
        if (!autoCheckDuringDrag) return;

        HighlightNearbyObject(position);
    }

    public void ManualCheckForHighlight()
    {
        HighlightNearbyObject(transform.position);
    }

    void HighlightNearbyObject(Vector3 position)
    {
        position += checkOffset;
        GameObject hitObj = null;

        if (useSphereCast)
        {
            // Use sphere cast for broader detection
            if (Physics.SphereCast(position, sphereCastRadius, checkDirection, out RaycastHit hit, highlightDistance))
            {
                hitObj = hit.collider.gameObject;
            }
        }
        else
        {
            // Use standard raycast
            Ray ray = new Ray(position, checkDirection);

            if (Physics.Raycast(ray, out RaycastHit hit, highlightDistance))
            {
                hitObj = hit.collider.gameObject;
            }
        }

        // Process the hit object
        if (hitObj != null && hitObj.CompareTag(highlightableTag) && hitObj != gameObject)
        {
            if (hitObj != currentlyHighlighted)
            {
                ClearHighlight();
                SetHighlight(hitObj);
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    void SetHighlight(GameObject obj)
    {
        if (outlineLayer == -1) return;

        currentlyHighlighted = obj;
        previousLayer = currentlyHighlighted.layer;
        currentlyHighlighted.layer = outlineLayer;

        OnHighlightStart?.Invoke(currentlyHighlighted);
    }

    public void ClearHighlight()
    {
        if (currentlyHighlighted != null)
        {
            currentlyHighlighted.layer = previousLayer;
            GameObject clearedObject = currentlyHighlighted;
            currentlyHighlighted = null;

            OnHighlightEnd?.Invoke(clearedObject);
        }
    }

    public GameObject GetCurrentlyHighlighted()
    {
        return currentlyHighlighted;
    }

    public bool IsHighlighting()
    {
        return currentlyHighlighted != null;
    }

    // Method for external objects to register as highlightable
    public static void RegisterHighlightable(GameObject obj, string tag)
    {
        obj.tag = tag;
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 startPos = transform.position + checkOffset;

        if (useSphereCast)
        {
            // Draw sphere cast visualization
            Gizmos.DrawWireSphere(startPos, sphereCastRadius);
            Gizmos.DrawRay(startPos, checkDirection * highlightDistance);
            Gizmos.DrawWireSphere(startPos + checkDirection * highlightDistance, sphereCastRadius);
        }
        else
        {
            // Draw raycast visualization
            Gizmos.DrawRay(startPos, checkDirection * highlightDistance);
        }
    }
}
#endif