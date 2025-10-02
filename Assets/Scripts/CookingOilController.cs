using UnityEngine;
using System.Collections;

public class CookingOilController : DragController
{
    [Header("Water Objects")]
    public GameObject pouringWater;

    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted != null)
        {
            Vector3 targetPos = highlighted.transform.position + new Vector3(-2.1f, 1.35f, 0f);
            Quaternion targetRot = Quaternion.Euler(-25f, 90f, -90f);

            StartCoroutine(AnimatePouring(targetPos, targetRot, 0.5f));
        }
        ClearHighlight();
    }

    IEnumerator AnimatePouring(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 fromPos = transform.position;
        Quaternion fromRot = transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(fromPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        pouringWater.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        pouringWater.SetActive(false);
        yield return ReturnToStart();
    }
}
