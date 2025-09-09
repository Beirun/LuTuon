// DragAndPlace.cs
// Combines Draggable, PlacementController, and HighlightController into one script.

using UnityEngine;
using System.Collections;
using System;

#if UNITY_ANDROID || UNITY_IOS
[RequireComponent(typeof(Rigidbody))]
public class LidController : MonoBehaviour
{
    [Header("Camera & Drag Settings")]
    [Tooltip("The camera used for screen-to-world conversions. Defaults to main camera.")]
    public Camera cam;
    [Tooltip("The maximum height the object will be lifted to when dragged.")]
    public float liftHeight = 2f;
    [Tooltip("Enable and set a specific rotation when lifting.")]
    public bool useRotation = false;
    public Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
    [Tooltip("Enable gravity when drag ends and not placed.")]
    public bool gravityOnEnd = false;

    [Header("Placement Settings")]
    [Tooltip("The vertical offset to apply when placing the object on a surface.")]
    public Vector3 placementOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Highlight Settings")]
    [Tooltip("The tag assigned to objects that can be highlighted.")]
    public string highlightableTag = "Highlightable";
    [Tooltip("The name of the layer used for the outline effect.")]
    public string outlineLayerName = "Outline";
    [Tooltip("The maximum distance to check for highlightable objects below.")]
    public float highlightDistance = 5f;

    [Header("Animation Durations")]
    [Tooltip("Time in seconds to place object on a highlight.")]
    public float placeDuration = 0.25f;
    [Tooltip("Time in seconds to return to starting position.")]
    public float returnDuration = 1.0f;


    // internal state
    private Rigidbody rb;
    private bool isDragging = false;
    private float zCoord;
    private Vector3 dragOffset;

    private GameObject currentlyHighlighted;
    private int previousLayer;
    private int outlineLayer;

    // Events
    public delegate void DragAction(Vector3 position);
    public static event DragAction OnDragStart;
    public static event DragAction OnDrag;
    public static event DragAction OnDragEnd;

    // Add these fields
    private Vector3 originalPos;
    private Quaternion originalRot;

    // fields
    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        outlineLayer = LayerMask.NameToLayer(outlineLayerName);

        // Save the original spawn position and rotation
        startPos = transform.position;
        startRot = transform.rotation;
    }


    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            HandleTouch(touch);
        }
    }

    void HandleTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                StartDrag(touch);
                break;
            case TouchPhase.Moved:
                if (isDragging) PerformDrag(touch.position);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isDragging)
                {
                    EndDrag();
                    if (gravityOnEnd) FinalizeDrag();
                }
                break;
        }
    }

    void StartDrag(Touch touch)
    {
        Ray ray = cam.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
        {
            isDragging = true;
            zCoord = cam.WorldToScreenPoint(transform.position).z;
            dragOffset = transform.position - GetWorldPosition(touch.position);

            originalPos = transform.position;   // save original
            originalRot = transform.rotation;

            StartCoroutine(LiftObject());
            OnDragStart?.Invoke(transform.position);
        }
    }


    IEnumerator LiftObject()
    {
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        float startY = transform.position.y;
        Quaternion startRot = transform.rotation;
        if (!useRotation) targetRotation = Quaternion.Euler(0f, startRot.eulerAngles.y, 0f);

        float duration = 0.25f, elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(startY, liftHeight, t);
            transform.position = pos;
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Vector3 finalPos = transform.position;
        finalPos.y = liftHeight;
        transform.position = finalPos;
        transform.rotation = targetRotation;
    }

    void PerformDrag(Vector2 screenPos)
    {
        Vector3 targetPos = GetWorldPosition(screenPos) + dragOffset;
        targetPos.y = liftHeight;
        transform.position = targetPos;
        OnDrag?.Invoke(transform.position);
        HighlightNearbyObject(transform.position);
    }

    void EndDrag()
    {
        isDragging = false;
        OnDragEnd?.Invoke(transform.position);
        HandlePlacement();
        ClearHighlight();
    }

    public void FinalizeDrag()
    {
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }

    private Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Vector3 point = new Vector3(screenPos.x, screenPos.y, zCoord);
        return cam.ScreenToWorldPoint(point);
    }

    // Placement handling
    void HandlePlacement()
    {
        if (currentlyHighlighted != null)
        {
            Vector3 targetPos = currentlyHighlighted.transform.position + placementOffset;
            StartCoroutine(AnimatePlacement(targetPos, Quaternion.Euler(0f, 0f, 0f), placeDuration, false));
        }
        else
        {
            StartCoroutine(AnimatePlacement(startPos, startRot, returnDuration, true)); // trajectory arc
        }
    }





    IEnumerator AnimatePlacement(Vector3 targetPos, Quaternion targetRot, float duration, bool useArc = false)
    {
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
                float arcHeight = 1.5f; // tweakable
                pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            }

            transform.position = pos;
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        FinalizeDrag();
    }





    // Highlight handling
    void HighlightNearbyObject(Vector3 position)
    {
        Ray ray = new Ray(position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, highlightDistance))
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj.CompareTag(highlightableTag) && hitObj != gameObject)
            {
                if (hitObj != currentlyHighlighted)
                {
                    ClearHighlight();
                    SetHighlight(hitObj);
                }
            }
            else ClearHighlight();
        }
        else ClearHighlight();
    }

    void SetHighlight(GameObject obj)
    {
        currentlyHighlighted = obj;
        previousLayer = currentlyHighlighted.layer;
        currentlyHighlighted.layer = outlineLayer;
    }

    void ClearHighlight()
    {
        if (currentlyHighlighted != null)
        {
            currentlyHighlighted.layer = previousLayer;
            currentlyHighlighted = null;
        }
    }
}
#endif
