using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterController : DragController
{
    List<Rigidbody> rbs = new List<Rigidbody>();

    [Header("Water Object")]
    public GameObject water;


    public LidController lid;

    public override void Start()
    {
        base.Start();
        foreach (Transform c in transform)
        {
            var rb = c.GetComponent<Rigidbody>();
            if (rb != null) rbs.Add(rb);
            foreach (Transform gc in c)
            {
                var grb = gc.GetComponent<Rigidbody>();
                if (grb != null) rbs.Add(grb);
            }
        }
        StartCoroutine(InitPhysics());
    }

    IEnumerator InitPhysics()
    {
        yield return new WaitForSeconds(0.15f);
        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }



    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && (lid == null || !lid.isClose))
        {
            Vector3 p = highlighted.transform.position;

            StartCoroutine(AnimatePlacement(p, transform.rotation, 0.5f));
        }
        else
        {
            StartCoroutine(ReturnToStart());
        }
        ClearHighlight();
    }

    public IEnumerator AnimatePlacement(Vector3 targetPos, Quaternion targetRot, float duration, bool isDragging = true)
    {
        isPerforming = true;
        Vector3 a = transform.position;
        Quaternion b = transform.rotation;
        float t0 = 0f;

        while (t0 < duration)
        {
            float t = t0 / duration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(a, targetPos, t);
            transform.rotation = Quaternion.Slerp(b, targetRot, t);
            t0 += Time.deltaTime;
            yield return null;
        }

        if (isDragging)
            EnablePhysicsOnChildren(transform);

        isPerforming = false;
        this.isDragging = false;

          StartCoroutine(AnimateSubmerge(0.69f, 3.5f));
        yield return StartCoroutine(AnimateWaterLevel(0.9805f, 3.5f));
    }

    void EnablePhysicsOnChildren(Transform p)
    {
        isInPot = true;
        foreach (Transform c in p)
        {
            var rb = c.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            if (c.childCount > 0)
                EnablePhysicsOnChildren(c);
        }
    }
    IEnumerator AnimateWaterLevel(float targetPosY, float duration)
    {
        water.SetActive(true);

        Vector3 fromPosition = water.transform.position;
        Vector3 toPosition = new Vector3(fromPosition.x, targetPosY, fromPosition.z);

        float elapsedTime = 0f;


        while (elapsedTime < duration)
        {
            float rawT = elapsedTime / duration;
            float smoothT = rawT * rawT * (3f - 2f * rawT);

            if (water.transform.position.y < 1f)
            {
                water.transform.position = Vector3.Lerp(fromPosition, toPosition, smoothT);
            }


            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isFinished = true;
    }
    IEnumerator AnimateSubmerge(float targetPosY, float duration)
    {

        Vector3 fromPosition = gameObject.transform.position;
        Vector3 toPosition = new Vector3(fromPosition.x, targetPosY, fromPosition.z);

        float elapsedTime = 0f;


        while (elapsedTime < duration)
        {
            float rawT = elapsedTime / duration;
            float smoothT = rawT * rawT * (3f - 2f * rawT);

            gameObject.transform.position = Vector3.Lerp(fromPosition, toPosition, smoothT);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isFinished = true;
    }
}
