using UnityEngine;
using System.Collections;

public class ChoppedLemonController : DragController
{
    [Header("Parts")]
    public Transform leftPart;
    public Transform rightPart;

    public ParticleSystem leftParticles;
    public ParticleSystem rightParticles;

    [SerializeField] float squeezeScaleY = 0.5f;
    [SerializeField] float squeezeDuration = 0.25f;

    public LidController lid;
    public bool makeChildAfterPlacement = false;
    public bool isPlaced = false;
    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null && (lid == null || !lid.isClose))
        {
            Vector3 targetPos = highlighted.transform.position + new Vector3(0f, 1.325f, 0f);
            if (makeChildAfterPlacement && highlighted != null && !highlighted.CompareTag("Pan"))
            {
                transform.SetParent(highlighted.transform, true);
            }
            StartCoroutine(AnimatePlacement(targetPos, transform.rotation, 0.5f));
        }
        else
        {
            StartCoroutine(ReturnToStart());
        }
        ClearHighlight();
    }

    public IEnumerator AnimatePlacement(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        isPerforming = true;
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(fromPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0;
        Quaternion lFromRot = leftPart.transform.rotation;
        Quaternion rFromRot = rightPart.transform.rotation;
        Quaternion lToRot = Quaternion.Euler(180f, 0, 70f);
        Quaternion rToRot = Quaternion.Euler(0, 0, 70f);

        Vector3 lFromPos = leftPart.position;
        Vector3 rFromPos = rightPart.position;
        Vector3 lToPos = leftPart.position + new Vector3(-0.3f, 0f, 0.07f);
        Vector3 rToPos = rightPart.position + new Vector3(0.33f, 0f, -0.3f);
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            leftPart.rotation = Quaternion.Slerp(lFromRot, lToRot, t);
            leftPart.position = Vector3.Lerp(lFromPos, lToPos, t);

            rightPart.rotation = Quaternion.Slerp(rFromRot, rToRot, t);
            rightPart.position = Vector3.Lerp(rFromPos, rToPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return AnimateSqueeze();

    }

    IEnumerator AnimateSqueeze()
    {
        if (leftPart == null || rightPart == null)
            yield break;

        Vector3 lStart = leftPart.localScale;
        Vector3 rStart = rightPart.localScale;

        Vector3 lTarget = new Vector3(lStart.x, squeezeScaleY, lStart.z);
        Vector3 rTarget = new Vector3(rStart.x, squeezeScaleY, rStart.z);

        leftParticles?.Play();
        rightParticles?.Play();

        float t = 0f;

        // squeeze down
        while (t < squeezeDuration)
        {
            float p = t / squeezeDuration;
            p = p * p * (3f - 2f * p);

            leftPart.localScale = Vector3.Lerp(lStart, lTarget, p);
            rightPart.localScale = Vector3.Lerp(rStart, rTarget, p);

            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;

        // release back
        while (t < squeezeDuration)
        {
            float p = t / squeezeDuration;
            p = p * p * (3f - 2f * p);

            leftPart.localScale = Vector3.Lerp(lTarget, lStart, p);
            rightPart.localScale = Vector3.Lerp(rTarget, rStart, p);

            t += Time.deltaTime;
            yield return null;
        }

        leftPart.localScale = lStart;
        rightPart.localScale = rStart;

        Vector3 fromPos = transform.position;
        float elapsed = 0f;

        while (elapsed < 0.75f)
        {
            t = elapsed / 0.75f;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(fromPos, fromPos + new Vector3(0,0,4.5f), t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        gameObject.SetActive(false);
        isPlaced = true;
        isFinished = true;
        isPerforming = false;
        isDragging = false;
    }

}
