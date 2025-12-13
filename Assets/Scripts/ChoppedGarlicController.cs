using UnityEngine;
using System.Collections;

public class ChoppedGarlicController : DragController
{
    public LidController lid;
    public bool makeChildAfterPlacement = false;  

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && (lid == null || !lid.isClose))
        {
            Vector3 targetPos = highlighted.transform.position + new Vector3(0f, 0.125f, 0f);
            if (makeChildAfterPlacement && highlighted != null && !highlighted.CompareTag("Pan"))
            {
                transform.SetParent(highlighted.transform, true);
            }
            StartCoroutine(AnimatePlacement(targetPos, transform.rotation, 0.5f));
        }
        else
        {
            StartCoroutine(ReturnToStart());
        }
        ClearHighlight();
    }

    public IEnumerator AnimatePlacement(Vector3 targetPos, Quaternion targetRot, float duration, bool isDragging = true)
    {
        isPerforming = true;
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

        if (isDragging) EnablePhysicsOnChildren(transform);

        isFinished = true;
        isPerforming = false;
        this.isDragging = false;
    }

    void EnablePhysicsOnChildren(Transform p)
    {
        isInPot = true;
        foreach (Transform c in p)
        {
            Rigidbody rb = c.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            if (c.childCount > 0)
                EnablePhysicsOnChildren(c);
        }
    }
}
