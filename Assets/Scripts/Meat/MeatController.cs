using Unity.VisualScripting;
using UnityEngine;
using System.Collections;


public class MeatController : DragController
{
    [HideInInspector]
    public bool isPlaced = false;

    public Vector3 newTargetPos = Vector3.zero;
    public MeatTouchManager touchManager;
    public ChoppingboardManager choppingboardManager;

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && (newTargetPos != null || !choppingboardManager.isOccupied))
        {
            Vector3 targetPos = highlighted.transform.position + new Vector3(0f, 0.2f, 0.3f);
            if(newTargetPos != Vector3.zero) targetPos = newTargetPos;

            StartCoroutine(AnimatePlacement(targetPos, transform.rotation, 0.5f));
        }else
        {
            choppingboardManager.isOccupied = false;
            isPlaced = false;
            StartCoroutine(ReturnToStart());
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
        isFinished = true;
        isDragging = false;
        isPerforming = false;
        touchManager.enabled = true;    
        if(newTargetPos != Vector3.zero)
        {
            touchManager.enabled = false;    
            isPerforming = true;
            choppingboardManager.isOccupied = true;
        }

    }

}
