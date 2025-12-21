using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EggBowlController : DragController
{

    public GameObject eggPlant;


    public override void EndDrag()
    {
        base.EndDrag();
        if (highlighted)
        {
            Vector3 pos = highlighted.transform.position + new Vector3(-0.3f, 1.8f, -1.42f);
            Quaternion rot = Quaternion.Euler(-165f, -180f, 180f);
            StartCoroutine(AnimatePouring(pos, rot, 0.5f));
        }
        else StartCoroutine(ReturnToStart());
        ClearHighlight();
    }

    IEnumerator AnimatePouring(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        isPerforming = true;
        Vector3 fp = transform.position;
        Quaternion fr = transform.rotation;
        float e = 0f;

        while (e < duration)
        {
            float t = e / duration;
            t = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(fp, targetPos, t);
            transform.rotation = Quaternion.Slerp(fr, targetRot, t);
            e += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        eggPlant.SetActive(true);
        gameObject.tag = "Bowl";
        DisableAllChildren();  
        yield return ReturnToStart();
        isFinished = true;
        isDisabled = true;

    }



    void DisableAllChildren()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform c = transform.GetChild(i);
            var t = c.GetComponent<ChoppedTomatoController>();
            var d = c.GetComponent<DicedOnionsController>();
            if(d != null) if(!d.isInPot) continue;
            if(t != null) if(!t.isPlaced) continue;
            c.gameObject.SetActive(false);
        }
    }
}
