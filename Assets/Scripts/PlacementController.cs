using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
public class PlacementController : MonoBehaviour
{
    [Header("Placement Settings")]
    [Tooltip("The vertical offset to apply when placing the object on a surface.")]
    public Vector3 placementOffset = new Vector3(0f, 1.5f, 0f);

    [Tooltip("The position to return to if not placed on a valid target.")]
    public Transform returnPosition;

    [Tooltip("Use original spawn position as return position.")]
    public bool useOriginalAsReturn = true;

    [Header("Animation Settings")]
    [Tooltip("Time in seconds to place object on a highlight.")]
    public float placeDuration = 0.25f;

    [Tooltip("Time in seconds to return to starting position.")]
    public float returnDuration = 1.0f;

    [Tooltip("Height of the arc when returning to start position.")]
    public float arcHeight = 1.5f;

    [Tooltip("Use arc trajectory when returning to start.")]
    public bool useArcOnReturn = true;

    // Components
    private DragController dragController;

    // Position tracking
    private Vector3 startPos;
    private Quaternion startRot;

    // Placement state
    private bool isPlacing = false;

    // Events
    public delegate void PlacementAction(GameObject target);
    public static event PlacementAction OnPlacementStart;
    public static event PlacementAction OnPlacementComplete;

    void Start()
    {
        dragController = GetComponent<DragController>();

        // Save the original spawn position and rotation
        startPos = transform.position;
        startRot = transform.rotation;

        if (useOriginalAsReturn && returnPosition == null)
        {
            // Create a dummy transform to hold the return position
            GameObject returnObj = new GameObject(gameObject.name + "_ReturnPos");
            returnPosition = returnObj.transform;
            returnPosition.position = startPos;
            returnPosition.rotation = startRot;
        }
    }

    public void HandlePlacement(GameObject targetObject)
    {
        if (isPlacing) return;

        if (targetObject != null)
        {
            // Place on highlighted object
            Vector3 targetPos = targetObject.transform.position + placementOffset;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, 0f);

            OnPlacementStart?.Invoke(targetObject);
            StartCoroutine(AnimatePlacement(targetPos, targetRot, placeDuration, false, targetObject));
        }
        else
        {
            // Return to start position
            Vector3 returnPos = useOriginalAsReturn ? startPos :
                (returnPosition != null ? returnPosition.position : startPos);
            Quaternion returnRot = useOriginalAsReturn ? startRot :
                (returnPosition != null ? returnPosition.rotation : startRot);

            OnPlacementStart?.Invoke(null);
            StartCoroutine(AnimatePlacement(returnPos, returnRot, returnDuration, useArcOnReturn, null));
        }
    }

    IEnumerator AnimatePlacement(Vector3 targetPos, Quaternion targetRot, float duration, bool useArc, GameObject placementTarget)
    {
        isPlacing = true;

        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // smoothstep

            Vector3 pos = Vector3.Lerp(fromPos, targetPos, t);

            if (useArc)
            {
                // Add arc: peak at mid-point (t=0.5)
                pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            }

            transform.position = pos;
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        // Finalize drag physics
        if (dragController != null)
        {
            dragController.FinalizeDrag();
        }

        isPlacing = false;
        OnPlacementComplete?.Invoke(placementTarget);
    }

    public void SetReturnPosition(Vector3 position, Quaternion rotation)
    {
        if (returnPosition != null)
        {
            returnPosition.position = position;
            returnPosition.rotation = rotation;
        }
    }

    public void ResetToOriginalPosition()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        if (useOriginalAsReturn && returnPosition != null)
        {
            returnPosition.position = startPos;
            returnPosition.rotation = startRot;
        }
    }

    public bool IsPlacing()
    {
        return isPlacing;
    }
}
#endif