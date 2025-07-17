// Draggable.cs
// This script makes a GameObject draggable using touch input.
// It handles lifting the object and moving it based on touch position.

using UnityEngine;
using System.Collections;
using System;

#if UNITY_ANDROID || UNITY_IOS
public class Draggable : MonoBehaviour
{
    [Tooltip("The camera used for screen-to-world conversions. Defaults to main camera.")]
    public Camera cam;
    [Tooltip("The maximum height the object will be lifted to when dragged.")]
    public float liftHeight = 2.0f;

    private Rigidbody rb;
    private bool isDragging = false;
    private float zCoord;
    private Vector3 dragOffset;

    // Events to communicate with other scripts
    public delegate void DragAction(Vector3 position);
    public static event DragAction OnDragStart;
    public static event DragAction OnDrag;
    public static event DragAction OnDragEnd;
    public bool gravityOnEnd = false;

    void Start()
    {
        // Assign the main camera if no camera is specified
        if (cam == null)
        {
            cam = Camera.main;
        }

        // Get the Rigidbody component attached to this GameObject
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check for touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            HandleTouch(touch);
        }
    }

    /// <summary>
    /// Handles the different phases of a touch input.
    /// </summary>
    /// <param name="touch">The touch input to handle.</param>
    void HandleTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                StartDrag(touch);
                break;
            case TouchPhase.Moved:
                if (isDragging)
                {
                    PerformDrag(touch.position);
                }
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isDragging)
                {
                    EndDrag();
                    if(gravityOnEnd) FinalizeDrag();
                }
                break;
        }
    }

    /// <summary>
    /// Initiates the drag operation if the touch is on this object.
    /// </summary>
    /// <param name="touch">The touch input that started the drag.</param>
    void StartDrag(Touch touch)
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(touch.position);

        // Check if the raycast hits this specific object
        if (Physics.Raycast(ray, out hit) && hit.transform == transform)
        {
            isDragging = true;

            // Calculate initial drag parameters
            zCoord = cam.WorldToScreenPoint(transform.position).z;
            dragOffset = transform.position - GetWorldPosition(touch.position);

            // Start the lifting coroutine
            StartCoroutine(LiftObject());

            // Fire the OnDragStart event
            OnDragStart?.Invoke(transform.position);
        }
    }

    /// <summary>
    /// Coroutine to animate the object lifting up and rotating.
    /// </summary>
    IEnumerator LiftObject()
    {
        // Make the Rigidbody kinematic to control it directly
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        float startY = transform.position.y;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, startRot.eulerAngles.y, 0f); // Level out the rotation on X and Z axes

        float duration = 0.25f;
        float elapsedTime = 0f;

        // Animate the lift and rotation over the specified duration
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // SmoothStep interpolation for smoother animation

            Vector3 currentPosition = transform.position;
            currentPosition.y = Mathf.Lerp(startY, liftHeight, t);
            transform.position = currentPosition;

            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position and rotation are set correctly
        Vector3 finalPosition = transform.position;
        finalPosition.y = liftHeight;
        transform.position = finalPosition;
        transform.rotation = targetRot;
    }

    /// <summary>
    /// Updates the object's position while it is being dragged.
    /// </summary>
    /// <param name="screenPos">The current screen position of the touch.</param>
    void PerformDrag(Vector2 screenPos)
    {
        Vector3 targetPosition = GetWorldPosition(screenPos) + dragOffset;
        targetPosition.y = liftHeight; // Keep the object at the lifted height
        transform.position = targetPosition;

        // Fire the OnDrag event
        OnDrag?.Invoke(transform.position);
    }

    /// <summary>
    /// Ends the drag operation and signals other components.
    /// </summary>
    void EndDrag()
    {
        isDragging = false;
        // Fire the OnDragEnd event, allowing other scripts (like PlacementController) to react
        OnDragEnd?.Invoke(transform.position);
    }

    /// <summary>
    /// Resets the Rigidbody to its original physics state.
    /// Called by PlacementController after placement is complete.
    /// </summary>
    public void FinalizeDrag()
    {
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }

    /// <summary>
    /// Converts a screen position to a world position at the object's depth.
    /// </summary>
    /// <param name="screenPos">The screen position to convert.</param>
    /// <returns>The corresponding world position.</returns>
    private Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Vector3 point = new Vector3(screenPos.x, screenPos.y, zCoord);
        return cam.ScreenToWorldPoint(point);
    }
}
#endif
