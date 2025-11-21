using System;
using System.Collections;
using UnityEngine;

#if UNITY_ANDROID || UNITY_IOS
public class DragController : HighlightController
{
    [Header("Camera & Drag Settings")]
    public Camera cam;
    public float liftHeight = 2f;
    public bool useRotation = false;
    public Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
    public bool gravityOnEnd = false;

    [Header("Animations")]
    public float returnDuration = .5f;
    public float liftDuration = 0.25f;

    private Rigidbody rb;
    [HideInInspector]
    public bool isDragging = false;
    private Vector3 dragOffset;

    [HideInInspector] public Vector3 startPos;
    [HideInInspector] public Quaternion startRot;

    [Header("Z Offset")]
    public bool addZOffset = false;
    public float zOffsetAmount = 0.25f;

    [HideInInspector] public bool isInPot = false;
    [HideInInspector] public bool isFinished = false;

    public virtual void Start()
    {
        if (cam == null) cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;
    }

    public virtual void Update()
    {
        if (Input.touchCount > 0 && !isInPot)
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

                SetLayerRecursive(gameObject, 0);  // CHANGE LAYER TO DEFAULT

                StartCoroutine(LiftObject());
            }
        }
    }

    void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform c in obj.transform)
            SetLayerRecursive(c.gameObject, layer);
    }

    IEnumerator LiftObject()
    {
        float startY = transform.position.y;
        float startX = transform.position.x;
        float startZ = transform.position.z;
        float elapsed = 0f, ld = liftDuration;
        Quaternion sr = transform.rotation;
        if (!useRotation) targetRotation = Quaternion.Euler(sr.eulerAngles.x, sr.eulerAngles.y, sr.eulerAngles.z);

        while (elapsed < ld)
        {
            float t = elapsed / ld;
            t = t * t * (3f - 2f * t);

            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(startY, liftHeight, t);
            pos.x = Mathf.Lerp(startX, startX - dragOffset.x, t);
            pos.z = addZOffset
                ? Mathf.Lerp(startZ, startZ - dragOffset.z - zOffsetAmount, t)
                : Mathf.Lerp(startZ, startZ - dragOffset.z, t);

            transform.position = pos;
            transform.rotation = Quaternion.Slerp(sr, targetRotation, t);

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
        if (highlighted == null) StartCoroutine(ReturnToStart());
    }

    public IEnumerator ReturnToStart()
    {
        Vector3 fp = transform.position;
        Quaternion fr = transform.rotation;
        float duration = Mathf.Max(Mathf.Max(Vector3.Distance(fp, startPos) * returnDuration, 0.5f) / 8, returnDuration * .8f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(fp, startPos, t);
            transform.rotation = Quaternion.Slerp(fr, startRot, t);
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
        isDragging = false;
    }

    Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0, liftHeight, 0));
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : transform.position;
    }
}
#endif
