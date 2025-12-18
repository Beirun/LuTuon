using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
[RequireComponent(typeof(Rigidbody))]
public class KnifeController : DragController
{
    [Header("Animation Durations")]
    public float cutDuration = 0.4f;
    public float cutDepth = 0.5f;
    public int numberOfCuts = 4;

    [HideInInspector]
    public int cutsMade = 0;

    [Header("Choppingboard Manager")]
    public ChoppingboardManager choppingboardManager;
    public override void EndDrag()
    {
        base.EndDrag();

        if (highlighted != null && choppingboardManager.isOccupied)
        {
            StartCoroutine(PlayCutAnimation(highlighted.transform.position));
        }
        else if(!choppingboardManager.isOccupied) StartCoroutine(ReturnToStart());
        ClearHighlight();
    }

    IEnumerator PlayCutAnimation(Vector3 boardPos)
    {
        isPerforming = true;
        Quaternion rot = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);



        float[] xOffsets = new float[numberOfCuts];

        float spacing = 0.1f;
        float totalWidth = (numberOfCuts - 1) * spacing;

        for (int i = 0; i < numberOfCuts; i++)
        {
            xOffsets[i] = -totalWidth / 2 + i * spacing;
        }

        Vector3 aboveBoard = new Vector3(boardPos.x + xOffsets[0], liftHeight, boardPos.z - 1f);
        yield return MoveTo(aboveBoard, rot, 0.2f);

        for (int i = 0; i < xOffsets.Length; i++)
        {
            Vector3 downPos = new Vector3(boardPos.x + xOffsets[i], liftHeight - cutDepth, boardPos.z - 1f);
            yield return MoveTo(downPos, rot, cutDuration * 0.5f);
            cutsMade++;

            if (i < xOffsets.Length - 1)
            {
                Vector3 nextAbove = new Vector3(boardPos.x + xOffsets[i + 1], liftHeight, boardPos.z - 1f);
                yield return MoveTo(nextAbove, rot, cutDuration * 0.5f);
            }
            else
            {
                Vector3 finalAbove = new Vector3(boardPos.x + xOffsets[i], liftHeight, boardPos.z - 1f);
                yield return MoveTo(finalAbove, rot, cutDuration * 0.5f);
            }
        }
        choppingboardManager.isOccupied = false;
        yield return ReturnToStart();
        isFinished = true;
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

}
#endif
