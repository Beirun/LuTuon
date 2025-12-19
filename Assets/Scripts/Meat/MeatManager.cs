using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeatManager : MonoBehaviour
{
    public bool isFinished = false;
    bool isModified = false;

    public GameObject meat;

    [Header("Meat Texture")]
    public Material halfCookedMaterial;
    public Material cookedMaterial;
    public MeshRenderer mesh;
    public MeatTouchManager touchManager;

    public ProgressBarManager progressBar;



    [Header("Parent Controller")]
    public MeatController controller;


    public int flipCounter = 0;
    // Update is called once per frame
    void Update()
    {
        if (controller.isPlaced && !isModified)
        {
            isModified = true;
            controller.enabled = false;

            progressBar.StartProgress(meat.transform, 5f);
        }


    }
    IEnumerator ChangeMaterial()
    {
        yield return new WaitForSeconds(0.2f);

        if (flipCounter == 0) mesh.material = halfCookedMaterial;
        else mesh.material = cookedMaterial;

        controller.newTargetPos =  new Vector3(0f, 0.15f, 0.3f);
        controller.highlightTags.Remove("Grill");
        controller.highlightTags.Add("Choppingboard");

        
        controller.isPlaced = false;
        isFinished = true;

        if(flipCounter == 1)
        {
            touchManager.isPerforming = true;    
            touchManager.enabled = false;
            controller.enabled = true;
            yield return new WaitForSeconds(0.4f);
            controller.startPos = controller.transform.position;
            controller.startRot = meat.transform.rotation;
        }
        flipCounter++;
    }

    public void Flip()
    {
        StartCoroutine(StartFlipping());
    }

    IEnumerator StartFlipping()
    {
        touchManager.isPerforming = true;
        touchManager.HideButton();
        float elapsed = 0f;
        Vector3 from = meat.transform.position;
        Vector3 to = from + new Vector3(0f, 0.5f, 0f);

        Coroutine rotRoutine = null;
        Coroutine changeRoutine = null;
        IEnumerator Rotate()
        {
            float dur = 0.4f;
            float t = 0f;

            Quaternion start = meat.transform.rotation;
            Quaternion target = start * Quaternion.Euler(180f, 0f, 0f); // rotate relative

            while (t < dur)
            {
                float n = t / dur;
                n = n * n * (3f - 2f * n); // smoothstep

                meat.transform.rotation = Quaternion.Slerp(start, target, n);

                if (changeRoutine == null)
                    changeRoutine = StartCoroutine(ChangeMaterial());

                t += Time.deltaTime;
                yield return null;
            }

        }


        while (elapsed < 0.4f)
        {
            float t = elapsed / 0.4f;
            t = t * t * (3f - 2f * t); // smoothstep

            Vector3 pos = Vector3.Lerp(from, to, t);
            meat.transform.position = pos;
            if (elapsed > 0.2f && rotRoutine == null)
            {
                rotRoutine = StartCoroutine(Rotate());
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0;
        while (elapsed < 0.4f)
        {
            float t = elapsed / 0.4f;
            t = t * t * (3f - 2f * t); // smoothstep

            Vector3 pos = Vector3.Lerp(to, from, t);
            meat.transform.position = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        touchManager.isPerforming = false;
        progressBar.StartProgress(meat.transform, 5f);
    }

}

