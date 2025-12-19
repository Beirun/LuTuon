using Unity.VisualScripting;
using UnityEngine;
using System.Collections;


public class RedChiliController : DragController
{
    [HideInInspector]
    public bool isPlaced = false;

    [Header("Choppingboard Manager")]
    public ChoppingboardManager choppingboardManager;

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && !choppingboardManager.isOccupied)
        {
            Vector3 targetPos = highlighted.transform.position + new Vector3(0.15f, 0.175f, 0f);

            StartCoroutine(AnimatePlacement(targetPos, Quaternion.Euler(0f, -90f, 0f), 0.5f));
        }
        ClearHighlight();
    }

    IEnumerator AnimatePlacement(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        isPerforming = true;
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
        isPlaced = true;
        choppingboardManager.isOccupied = true;
        isFinished = true;
        isDragging = false;
        isPerforming = false;
    }

}
