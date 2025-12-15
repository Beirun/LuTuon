using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class EggplantManager : MonoBehaviour
{
    public bool isFinished = false;
    bool isModified = false;

    public EggplantController controller;
    public ForkController forkController;
    public GameObject eggplant;

    [Header("Eggplant Texture")]
    public Material[] peeledEggplantMaterials;
    public Material cookedEggPlantMaterial;
    public MeshRenderer mesh;
    public EggplantTouchManager touchManager;


    void Update()
    {
        if (controller.isPlaced && !isModified)
        {
            controller.enabled = false;
            StartCoroutine(StartProgressBar(eggplant, 5f));
            StartCoroutine(ChangeMaterial());
        }
    }

    Slider GetProgressBar()
    {
        GameObject template = Resources
            .FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(g =>
                g.name == "ProgressBarTemplate" &&
                g.scene.IsValid());

        if (!template)
        {
            Debug.LogError("ProgressBarTemplate not found (including inactive)");
            return null;
        }

        GameObject copy = Instantiate(template, template.transform.parent);
        copy.transform.localPosition = template.transform.localPosition;
        copy.transform.localRotation = template.transform.localRotation;
        copy.transform.localScale = template.transform.localScale;
        copy.SetActive(true);

        Slider progressBar = copy.GetComponent<Slider>();
        if (!progressBar)
        {
            Debug.LogError("ProgressBarTemplate has no Slider");
            Destroy(copy);
        }
        progressBar.gameObject.SetActive(false);
        progressBar.value = 0f;
        return progressBar;
    }



    IEnumerator StartProgressBar(GameObject gameObject,float duration, float offset = 0.9f)
    {
        Slider progressBar = GetProgressBar();
        Camera cam = Camera.main;
        Vector3 p = gameObject.transform.position - cam.transform.up * offset;
        Vector3 screen = cam.WorldToScreenPoint(p);
        if (screen.z < 0) screen = cam.WorldToScreenPoint(gameObject.transform.position);
        progressBar.gameObject.transform.position = screen;

        progressBar.gameObject.SetActive(true);
        progressBar.value = 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            progressBar.value = Mathf.Clamp01(t / duration);
            yield return null;
        }

        progressBar.value = 1f;
        progressBar.gameObject.SetActive(false);
        Destroy(progressBar.gameObject);
    }

    IEnumerator ChangeMaterial()
    {
        isModified = true;
        yield return new WaitForSeconds(5f);

        mesh.material = cookedEggPlantMaterial;

        controller.newTargetPos = new Vector3(-2.894f, 0.362f, 0.628f);
        controller.highlightTags.Remove("Grill");
        controller.highlightTags.Add("EggplantPlate");

        controller.startPos = controller.transform.position;
        controller.startRot = controller.transform.rotation;
        controller.isPlaced = false;
        controller.enabled = true;

        isFinished = true;
    }

    public void Peel()
    {
        StartCoroutine(StartPeeling());
    }

    IEnumerator StartPeeling()
    {
        touchManager.isPerforming = true;
        touchManager.HideButton();

        for (int i = 0; i < peeledEggplantMaterials.Length; i++)
        {
            mesh.material = peeledEggplantMaterials[i];

            yield return new WaitForSeconds(0.5f);
        }

        touchManager.isFinished = true;
        touchManager.isPerforming = false;
        eggplant.tag = "PeeledEggplant";
    }
}
