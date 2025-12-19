using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
public class EggController : DragController
{

    [Header("Fork Controller")]
    public ForkController controller; 

    [Header("Egg Parts")]
    public Transform leftShell;
    public Transform rightShell;
    public GameObject rawEgg;
    public GameObject bowl;

    [Header("Settings")]
    public float openSpeed = 0.5f;
    public float separationDistance = 0.15f; 
    public float separationAngle = 45f;   

    private Vector3 lStartPos, rStartPos;
    private Quaternion lStartRot, rStartRot;

    public override void Awake()
    {
        base.Awake();
        if (leftShell) { lStartPos = leftShell.localPosition; lStartRot = leftShell.localRotation; }
        if (rightShell) { rStartPos = rightShell.localPosition; rStartRot = rightShell.localRotation; }
    }

    public override void EndDrag()
    {
        base.EndDrag();

        if (highlighted != null)
        {
            StartCoroutine(PlayCrackEgg(highlighted.transform.position));
        }
        else
        {
            StartCoroutine(ReturnToStart());
        }

        ClearHighlight();
    }

    IEnumerator PlayCrackEgg(Vector3 targetCenter)
    {
        isPerforming = true;

        Vector3 crackPos = targetCenter + new Vector3(0f, 1.5f, 0f);


        yield return MoveTo(crackPos, 0.3f);

        yield return AnimateShells(true);

        yield return new WaitForSeconds(0.5f);

        yield return AnimateShells(false);

        crackPos = targetCenter + new Vector3(0f, 0f, -3f);
        yield return MoveTo(crackPos, 0.75f);
        gameObject.SetActive(false);
        bowl.tag = "EggBowl";
        isPerforming = false;
        isDragging = false;
        isFinished = true;
    }

    IEnumerator AnimateShells(bool isOpen)
    {
        float elapsed = 0f;
        float duration = openSpeed;

        Vector3 lTargetPos = isOpen ? lStartPos + (Vector3.left * separationDistance) : lStartPos; 
        Vector3 rTargetPos = isOpen ? rStartPos + (Vector3.right * separationDistance) : rStartPos; 

        Quaternion lTargetRot = isOpen ? lStartRot * Quaternion.Euler(-separationAngle, 0, 0) : lStartRot;
        Quaternion rTargetRot = isOpen ? rStartRot * Quaternion.Euler(separationAngle, 0, 0) : rStartRot;

        Vector3 lCurrentPos = leftShell.localPosition;
        Vector3 rCurrentPos = rightShell.localPosition;
        Quaternion lCurrentRot = leftShell.localRotation;
        Quaternion rCurrentRot = rightShell.localRotation;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); 

            if (leftShell)
            {
                leftShell.localPosition = Vector3.Lerp(lCurrentPos, lTargetPos, t);
                leftShell.localRotation = Quaternion.Slerp(lCurrentRot, lTargetRot, t);
            }
            if (rightShell)
            {
                rightShell.localPosition = Vector3.Lerp(rCurrentPos, rTargetPos, t);
                rightShell.localRotation = Quaternion.Slerp(rCurrentRot, rTargetRot, t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (leftShell) leftShell.SetLocalPositionAndRotation(lTargetPos, lTargetRot);
        if (rightShell) rightShell.SetLocalPositionAndRotation(rTargetPos, rTargetRot);
        if (isOpen)
        {
            rawEgg.SetActive(true);
        }
        
    }

    IEnumerator MoveTo(Vector3 targetPos, float duration)
    {
        Vector3 fromPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(fromPos, targetPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }
}
#endif