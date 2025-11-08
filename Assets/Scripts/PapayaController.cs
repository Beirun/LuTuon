using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PapayaController : DragController
{
    List<Rigidbody> rbs = new List<Rigidbody>();

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
        for (int i = 0; i < rbs.Count; i++)
        {
            var rb = rbs[i];
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null)
        {
            Vector3 p = highlighted.transform.position + new Vector3(0f, 0.125f, 0f);
            StartCoroutine(AnimatePlacement(p, transform.rotation, 0.5f));
        }
        ClearHighlight();
    }

    public IEnumerator AnimatePlacement(Vector3 targetPos, Quaternion targetRot, float duration, bool isDragging = true)
    {
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

        if (isDragging) EnablePhysicsOnChildren(transform);
    }

    void EnablePhysicsOnChildren(Transform p)
    {
        foreach (Transform c in p)
        {
            var rb = c.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            if (c.childCount > 0) EnablePhysicsOnChildren(c);
        }
    }
}
