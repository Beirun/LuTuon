using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
public class ForkController : DragController
{
    public float scrambleDuration = 2f;
    public int loops = 3;
    public float ellipseA = 0.15f;
    public float ellipseB = 0.07f;
    public float depth = 0.15f;

    public LidController lid;

    public override void EndDrag()
    {
        base.EndDrag();

        if (highlighted != null && (lid == null || !lid.isClose))
            StartCoroutine(PlayScramble(highlighted.transform.position));
        else
            StartCoroutine(ReturnToStart());

        ClearHighlight();
    }

    IEnumerator PlayScramble(Vector3 targetCenter)
    {
        Vector3 center = new Vector3(targetCenter.x, targetCenter.y + 0.5f, targetCenter.z);
        Quaternion startRot = transform.rotation;
        Quaternion forkRot = Quaternion.Euler(245f, -100f, 9f);

        Vector3 startPos = transform.position;
        Vector3 dipPos = center + new Vector3(-0.7f, -depth, 0f);

        float entryDur = 0.4f;
        float entryT = 0f;

        while (entryT < entryDur)
        {
            float t = entryT / entryDur;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, dipPos, t);
            transform.rotation = Quaternion.Slerp(startRot, forkRot, t);
            entryT += Time.deltaTime;
            yield return null;
        }

        transform.position = dipPos;
        transform.rotation = forkRot;

        float d = 0f;
        float total = scrambleDuration;
        float twoPi = Mathf.PI * 2f;

        while (d < total)
        {
            float t = d / total;
            float ang = t * loops * twoPi;

            float y = Mathf.Cos(ang) * ellipseA;
            float z = Mathf.Sin(ang) * ellipseB;

            transform.position = center + new Vector3(-0.7f, -depth + y, z);

            d += Time.deltaTime;
            yield return null;
        }


        yield return RestoreHeight(liftHeight, 0.2f);
        yield return RestoreRotation(startRot, 0.3f);
        yield return ReturnToStart();
        isFinished = true;
    }

    IEnumerator RestoreHeight(float targetY, float dur)
    {
        Vector3 from = transform.position;
        Vector3 to = new Vector3(from.x, targetY, from.z);
        float e = 0f;

        while (e < dur)
        {
            float t = e / dur;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(from, to, t);
            e += Time.deltaTime;
            yield return null;
        }
        transform.position = to;
    }

    IEnumerator RestoreRotation(Quaternion target, float dur)
    {
        Quaternion from = transform.rotation;
        float e = 0f;

        while (e < dur)
        {
            float t = e / dur;
            t = t * t * (3f - 2f * t);
            transform.rotation = Quaternion.Slerp(from, target, t);
            e += Time.deltaTime;
            yield return null;
        }
        transform.rotation = target;
    }

}
#endif
