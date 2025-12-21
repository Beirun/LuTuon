using Unity.VisualScripting;
using UnityEngine;
using System.Collections;


public class MeatController : DragController
{
    [HideInInspector]
    public bool isPlaced = false;

    public MeatTouchManager touchManager;
    public ChoppingboardManager choppingboardManager;
    public Quaternion oldRot;
    public Vector3 oldPos;
    ProgressBarManager progressBarManager;

    public override void Start()
    {
        base.Start();
        progressBarManager = FindFirstObjectByType<ProgressBarManager>();
    }
    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && (!highlightTags.Contains("Choppingboard") || !choppingboardManager.isOccupied))
        {
            Vector3 targetPos = highlighted.transform.position + new Vector3(0f, 0.2f, 0.3f);
            if (highlightTags.Contains("MeatPlate")) StartCoroutine(AnimatePlacement(oldPos, oldRot, 0.5f));
            else if (highlightTags.Contains("Choppingboard")) StartCoroutine(AnimatePlacement(new Vector3(0f, 0.15f, 0.3f), transform.rotation, 0.5f));
            else StartCoroutine(AnimatePlacement(targetPos, transform.rotation, 0.5f));
        }
        else
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
        if (!highlightTags.Contains("MeatPlate")) isFinished = true;
        isDragging = false;
        isPerforming = false;
        touchManager.enabled = true;
        if (!highlightTags.Contains("Choppingboard"))
        {
            oldPos = startPos;
            oldRot = startRot;
            startPos = transform.position;
            startRot = transform.rotation;
        }
        if (!highlightTags.Contains("Grill"))
        {
            touchManager.enabled = false;
        }
        if (highlightTags.Contains("Choppingboard"))
        {
            choppingboardManager.isOccupied = true;
        }
        if (highlightTags.Contains("MeatPlate"))
        {
            highlightTags.Add("Choppingboard");
            highlightTags.Remove("MeatPlate");
            isPlaced = false;
            isPerforming = true;
            isDragging = true;
            progressBarManager.StartProgress(gameObject.transform, 2.5f);
            yield return new WaitForSeconds(2.5f);
            isDragging = false;
            isPerforming = false;
            isFinished = true;
        }

    }

}
