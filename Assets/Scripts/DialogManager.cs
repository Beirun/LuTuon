using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogManager : MonoBehaviour
{

    [System.Serializable]
    public class ManagedDialogPanel
    {
        public GameObject panelGameObject;
    }

    public GameObject blackOverlay;
    public List<ManagedDialogPanel> managedDialogPanels;
    private Animator blackOverlayAnimator;
    private Dictionary<string, PanelData> allPanels;
    private HashSet<string> activeDialogNames;
    public float animationDuration = 0.5f;

    private class PanelData
    {
        public GameObject gameObject;
        public Animator animator;
    }

    void Awake()
    {
        allPanels = new Dictionary<string, PanelData>();
        activeDialogNames = new HashSet<string>();

        if (blackOverlay != null)
        {
            blackOverlayAnimator = blackOverlay.GetComponent<Animator>();
            if (blackOverlay.GetComponent<CanvasGroup>() == null)
                blackOverlay.AddComponent<CanvasGroup>();
            blackOverlay.SetActive(false);
        }

        foreach (var managedPanel in managedDialogPanels)
        {
            if (managedPanel.panelGameObject != null)
            {
                string panelName = managedPanel.panelGameObject.name;
                PanelData data = new PanelData
                {
                    gameObject = managedPanel.panelGameObject,
                    animator = managedPanel.panelGameObject.GetComponent<Animator>()
                };
                allPanels.Add(panelName, data);
                managedPanel.panelGameObject.SetActive(false);
            }
        }
    }

    public void OpenDialog(string panelName)
    {
        if (allPanels.TryGetValue(panelName, out PanelData panelData))
        {
            if (blackOverlay != null && !blackOverlay.activeSelf)
            {
                blackOverlay.SetActive(true);
                if (blackOverlayAnimator != null)
                {
                    blackOverlay.GetComponent<CanvasGroup>().alpha = 0;
                    blackOverlayAnimator.SetTrigger("FadeIn");
                }
            }
            if (!panelData.gameObject.activeSelf)
            {
                panelData.gameObject.SetActive(true);
                if (panelData.animator != null)
                {
                    panelData.gameObject.transform.localScale = Vector3.zero;
                    panelData.animator.SetTrigger("ZoomIn");
                }
                activeDialogNames.Add(panelName);
            }
        }
        else Debug.LogWarning($"Dialog panel with name '{panelName}' not found in managed list.");
    }

    public void CloseDialog(string panelName)
    {
        if (activeDialogNames.Contains(panelName) && allPanels.TryGetValue(panelName, out PanelData panelData))
            StartCoroutine(CloseDialogWithAnimation(panelData, panelName));
        else if (!activeDialogNames.Contains(panelName))
            Debug.LogWarning($"Attempted to close dialog '{panelName}' but it was not marked as active.");
        else
            Debug.LogWarning($"Attempted to close dialog '{panelName}' but it was not found in managed list.");
    }

    private IEnumerator CloseDialogWithAnimation(PanelData panelData, string panelName)
    {
        if (panelData.animator != null) panelData.animator.SetTrigger("ZoomOut");
        activeDialogNames.Remove(panelName);
        bool shouldFadeOutOverlay = (activeDialogNames.Count == 0);
        if (shouldFadeOutOverlay && blackOverlayAnimator != null) blackOverlayAnimator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(animationDuration);
        if (panelData.gameObject != null) panelData.gameObject.SetActive(false);
        if (shouldFadeOutOverlay && blackOverlay != null) blackOverlay.SetActive(false);
    }

    public void CloseAllDialogs()
    {
        List<string> currentlyActive = new List<string>(activeDialogNames);
        bool isFeedbackActive = activeDialogNames.Contains("Feedback");
        bool isEditAvatarActive = activeDialogNames.Contains("Edit Avatar");
        foreach (string panelName in currentlyActive)
        {
            if (isFeedbackActive && panelName == "Feedback")
            {
                CloseDialogWithoutOverlay(panelName);
                OpenDialog("Settings");
            }
            else if (isEditAvatarActive && panelName == "Edit Avatar")
            {
                CloseDialogWithoutOverlay(panelName);
                OpenDialog("Profile");
            }
            else CloseDialog(panelName);
        }
    }

    public void CloseDialogWithoutOverlay(string panelName)
    {
        if (activeDialogNames.Contains(panelName) && allPanels.TryGetValue(panelName, out PanelData panelData))
            StartCoroutine(CloseDialogWithAnimationWithoutOverlay(panelData, panelName));
        else if (!activeDialogNames.Contains(panelName))
            Debug.LogWarning($"Attempted to close dialog '{panelName}' without overlay, but it was not marked as active.");
        else
            Debug.LogWarning($"Attempted to close dialog '{panelName}' without overlay, but it was not found in managed list.");
    }

    private IEnumerator CloseDialogWithAnimationWithoutOverlay(PanelData panelData, string panelName)
    {
        if (panelData.animator != null) panelData.animator.SetTrigger("ZoomOut");
        activeDialogNames.Remove(panelName);
        yield return new WaitForSeconds(animationDuration);
        if (panelData.gameObject != null) panelData.gameObject.SetActive(false);
    }

}
