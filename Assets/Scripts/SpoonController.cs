using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
[RequireComponent(typeof(Rigidbody))]
public class SpoonController : MonoBehaviour
{
    [Header("Camera & Drag Settings")]
    public Camera cam;
    public float liftHeight = 2f;
    public bool useRotation = false;
    public Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
    public bool gravityOnEnd = false;

    [Header("Highlight Settings")]
    public string stirTag = "Stirrable";
    public string outlineLayerName = "Outline";
    public float highlightDistance = 5f;

    [Header("Animation Durations")]
    public float returnDuration = 1f;
    public float stirDuration = 2f;       // total time to stir
    public int stirLoops = 2;             // number of circles while stirring
    public float stirRadius = 0.2f;       // how wide the stir circle is
    public float stirDepth = 0.2f;        // how deep spoon dips while stirring

    private Rigidbody rb;
    private bool isDragging = false;
    private Vector3 dragOffset;

    private GameObject highlighted;
    private int previousLayer;
    private int outlineLayer;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        outlineLayer = LayerMask.NameToLayer(outlineLayerName);

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
                if (isDragging) EndDrag();
                break;
        }
    }

    void StartDrag(Touch touch)
    {
        Ray ray = cam.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
        {
            isDragging = true;
            dragOffset = transform.position - GetWorldPosition(touch.position);

            rb.useGravity = false;
            rb.isKinematic = true;
            StartCoroutine(LiftObject());
        }
    }

    IEnumerator LiftObject()
    {
        float startY = transform.position.y;
        float startX = transform.position.x;
        float startZ = transform.position.z;
        Quaternion startRot = transform.rotation;
        if (!useRotation) targetRotation = Quaternion.Euler(0f, startRot.eulerAngles.y, 0f);

        float duration = 0.25f, elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);

            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(startY, liftHeight, t);
            pos.x = Mathf.Lerp(startX, startX - dragOffset.x, t);
            pos.z = Mathf.Lerp(startZ, startZ - dragOffset.z, t);
            transform.position = pos;
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        //transform.position = new Vector3(transform.position.x, liftHeight, transform.position.z);
        transform.rotation = targetRotation;
    }

    void PerformDrag(Vector2 screenPos)
    {
        Vector3 pos = GetWorldPosition(screenPos);
        pos.y = liftHeight;
        transform.position = pos;
        HighlightBelow(pos);
    }

    void EndDrag()
    {
        isDragging = false;

        if (highlighted != null)
            StartCoroutine(PlayStirAnimation(highlighted.transform.position));
        else
            StartCoroutine(ReturnToStart());

        ClearHighlight();
    }

    private Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0, liftHeight, 0));
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : transform.position;
    }

    // --- Animations ---

    IEnumerator PlayStirAnimation(Vector3 targetCenter)
    {
        Vector3 center = new Vector3(targetCenter.x, targetCenter.y + 0.5f, targetCenter.z);
        Quaternion stirRot = Quaternion.Euler(360f, 180f, 90f);

        // --- step 0: animate into stir position ---
        Vector3 startPos = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 firstStirPos = center + new Vector3(stirRadius, -stirDepth, 0f); // entry point on circle
        float entryDuration = 0.3f;
        float entryElapsed = 0f;

        while (entryElapsed < entryDuration)
        {
            float t = entryElapsed / entryDuration;
            t = t * t * (3f - 2f * t); // smoothstep

            transform.position = Vector3.Lerp(startPos, firstStirPos, t);
            transform.rotation = Quaternion.Slerp(startRotation, stirRot, t);

            entryElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = firstStirPos;
        transform.rotation = stirRot;

        // --- step 1: do the stirring circles ---
        float elapsed = 0f;
        while (elapsed < stirDuration)
        {
            float t = elapsed / stirDuration;
            float angle = t * stirLoops * Mathf.PI * 2f;

            Vector3 offset = new Vector3(Mathf.Cos(angle), -stirDepth, Mathf.Sin(angle)) * stirRadius;
            transform.position = center + offset;
            transform.rotation = stirRot;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- step 2: lift, restore rotation, return home ---
        yield return RestoreHeight(liftHeight, 0.2f);
        yield return RestoreRotation(startRot, 0.3f);
        yield return ReturnToStart();
    }


    IEnumerator RestoreHeight(float targetY, float duration)
    {
        Vector3 fromPos = transform.position;
        Vector3 toPos = new Vector3(fromPos.x, targetY, fromPos.z);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(fromPos, toPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = toPos;
    }

    IEnumerator RestoreRotation(Quaternion targetRot, float duration)
    {
        Quaternion fromRot = transform.rotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRot;
    }



    IEnumerator ReturnToStart()
    {
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            float t = elapsed / returnDuration;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(fromPos, startPos, t);
            transform.rotation = Quaternion.Slerp(fromRot, startRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPos;
        transform.rotation = startRot;

        if (gravityOnEnd)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }

    // --- Highlighting ---

    void HighlightBelow(Vector3 position)
    {
        Ray ray = new Ray(position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, highlightDistance))
        {
            GameObject obj = hit.collider.gameObject;
            if (obj.CompareTag(stirTag) && obj != gameObject)
            {
                if (obj != highlighted)
                {
                    ClearHighlight();
                    SetHighlight(obj);
                }
            }
            else ClearHighlight();
        }
        else ClearHighlight();
    }

    void SetHighlight(GameObject obj)
    {
        highlighted = obj;
        previousLayer = highlighted.layer;
        highlighted.layer = outlineLayer;
    }

    void ClearHighlight()
    {
        if (highlighted != null)
        {
            highlighted.layer = previousLayer;
            highlighted = null;
        }
    }
}
#endif
