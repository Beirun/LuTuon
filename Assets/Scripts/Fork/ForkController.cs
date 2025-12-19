using UnityEngine;
using System.Collections;

#if UNITY_ANDROID || UNITY_IOS
public class ForkController : DragController
{
    [Header("Scramble")]
    public float scrambleDuration = 2f;
    public int loops = 3;
    public float ellipseA = 0.15f;
    public float ellipseB = 0.07f;
    public float depth = 0.15f;
    public GameObject eggBeat;
    public GameObject egg;
    public GameObject eggBowl;
    bool isScrambled = false;

    [Header("Flatten")]
    public float flattenDepth = 0.2f;
    public float flattenDuration = 2f;
    public GameObject flattenedEggplant;
    public GameObject eggplant;
    bool isFlattened = false;
    public override void EndDrag()
    {
        base.EndDrag();

        if (highlighted != null)
        {
            if (highlighted.name=="Eggplant" && !isFlattened) StartCoroutine(PlayFlatten(highlighted.transform.position));
            else if (highlighted.name == "EggBowl" && !isScrambled) StartCoroutine(PlayScramble(highlighted.transform.position));
        }
        else
            StartCoroutine(ReturnToStart());

        ClearHighlight();
    }


    IEnumerator PlayFlatten(Vector3 targetCenter)
    {
        isPerforming = true;
        Quaternion rot = Quaternion.Euler(267f, -280f, 9f);
        targetCenter += new Vector3(0.55f, 0.4f, -0.35f);
        float[] zOffsets = { -0.15f, 0f, 0.15f, 0.3f };

        Vector3 aboveEggplant = new Vector3(targetCenter.x, targetCenter.y, targetCenter.z + zOffsets[0]);
        yield return MoveTo(aboveEggplant, rot, 0.2f);

        for (int i = 0; i < zOffsets.Length; i++)
        {
            Vector3 downPos = new Vector3(targetCenter.x, targetCenter.y - flattenDepth, targetCenter.z + zOffsets[i]);
            
            yield return MoveTo(downPos, rot, flattenDuration * 0.5f);
            if (eggplant.activeInHierarchy && i == 1)
            {
                eggplant.SetActive(false);
                flattenedEggplant.SetActive(true);

            }
            if (i < zOffsets.Length - 1)
            {
                Vector3 nextAbove = new Vector3(targetCenter.x, targetCenter.y, targetCenter.z + zOffsets[i + 1]);
                yield return MoveTo(nextAbove, rot, flattenDuration * 0.5f);
            }
            else
            {
                Vector3 finalAbove = new Vector3(targetCenter.x, targetCenter.y, targetCenter.z + zOffsets[i]);
                yield return MoveTo(finalAbove, rot, flattenDuration * 0.5f);
            }
        }
        yield return ReturnToStart();
        isFinished = true;
        isFlattened = true;
    }



    IEnumerator MoveTo(Vector3 targetPos, Quaternion targetRot, float duration)
    {
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

        transform.position = targetPos;
        transform.rotation = targetRot;
    }

    IEnumerator PlayScramble(Vector3 targetCenter)
    {
        isPerforming = true;
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
        Vector3 eggBeatStartPos = eggBeat.transform.localPosition;
        Vector3 eggBeatEndPos = eggBeat.transform.localPosition + new Vector3(0f, 0.0f, 0.05f);
        eggBeat.SetActive(true);
        while (d < total)
        {
            float t = d / total;
            
            float ang = t * loops * twoPi;

            float y = Mathf.Cos(ang) * ellipseA;
            float z = Mathf.Sin(ang) * ellipseB;

            transform.position = center + new Vector3(-0.7f, -depth + y, z);
            float tt = t * t * (3f - 2f * t);
            eggBeat.transform.localPosition = Vector3.Lerp(eggBeatStartPos, eggBeatEndPos, tt);
            if(d > total /2 )
            {
                if (egg.activeInHierarchy) egg.SetActive(false);
                eggBowl.tag = "ScrambledEggBowl";
            }
            d += Time.deltaTime;
            yield return null;
        }


        yield return RestoreHeight(liftHeight, 0.2f);
        yield return RestoreRotation(startRot, 0.3f);
        yield return ReturnToStart();
        isFinished = true;
        isScrambled = true;
        highlightTags.Remove("EggBowl");
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
