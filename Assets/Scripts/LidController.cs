// DragAndPlace.cs
// Combines Draggable, PlacementController, and HighlightController into one script.

using UnityEngine;
using System.Collections;
using System;

#if UNITY_ANDROID || UNITY_IOS
[RequireComponent(typeof(Rigidbody))]
public class LidController : DragController
{
    private Vector3 placementOffset = new Vector3(0f, 1.25f, 0f);

    [Header("Animation Durations")]
    [Tooltip("Time in seconds to place object on a highlight.")]
    public float placeDuration = 0.25f;

    [HideInInspector]
    public bool isClose = false;

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && !isClose)
        {

            Vector3 targetPos = highlighted.transform.position + placementOffset;
            isClose = true;
            StartCoroutine(AnimatePlacement(targetPos, Quaternion.Euler(0f, 0f, 0f), placeDuration, true));
        }
        else
        {
            if(isClose)
            {
                isFinished = true;
                Debug.LogWarning($"is Finished: {isFinished}");
            }
            isClose = false;
        }
        ClearHighlight();
    }

    

    IEnumerator AnimatePlacement(Vector3 targetPos, Quaternion targetRot, float duration, bool useArc = false)
    {
        isPerforming = true;
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            // smoothstep ease in/out
            t = t * t * (3f - 2f * t);

            Vector3 pos = Vector3.Lerp(fromPos, targetPos, t);

            if (useArc)
            {
                float arcHeight = 0.5f; // smaller arc for smoother land
                pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            }

            transform.position = pos;
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isFinished = true;
        isDragging = false;
        isPerforming = false;
    }


}
#endif
