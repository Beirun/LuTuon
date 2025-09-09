using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
[RequireComponent(typeof(Rigidbody))]
public class DragController : MonoBehaviour
{
    [Header("Camera & Drag Settings")]
    public Camera cam;
    public float liftHeight = 2f;
    public bool useRotation = false;
    public Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
    public bool gravityOnEnd = false;

    [Header("Animation Settings")]
    public float liftDuration = 0.25f;

    private Rigidbody rb;
    private PlacementController placementController;
    private HighlightController highlightController;

    private bool isDragging = false;
    private Vector3 dragOffset;
    private Vector3 dragEdgeOffset; // <-- persistent offset

    private Vector3 originalPos;
    private Quaternion originalRot;

    public delegate void DragAction(Vector3 position);
    public static event DragAction OnDragStart;
    public static event DragAction OnDrag;
    public static event DragAction OnDragEnd;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        rb = GetComponent<Rigidbody>();
        placementController = GetComponent<PlacementController>();
        highlightController = GetComponent<HighlightController>();
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
                if (isDragging)
                    PerformDrag(touch.position);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isDragging)
                    EndDrag();
                break;
        }
    }

    void StartDrag(Touch touch)
    {
        Ray ray = cam.ScreenPointToRay(touch.position);

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
        {
            isDragging = true;
            originalPos = transform.position;
            originalRot = transform.rotation;

            dragOffset = transform.position - GetWorldPosition(touch.position);

            StartCoroutine(LiftObject(touch.position));
            OnDragStart?.Invoke(transform.position);
        }
    }

    IEnumerator LiftObject(Vector2 startScreenPos)
    {
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        if (!useRotation)
            targetRotation = Quaternion.Euler(0f, startRot.eulerAngles.y, 0f);

        // calculate offset based on screen position
        float screenHalfX = Screen.width * 0.5f;
        float screenHalfY = Screen.height * 0.5f;
        float edgeFactorX = (startScreenPos.x - screenHalfX) / screenHalfX;
        float edgeFactorY = (startScreenPos.y - screenHalfY) / screenHalfY;

        float maxOffsetX = 0.7f;
        float maxOffsetZ = 0.7f;

        dragEdgeOffset = Vector3.right * (-edgeFactorX * maxOffsetX) +
                         Vector3.forward * (-edgeFactorY * maxOffsetZ);

        Vector3 endPos = new Vector3(startPos.x, liftHeight, startPos.z) + dragEdgeOffset;

        float elapsedTime = 0f;
        while (elapsedTime < liftDuration)
        {
            float t = elapsedTime / liftDuration;
            t = t * t * (3f - 2f * t); // smoothstep

            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        transform.rotation = targetRotation;
    }

    void PerformDrag(Vector2 screenPos)
    {
        Vector3 targetPos = GetWorldPosition(screenPos) + dragOffset;

        targetPos.y = liftHeight;
        targetPos += dragEdgeOffset; // <-- keep offset applied

        transform.position = targetPos;

        OnDrag?.Invoke(transform.position);

        if (highlightController != null)
            highlightController.CheckForHighlightableObject(transform.position);
    }

    void EndDrag()
    {
        isDragging = false;
        OnDragEnd?.Invoke(transform.position);

        if (placementController != null)
        {
            GameObject highlightedObject = highlightController != null ?
                highlightController.GetCurrentlyHighlighted() : null;

            placementController.HandlePlacement(highlightedObject);
        }
        else if (gravityOnEnd)
        {
            FinalizeDrag();
        }

        if (highlightController != null)
            highlightController.ClearHighlight();
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
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0, liftHeight, 0));

        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : transform.position;
    }

    public bool IsDragging() => isDragging;
    public Vector3 GetOriginalPosition() => originalPos;
    public Quaternion GetOriginalRotation() => originalRot;
}
#endif
