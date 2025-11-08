using UnityEngine;
using System.Collections;
public class ChoppedGingerController : DragController
{

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null)
        {
            Vector3 targetPos = highlighted.transform.position + new Vector3(0f, 0.125f, 0f);

            StartCoroutine(AnimatePlacement(targetPos, transform.rotation, 0.5f));
        }
        ClearHighlight();
    }
    public IEnumerator AnimatePlacement(Vector3 targetPos, Quaternion targetRot, float duration, bool isDragging = true)
    {
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            Vector3 pos = Vector3.Lerp(fromPos, targetPos, t);


            transform.position = pos;
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (isDragging) EnablePhysicsOnChildren(transform);

    }
    void EnablePhysicsOnChildren(Transform parent)
    {
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
