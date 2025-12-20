using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
[RequireComponent(typeof(Rigidbody))]
public class SpoonController : DragController
{
   

    [Header("Animation Durations")]
    public float stirDuration = 2f;     
    public int stirLoops = 2;            
    public float stirRadius = 0.2f;  
    public float stirDepth = 0.2f;  
    public float xRot = 360f;
    public float yRot = 180f;
    public float zRot = 90f;

    public LidController lid;
    public Vector3 targetOffset = new(0f, 0f, 0f); 

    public override void EndDrag()
    {
        base.EndDrag();

        if (highlighted != null && (lid == null || !lid.isClose))
        {
            StartCoroutine(PlayStirAnimation(highlighted.transform.position + targetOffset));
        }
        else StartCoroutine(ReturnToStart());
        ClearHighlight();
    }

    IEnumerator PlayStirAnimation(Vector3 targetCenter)
    {
        isPerforming = true;
        Vector3 center = new Vector3(targetCenter.x, targetCenter.y + 0.5f, targetCenter.z);
        Quaternion stirRot = Quaternion.Euler(xRot, yRot, zRot);

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

       
        float spiralDur = stirDuration * 0.5f;
        float s = 0f;

        while (s < spiralDur)
        {
            float t = s / spiralDur;

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
        isPerforming = false;

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
