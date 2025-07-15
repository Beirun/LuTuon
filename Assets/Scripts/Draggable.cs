using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
public class Draggable : MonoBehaviour
{
    public Camera cam;
    public float maxY = 2.0f;
    public Material highlightMaterial;

    private float zCoord;
    private Vector2 lastInputPosition;
    private Rigidbody rb;
    private bool isDragging = false;
    private Vector3 dragOffset;

    private GameObject currentlyHighlighted;
    private Material originalMaterial;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

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
        RaycastHit hit;
        if (touch.phase == TouchPhase.Began)
        {
            if (Physics.Raycast(cam.ScreenPointToRay(touch.position), out hit) && hit.transform == transform)
            {
                isDragging = true;
                lastInputPosition = touch.position;
                zCoord = cam.WorldToScreenPoint(transform.position).z;
                dragOffset = transform.position - GetWorldPosition(touch.position);
                StartCoroutine(Lift());
            }
        }
        else if (touch.phase == TouchPhase.Moved && isDragging)
        {
            Drag(touch.position);
        }
        else if (touch.phase == TouchPhase.Ended && isDragging)
        {
            EndDrag();
            isDragging = false;
        }
    }

    IEnumerator Lift()
    {
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        float startPos = transform.position.y;
        float liftedY = maxY;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, startRot.eulerAngles.y, 0f);

        float duration = 0.25f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            Vector3 currentPosition = transform.position;
            currentPosition.y = Mathf.Lerp(startPos, liftedY, t);
            transform.position = currentPosition;

            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Vector3 finalPosition = transform.position;
        finalPosition.y = liftedY;
        transform.position = finalPosition;
        transform.rotation = targetRot;
    }

    void Drag(Vector2 screenPos)
    {
        Vector3 targetPosition = GetWorldPosition(screenPos) + dragOffset;
        targetPosition.y = maxY;
        transform.position = targetPosition;

        HighlightNearbyObject();
    }

    void HighlightNearbyObject()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5f))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject != currentlyHighlighted && hitObject != this.gameObject)
            {
                ClearHighlight();

                Renderer rend = hitObject.GetComponent<Renderer>();
                if (rend != null)
                {
                    originalMaterial = rend.material;
                    rend.material = highlightMaterial;
                    currentlyHighlighted = hitObject;
                }
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    void ClearHighlight()
    {
        if (currentlyHighlighted != null)
        {
            Renderer rend = currentlyHighlighted.GetComponent<Renderer>();
            if (rend != null && originalMaterial != null)
            {
                rend.material = originalMaterial;
            }
            currentlyHighlighted = null;
        }
    }

    void EndDrag()
    {
        ClearHighlight();

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }

    Vector3 GetWorldPosition(Vector2 screenPos)
    {
        Vector3 point = new Vector3(screenPos.x, screenPos.y, zCoord);
        return cam.ScreenToWorldPoint(point);
    }
}
#endif
