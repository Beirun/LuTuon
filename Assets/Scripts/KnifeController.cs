using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
[RequireComponent(typeof(Rigidbody))]
public class KnifeController : MonoBehaviour
{
    [Header("Camera & Drag Settings")]
    public Camera cam;
    public float liftHeight = 2f;
    public bool useRotation = false;
    public Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
    public bool gravityOnEnd = false;

    [Header("Highlight Settings")]
    public string choppingBoardTag = "Choppingboard";
    public string outlineLayerName = "Outline";
    public float highlightDistance = 5f;

    [Header("Animation Durations")]
    public float returnDuration = 1f;
    public float cutDuration = 0.4f;
    public float cutDepth = 0.5f;

    private Rigidbody rb;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private int outlineLayer;
    private int previousLayer;
    private GameObject highlighted;

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
        float elapsed = 0f, duration = 0.25f;
        Quaternion startRot = transform.rotation;
        if (!useRotation) targetRotation = Quaternion.Euler(0f, startRot.eulerAngles.y, 0f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);

            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(startY, liftHeight, t);
            pos.x = Mathf.Lerp(startX, startX - dragOffset.x, t);
            pos.z = Mathf.Lerp(startZ, startZ - dragOffset.z - 0.25f , t);
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
        pos.z = pos.z - 0.25f;
        pos.y = liftHeight;
        transform.position = pos;
        HighlightBelow(pos);
    }

    void EndDrag()
    {
        isDragging = false;
        if (highlighted != null)
        {
            StartCoroutine(PlayCutAnimation(highlighted.transform.position));
        }
        else
        {
            StartCoroutine(ReturnToStart());
        }
        ClearHighlight();
    }

    private Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0, liftHeight, 0));
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : transform.position;
    }

    // --- Animations ---
    IEnumerator PlayCutAnimation(Vector3 boardPos)
    {
        Quaternion rot = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);

        float[] xOffsets = { -0.15f, -0.05f, 0.05f, 0.15f };

        // start above first cut position
        Vector3 aboveBoard = new Vector3(boardPos.x + xOffsets[0], liftHeight, boardPos.z - 1f);
        yield return MoveTo(aboveBoard, rot, 0.2f);

        for (int i = 0; i < xOffsets.Length; i++)
        {
            // down slice at current offset
            Vector3 downPos = new Vector3(boardPos.x + xOffsets[i], liftHeight - cutDepth, boardPos.z - 1f);
            yield return MoveTo(downPos, rot, cutDuration * 0.5f);

            // if not last cut, go UP + SLIDE to next offset in one motion
            if (i < xOffsets.Length - 1)
            {
                Vector3 nextAbove = new Vector3(boardPos.x + xOffsets[i + 1], liftHeight, boardPos.z - 1f);
                yield return MoveTo(nextAbove, rot, cutDuration * 0.5f);
            }
            else
            {
                // last cut → just go up in same position
                Vector3 finalAbove = new Vector3(boardPos.x + xOffsets[i], liftHeight, boardPos.z - 1f);
                yield return MoveTo(finalAbove, rot, cutDuration * 0.5f);
            }
        }

        yield return ReturnToStart();
    }





    IEnumerator ReturnToStart()
    {
        yield return MoveTo(startPos, startRot, returnDuration);
        if (gravityOnEnd)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }

    IEnumerator MoveTo(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(fromPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }

    // --- Highlighting ---

    void HighlightBelow(Vector3 position)
    {
        Ray ray = new Ray(position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, highlightDistance))
        {
            GameObject obj = hit.collider.gameObject;
            if (obj.CompareTag(choppingBoardTag) && obj != gameObject)
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
