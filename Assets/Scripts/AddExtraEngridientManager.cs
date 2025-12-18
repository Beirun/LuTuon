using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AddExtraEngridientManager : MonoBehaviour
{
    float duration = 0.25f;
    float elapsed;

    public GameObject AddIngredientLayout;
    public Button button;

    bool isOpen;
    DragManager dragManager;
    private Animator blackOverlayAnimator;
    public GameObject blackOverlay;

    private void Start()
    {
        dragManager = FindFirstObjectByType<DragManager>();
        isOpen = false;
        if (blackOverlay != null)
        {
            blackOverlayAnimator = blackOverlay.GetComponent<Animator>();
            if (blackOverlay.GetComponent<CanvasGroup>() == null)
                blackOverlay.AddComponent<CanvasGroup>();
            blackOverlay.SetActive(false);
        }
    }
    void OnEnable()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        if (isOpen) CloseLayout();
        else OpenLayout();

        if(isOpen) dragManager.RestoreDraggingState();
        else dragManager.DisableAllDragging();
        isOpen = !isOpen;
    }

    public void OpenLayout()
    {
        StopAllCoroutines();
        elapsed = 0f;
        if (blackOverlay != null && !blackOverlay.activeSelf)
        {
            blackOverlay.SetActive(true);
            if (blackOverlayAnimator != null)
            {
                blackOverlay.GetComponent<CanvasGroup>().alpha = 0;
                blackOverlayAnimator.SetTrigger("FadeIn");
            }
        }
        StartCoroutine(StartOpenLayout());
    }

    public void CloseLayout()
    {
        StopAllCoroutines();
        elapsed = 0f;
        StartCoroutine(StartCloseLayout());
        StartCoroutine(StartFaeOut());
        
    }
    IEnumerator StartFaeOut()
    {
        if (blackOverlayAnimator != null) blackOverlayAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(0.5f);
        if (blackOverlay != null) blackOverlay.SetActive(false);
    }

    IEnumerator StartOpenLayout()
    {
        Vector3 startPos = AddIngredientLayout.transform.position;
        Vector3 endPos = startPos + new Vector3(535f, 0f, 0f);

        Quaternion startRot = button.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 0f, 180f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);

            AddIngredientLayout.transform.position = Vector3.Lerp(startPos, endPos, t);
            button.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        AddIngredientLayout.transform.position = endPos;
        button.transform.rotation = endRot;
    }

    IEnumerator StartCloseLayout()
    {
        Vector3 startPos = AddIngredientLayout.transform.position;
        Vector3 endPos = startPos + new Vector3(-535f, 0f, 0f);

        Quaternion startRot = button.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 0f, 180f);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);

            AddIngredientLayout.transform.position = Vector3.Lerp(startPos, endPos, t);
            button.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        AddIngredientLayout.transform.position = endPos;
        button.transform.rotation = endRot;
    }
}
