using Unity.VisualScripting;
using UnityEngine;
using System.Collections;


public class FlatEggplantController : DragController
{
    [HideInInspector]
    public bool isPlaced = false;

    public EggBowlController bowlController;

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null)
        {
            Vector3 targetPos = highlighted.transform.position + new Vector3(0, 0.1275f, 0.12f);

            StartCoroutine(AnimatePlacement(targetPos, transform.rotation, 0.5f));
        }
        ClearHighlight();
    }

    public IEnumerator AnimatePlacement(Vector3 targetPos, Quaternion targetRot, float duration)
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
        isDisabled = true;
        bowlController.enabled = true;
    }

}
