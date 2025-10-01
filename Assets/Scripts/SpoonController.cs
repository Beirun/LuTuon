using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
[RequireComponent(typeof(Rigidbody))]
public class SpoonController : DragController
{
   

    [Header("Animation Durations")]
    public float stirDuration = 2f;       // total time to stir
    public int stirLoops = 2;             // number of circles while stirring
    public float stirRadius = 0.2f;       // how wide the stir circle is
    public float stirDepth = 0.2f;        // how deep spoon dips while stirring



    public override void EndDrag()
    {
        base.EndDrag();

        if (highlighted != null)
        {
            StartCoroutine(PlayStirAnimation(highlighted.transform.position));
        }
        ClearHighlight();
    }
    IEnumerator PlayStirAnimation(Vector3 targetCenter)
    {
        Vector3 center = new Vector3(targetCenter.x, targetCenter.y + 0.5f, targetCenter.z);
        Quaternion stirRot = Quaternion.Euler(360f, 180f, 90f);

        // --- step 0: animate into stir position ---
        Vector3 startPos = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 firstStirPos = center + new Vector3(stirRadius, -stirDepth, 0f); // entry point on circle
        float entryDuration = 0.3f;
        float entryElapsed = 0f;

        while (entryElapsed < entryDuration)
        {
            float t = entryElapsed / entryDuration;
            t = t * t * (3f - 2f * t); // smoothstep

            transform.position = Vector3.Lerp(startPos, firstStirPos, t);
            transform.rotation = Quaternion.Slerp(startRotation, stirRot, t);

            entryElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = firstStirPos;
        transform.rotation = stirRot;

        // --- step 1: do the stirring circles ---
        float elapsed = 0f;
        while (elapsed < stirDuration)
        {
            float t = elapsed / stirDuration;
            float angle = t * stirLoops * Mathf.PI * 2f;

            Vector3 offset = new Vector3(Mathf.Cos(angle), -stirDepth, Mathf.Sin(angle)) * stirRadius;
            transform.position = center + offset;
            transform.rotation = stirRot;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- step 2: lift, restore rotation, return home ---
        yield return RestoreHeight(liftHeight, 0.2f);
        yield return RestoreRotation(startRot, 0.3f);
        yield return ReturnToStart();
    }


    IEnumerator RestoreHeight(float targetY, float duration)
    {
        Vector3 fromPos = transform.position;
        Vector3 toPos = new Vector3(fromPos.x, targetY, fromPos.z);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(fromPos, toPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = toPos;
    }

    IEnumerator RestoreRotation(Quaternion targetRot, float duration)
    {
        Quaternion fromRot = transform.rotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRot;
    }



   
}
#endif
