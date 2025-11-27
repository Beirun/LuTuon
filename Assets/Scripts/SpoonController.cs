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


    public LidController lid;

    public override void EndDrag()
    {
        base.EndDrag();

        if (highlighted != null && (lid == null || !lid.isClose))
        {
            StartCoroutine(PlayStirAnimation(highlighted.transform.position));
        }
        else StartCoroutine(ReturnToStart());
        ClearHighlight();
    }

    IEnumerator PlayStirAnimation(Vector3 targetCenter)
    {
        Vector3 center = new Vector3(targetCenter.x, targetCenter.y + 0.5f, targetCenter.z);
        Quaternion stirRot = Quaternion.Euler(360f, 180f, 90f);

        Vector3 startPos = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 firstStirPos = center + new Vector3(0f, -stirDepth, 0f);
        float entryDuration = 0.5f;
        float entryElapsed = 0f;

        while (entryElapsed < entryDuration)
        {
            float t = entryElapsed / entryDuration;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPos, firstStirPos, t);
            transform.rotation = Quaternion.Slerp(startRotation, stirRot, t);

            entryElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = firstStirPos;
        transform.rotation = stirRot;

        // -------- FIGURE 8 PHASE --------

        float eightDur = stirDuration * 0.5f;
        float e = 0f;

        while (e < eightDur)
        {
            float t = e / eightDur;
            float angle = t * stirLoops * Mathf.PI * 2f;

            // figure-8 pattern using Lissajous-like motion
            float x = Mathf.Sin(angle);
            float z = Mathf.Sin(angle * 2f) * 0.5f; // half amplitude for the “crossing”

            Vector3 offset = new Vector3(x, -stirDepth, z) * stirRadius;
            transform.position = center + offset;
            transform.rotation = stirRot;

            e += Time.deltaTime;
            yield return null;
        }


        float tr = 0f;
        float transitionDur = stirDuration * 0.025f;
        Vector3 fromPosTransition = transform.position;
        Vector3 targetPosTransition = fromPosTransition + new Vector3(stirRadius, 0f, 0);

        while (tr < transitionDur)
        {

            float t = tr / transitionDur;
            t = t * t * (3f - 2f * t);
            Vector3 pos = Vector3.Lerp(fromPosTransition, targetPosTransition, t);
            transform.position = pos;
            tr += Time.deltaTime;
            yield return null;
        }

       
        // -------- SPIRAL PHASE --------
        float spiralDur = stirDuration * 0.5f;
        float s = 0f;

        while (s < spiralDur)
        {
            float t = s / spiralDur;

            // radius shrinks → creates spiral
            float r = Mathf.Lerp(stirRadius, 0f, t);
            float angle = t * stirLoops * Mathf.PI * 2f;

            Vector3 offset = new Vector3(Mathf.Cos(angle), -stirDepth, Mathf.Sin(angle)) * r;
            transform.position = center + offset;
            transform.rotation = stirRot;

            s += Time.deltaTime;
            yield return null;
        }


      

        yield return RestoreHeight(liftHeight, 0.2f);
        yield return RestoreRotation(startRot, 0.3f);
        yield return ReturnToStart();
        isFinished = true;
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
