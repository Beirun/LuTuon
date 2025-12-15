using UnityEngine;
using System.Collections;

public class DicedOnionsController : DragController
{
    public LidController lid;
    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && (lid == null || !lid.isClose))
        {
            Vector3 targetPos = highlighted.transform.position + new Vector3(0f, 0.125f, 0f);
            StartCoroutine(AnimatePlacement(targetPos, transform.rotation, 0.5f));
        }
        else StartCoroutine(ReturnToStart());
        ClearHighlight();
    }

    public IEnumerator AnimatePlacement(Vector3 targetPos, Quaternion targetRot, float duration, bool isDragging = true)
    {
        isPerforming =true;
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(fromPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }


        if(isDragging) EnablePhysicsOnChildren(transform);
        if (isDragging) isFinished = true;
        this.isDragging = false;
        isPerforming = false;

    }

    void EnablePhysicsOnChildren(Transform parent)
    {
        isInPot = true;
        foreach (Transform child in parent)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            if (child.childCount > 0)
                EnablePhysicsOnChildren(child);
        }
    }
}
