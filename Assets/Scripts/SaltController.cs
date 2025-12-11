using UnityEngine;
using System.Collections;

public class SaltController : DragController
{
    [Header("Particles")]

    public ParticleSystem ps;
    public ParticleSystem ps2;


    [Header("Other")]
    public Vector3 highlightOffset = new Vector3(-1.9f, 1.35f, 0f);
    public LidController lid;



    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && (lid == null || !lid.isClose))
        {
            Vector3 target = highlighted.transform.position + highlightOffset;
            Quaternion targetRot = Quaternion.Euler(-25f, 0f, -90f);
            StartCoroutine(MoveThenAnimate(target, targetRot, 0.5f));
        }
        else StartCoroutine(ReturnToStart());
        ClearHighlight();
    }

    IEnumerator MoveThenAnimate(Vector3 targetPos, Quaternion targetRot, float moveDuration)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float t = 0f;

        // Move to target first
        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        // After reaching target, perform arcs
        yield return AnimateShake(targetPos, targetRot, 0.5f);
    }

    IEnumerator AnimateShake(Vector3 basePos, Quaternion baseRot, float duration)
    {
        isPerforming = true;

        IEnumerator Shake()
        {
            Quaternion startRot = Quaternion.Euler(-25f, 0f, -90f);
            Quaternion endRot = Quaternion.Euler(25f, 0f, -90f);
            float t = 0f;
            while (t < 0.75f)
            {
                t += Time.deltaTime / duration;
                float x = basePos.x + Mathf.Lerp(0f, 1f, t); // forward arc
                float y = basePos.y + Mathf.Sin(t * Mathf.PI) * 0.5f; // arc height
                transform.position = new Vector3(x, y, basePos.z);

                transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
                yield return null;
            }

            t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime / duration;
                float x = basePos.x + Mathf.Lerp(1f, 0.8f, t); // backward arc
                float y = basePos.y + Mathf.Lerp(0f, 0.125f, t);
                transform.position = new Vector3(x, y, basePos.z);

                yield return null;
            }

            t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime / duration;
                float x = basePos.x + Mathf.Lerp(0.8f, 1f, t); // forward arc
                float y = basePos.y + Mathf.Lerp(0.125f, 0f, t);
                transform.position = new Vector3(x, y, basePos.z);
                ps2.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps2.Play();
                yield return null;
            }



            t = 0f;
            while (t < 0.75f)
            {
                t += Time.deltaTime / duration;
                float x = basePos.x + Mathf.Lerp(1f, 0f, t); // backward arc
                float y = basePos.y + Mathf.Sin(t * Mathf.PI) * 0.5f;
                transform.position = new Vector3(x, y, basePos.z);

                transform.rotation = Quaternion.Slerp(endRot, startRot, t);
                yield return null;
            }

            transform.position = basePos;
            transform.rotation = baseRot;
        }

        yield return Shake();
        yield return new WaitForSeconds(0.1f);
        yield return Shake();
        yield return new WaitForSeconds(0.1f);
        yield return Shake();
        yield return ReturnToStart();
        isFinished = true;
    }

}
