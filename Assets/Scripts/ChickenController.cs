using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenController : DragController
{
    List<Rigidbody> rbs = new List<Rigidbody>();
    Dictionary<Rigidbody, Vector3> homePositions = new Dictionary<Rigidbody, Vector3>();

    [Header("Water Object")]
    public GameObject water;

    [Header("Pot Settings")]
    public Transform potCenter; // assign your pot or water center
    public float potRadius = 0.67f;

    [Header("Floating Settings")]
    public float waterSurfaceOffset = -0.2f; // initial depth offset (sink a bit below surface)
    public float floatRadius = 0.25f;
    public float floatStrength = 2f;
    public float driftSpeed = 0.5f;
    public float bobAmplitude = 0.05f;
    public float bobSpeed = 1f;


    bool floating;

    [Header("Timer Controller")]
    public TimerController timerController;
    public bool isTutorial = false; 
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

    public override void Update()
    {
        base.Update();
        if (isInPot && !floating && water != null && water.activeInHierarchy && water.transform.position.y > 1f)
        {
            EnablePhysicsOnChildren(transform);
            StartFloating();
        }
    }

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && !lid.isClose)
        {
            Vector3 p = highlighted.transform.position;
            p.y = water.transform.position.y + waterSurfaceOffset;
            if (water.transform.position.y < 1f) p = highlighted.transform.position + new Vector3(0f, 0.2f, 0f);
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

        if (isDragging)
        {
            EnablePhysicsOnChildren(transform);
            isFinished = true;
            if(!isTutorial){
                timerController.enabled = true;
                timerController.StartTimer(5);
            }
        }
    }

    void EnablePhysicsOnChildren(Transform p)
    {
        isInPot = true;
        foreach (Transform c in p)
        {
            var rb = c.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = water.transform.position.y > 1f;
                rb.useGravity = water.transform.position.y < 1f;
            }
            if (c.childCount > 0) EnablePhysicsOnChildren(c);
        }
    }

    void StartFloating()
    {
        if (floating) return;
        floating = true;

        foreach (var rb in rbs)
        {
            // assign a random "home" point inside the pot
            Vector2 randomCircle = Random.insideUnitCircle * (potRadius * 0.8f);
            Vector3 home = new Vector3(
                potCenter.position.x + randomCircle.x,
                water.transform.position.y + waterSurfaceOffset,
                potCenter.position.z + randomCircle.y
            );
            homePositions[rb] = home;

            StartCoroutine(FloatAndDrift(rb));
        }
    }

    IEnumerator FloatAndDrift(Rigidbody rb)
    {
        Vector3 home = homePositions[rb];
        float seedX = Random.Range(0f, 100f);
        float seedZ = Random.Range(0f, 100f);
        float seedY = Random.Range(0f, 100f);

        while (floating)
        {
            float t = Time.time * driftSpeed;

            float offsetX = (Mathf.PerlinNoise(seedX, t) - 0.5f) * 2f * floatRadius;
            float offsetZ = (Mathf.PerlinNoise(seedZ, t + 100f) - 0.5f) * 2f * floatRadius;
            float offsetY = Mathf.Sin((t + seedY) * bobSpeed) * bobAmplitude;

            Vector3 target = new Vector3(
                home.x + offsetX,
                water.transform.position.y + waterSurfaceOffset + offsetY,
                home.z + offsetZ
            );

            // stay inside pot radius
            Vector3 centerXZ = new Vector3(potCenter.position.x, 0, potCenter.position.z);
            Vector3 posXZ = new Vector3(target.x, 0, target.z);
            Vector3 dir = posXZ - centerXZ;
            if (dir.sqrMagnitude > potRadius * potRadius)
                target = centerXZ + dir.normalized * potRadius + Vector3.up * target.y;

            rb.MovePosition(Vector3.Lerp(rb.position, target, Time.deltaTime * floatStrength));
            yield return null;
        }

    }
}
