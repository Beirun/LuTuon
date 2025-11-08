using System.Collections;
using UnityEngine;


#if UNITY_ANDROID || UNITY_IOS
public class DragController : HighlightController
{
    [Header("Camera & Drag Settings")]
    public Camera cam;
    public float liftHeight = 2f;
    public bool useRotation = false;
    public Quaternion targetRotation = Quaternion.Euler(0f,0f,0f);
    public bool gravityOnEnd = false;

    [Header("Animations")]
    public float returnDuration = .5f;
    public float liftDuration = 0.25f;

    private Rigidbody rb;
    private bool isDragging = false;
    private Vector3 dragOffset;

    [HideInInspector]
    public Vector3 startPos;
    [HideInInspector]
    public Quaternion startRot;

    [Header("Z Offset")]
    public bool addZOffset = false;
    public float zOffsetAmount = 0.25f;
    public virtual void Start()
    {
        if (cam == null) cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartDrag(touch.position);
                    break;
                case TouchPhase.Moved:
                    if (isDragging) PerformDrag(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isDragging) EndDrag();
                    break;
            }
        }
    }

    void StartDrag(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                isDragging = true;
                dragOffset = transform.position - GetWorldPosition(screenPos);

                if (rb != null)
                {
                    rb.useGravity = false;
                    rb.isKinematic = true;
                }
                StartCoroutine(LiftObject());
            }
        }

    }

    IEnumerator LiftObject()
    {
        float startY = transform.position.y;
        float startX = transform.position.x;
        float startZ = transform.position.z;
        float elapsed = 0f, liftDuration = 0.25f;
        Quaternion startRot = transform.rotation;
        if (!useRotation) targetRotation = Quaternion.Euler(startRot.eulerAngles.x, startRot.eulerAngles.y, startRot.eulerAngles.z);

        while (elapsed < liftDuration)
        {
            float t = elapsed / liftDuration;
            t = t * t * (3f - 2f * t);

            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(startY, liftHeight, t);
            pos.x = Mathf.Lerp(startX, startX - dragOffset.x, t);
            if(!addZOffset) pos.z = Mathf.Lerp(startZ, startZ - dragOffset.z, t);
            else pos.z = Mathf.Lerp(startZ, startZ - dragOffset.z - zOffsetAmount, t);

            transform.position = pos;
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    void PerformDrag(Vector2 screenPos)
    {
        Vector3 pos = GetWorldPosition(screenPos);
        pos.y = liftHeight;
        if (addZOffset) pos.z -= zOffsetAmount;
        transform.position = pos;

        HighlightAtTouch(screenPos, cam, this.gameObject);
    }

    public virtual void EndDrag()
    {
        isDragging = false;
        if (highlighted == null) StartCoroutine(ReturnToStart());
    }

    public IEnumerator ReturnToStart()
    {
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;
        float duration = Mathf.Max(Mathf.Max(Vector3.Distance(fromPos, startPos) * returnDuration,0.5f) / 8, returnDuration * .8f);
        float elapsed = 0f;

        while(elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(fromPos, startPos, t);
            transform.rotation = Quaternion.Slerp(fromRot, startRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPos;
        transform.rotation = startRot;

        if (gravityOnEnd && rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }

    Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0, liftHeight, 0));
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : transform.position;
    }
}
#endif
