using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterController : DragController
{
    List<Rigidbody> rbs = new List<Rigidbody>();
    Dictionary<Rigidbody, Vector3> homePositions = new Dictionary<Rigidbody, Vector3>();

    [Header("Water Object")]
    public GameObject water;

    [Header("Pot Settings")]
    public Transform potCenter;
    public float potRadius = 0.67f;

    [Header("Floating Settings")]
    public float waterSurfaceOffset = 0.1f;
    public float floatRadius = 0.25f;
    public float floatStrength = 2f;
    public float driftSpeed = 0.5f;
    public float bobAmplitude = 0.05f;
    public float bobSpeed = 1f;

    bool floating;
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
            //p.y = water.transform.position.y + waterSurfaceOffset;
            //if (water.transform.position.y < 1f)
            //    p = highlighted.transform.position + new Vector3(0f, 0.2f, 0f);

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

        //yield return StartCoroutine(DisableAfterDelay(2f));
        isFinished = true;
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
}
