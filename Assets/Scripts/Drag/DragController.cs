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
    public PourVolumeManager pourManager;
    public DragManager manager;
    public bool isPerforming = false;
    public bool isDisabled = false;

    private float returnSpeed = 7.5f;         
    private float rotationReturnSpeed = 360f; 
    private bool isLifting = false;
    private bool hasFollowed = false;
    private Quaternion currentRot;
    private Vector3 currentPos;
    [Header("Animations")]
    public float liftDuration = 0.25f;
    private float liftRotationElapsed = 0f;
    Rigidbody rb;
    [HideInInspector] public bool isDragging = false;
    Vector3 dragOffset;

    [HideInInspector] public Vector3 startPos;
    [HideInInspector] public Quaternion startRot;

    

    [HideInInspector] public bool isInPot = false;
    [HideInInspector] public bool isFinished = false;
    [HideInInspector] public SFXPlayer sfx;


    [Header("Audio")]
    public AudioClip clip;
    Coroutine liftRoutine;
    Coroutine followRoutine;
    public virtual void Start()
    {
        if (cam == null) cam = Camera.main;
        if (manager == null) manager = FindFirstObjectByType<DragManager>();
        rb = GetComponent<Rigidbody>();
        pourManager = FindFirstObjectByType<PourVolumeManager>();
        sfx = FindFirstObjectByType<SFXPlayer>();
        startPos = transform.position;
        startRot = transform.rotation;
    }

    public virtual void Update()
    {

        if (Input.touchCount > 0 && !isInPot && (isDragging || !manager.isStillDragging) && !isPerforming && !isDisabled)
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
        if (isDragging) return;

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
                if(sfx != null && clip != null)
                {
                    sfx.PlaySound(clip);
                }
                SetLayerRecursive(gameObject, 0);
                liftRoutine = StartCoroutine(LiftObject());
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
        float elapsed = 0f;
        liftRotationElapsed = elapsed;
        Quaternion sr = transform.rotation;
        isLifting = true;
        if (!useRotation)
            targetRotation = sr;
        currentRot = transform.rotation;
        while (elapsed < liftDuration)
        {
            float t = elapsed / liftDuration;
            t = t * t * (3f - 2f * t);

            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(startY, liftHeight, t);
            pos.x = Mathf.Lerp(startX, startX - dragOffset.x, t);
            pos.z = Mathf.Lerp(startZ, startZ - dragOffset.z, t);
            currentPos = pos;
            transform.position = pos;
            transform.rotation = Quaternion.Slerp(sr, targetRotation, t);
            currentRot = transform.rotation;
            elapsed += Time.deltaTime;
            liftRotationElapsed = elapsed;
            yield return null;
        }

        transform.rotation = targetRotation;
        isLifting = false;
    }

    void PerformDrag(Vector2 screenPos)
    {
        if(isLifting) CancelLifting();
        Vector3 pos = GetWorldPosition(screenPos);
        if (!hasFollowed)
        {
            if (followRoutine != null)
            {
                StopCoroutine(followRoutine);
                followRoutine = null;
            }
            followRoutine = StartCoroutine(FollowToPosition(pos));
            return;
        }
        pos.y = liftHeight;
        transform.position = pos;

        HighlightAtTouch(screenPos, cam, gameObject);
    }

    public IEnumerator FollowToPosition(Vector3 targetPos)
    {
        Vector3 from = currentPos;
        Vector3 to = targetPos;
        Quaternion sr = currentRot;
        if (!useRotation)
            targetRotation = sr;


        while (liftRotationElapsed < liftDuration)
        {
            float t = liftRotationElapsed / liftDuration;
            t = t * t * (3f - 2f * t); // smoothstep

            Vector3 pos = Vector3.Lerp(from, to, t);

            currentPos = pos;
            transform.position = pos;
            transform.rotation = Quaternion.Slerp(sr, targetRotation, t);

            liftRotationElapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        hasFollowed = true;
    }


    public virtual void EndDrag()
    {
        if (highlighted == null)
        {
            StartCoroutine(ReturnToStart());

        }
    }

    void CancelLifting()
    {
        if (isLifting && liftRoutine != null)
        {
            StopCoroutine(liftRoutine);
            liftRoutine = null;
            isLifting = false;
        }
    }

    void CancelFollow()
    {
        if(followRoutine != null)
        {
            StopCoroutine(followRoutine);
            followRoutine = null;
        }
    }

    public IEnumerator ReturnToStart()
    {
        isPerforming = true;
        CancelLifting();
        CancelFollow();

        Vector3 from = transform.position;
        Vector3 to = startPos;

        float dist = Vector3.Distance(from, to);
        if (dist < 0.001f)
        {
            FinishReturn();
            yield break;
        }
        dist = Mathf.Max(dist, 3.25f);
        float arcHeight = Mathf.Clamp(dist * 0.2f, 0.2f, from.y);
        float traveled = 0f;

        while (true)
        {
            float step = returnSpeed * Time.deltaTime;
            traveled += step;

            float t = Mathf.Clamp01(traveled / dist);

            Vector3 pos = Vector3.Lerp(from, to, t);

            float arc = 4f * arcHeight * t * (1f - t);
            pos.y += arc;

            transform.position = pos;

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                startRot,
                rotationReturnSpeed * Time.deltaTime
            );

            if (t >= 1f)
                break;

            yield return null;
        }

        transform.position = startPos;
        transform.rotation = startRot;

        FinishReturn();
    }

    void FinishReturn()
    {
        if (gravityOnEnd && rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        isDragging = false;
        isPerforming = false;
        hasFollowed = false;
    }

    Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0, liftHeight, 0));
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : transform.position;
    }
}
#endif
