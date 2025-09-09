using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
public class Draggable : MonoBehaviour
{
    [Tooltip("The camera used for screen-to-world conversions. Defaults to main camera.")]
    public Camera cam;
    [Tooltip("The maximum height the object will be lifted to when dragged.")]
    public float liftHeight = 2.0f;

    public Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
    public bool useRotation = false;
    private Rigidbody rb;
    private bool isDragging = false;
    private float zCoord;
    private Vector3 dragOffset;

    public bool gravityOnEnd = false;

    // Instance-based events
    public delegate void DragAction(Draggable draggable, Vector3 position);
    public event DragAction OnDragStart;
    public event DragAction OnDrag;
    public event DragAction OnDragEnd;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        rb = GetComponent<Rigidbody>();
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
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(touch.position);

        if (Physics.Raycast(ray, out hit) && hit.transform == transform)
        {
            isDragging = true;
            zCoord = cam.WorldToScreenPoint(transform.position).z;
            dragOffset = transform.position - GetWorldPosition(touch.position);
            StartCoroutine(LiftObject());
            OnDragStart?.Invoke(this, transform.position);
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

        float duration = 0.25f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            Vector3 currentPosition = transform.position;
            currentPosition.y = Mathf.Lerp(startY, liftHeight, t);
            transform.position = currentPosition;

            transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Vector3 finalPosition = transform.position;
        finalPosition.y = liftHeight;
        transform.position = finalPosition;
        transform.rotation = targetRotation;
    }

    void PerformDrag(Vector2 screenPos)
    {
        Vector3 targetPosition = GetWorldPosition(screenPos) + dragOffset;
        targetPosition.y = liftHeight;
        transform.position = targetPosition;
        OnDrag?.Invoke(this, transform.position);
    }

    void EndDrag()
    {
        isDragging = false;
        OnDragEnd?.Invoke(this, transform.position);
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
}
#endif
