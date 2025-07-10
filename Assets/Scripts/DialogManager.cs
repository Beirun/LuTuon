using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // Required for Dictionary and HashSet

public class DialogManager : MonoBehaviour
{
    [System.Serializable]
    public class ManagedDialogPanel
    {
        public GameObject panelGameObject;
        // You might add other panel-specific data here if needed in the future
    }

    // Public variable to assign the black overlay in the Inspector
    public GameObject blackOverlay;

    // Public list to assign all dialog panels you want to manage in the Inspector
    public List<ManagedDialogPanel> managedDialogPanels;

    // References to the Animators
    private Animator blackOverlayAnimator;

    // Internal dictionary for quick lookup of panels by name
    private Dictionary<string, PanelData> allPanels;

    // Internal class to hold GameObject and its Animator reference
    private class PanelData
    {
        public GameObject gameObject;
        public Animator animator;
    }

    // Tracks which dialog panels are currently active
    private HashSet<string> activeDialogNames;

    // Public variable to set animation duration (should match your animation clip lengths)
    public float animationDuration = 0.5f;

    void Awake()
    {
        allPanels = new Dictionary<string, PanelData>();
        activeDialogNames = new HashSet<string>();

        // Initialize the black overlay animator
        if (blackOverlay != null)
        {
            blackOverlayAnimator = blackOverlay.GetComponent<Animator>();
            if (blackOverlay.GetComponent<CanvasGroup>() == null)
            {
                blackOverlay.AddComponent<CanvasGroup>();
            }
            blackOverlay.SetActive(false); // Ensure overlay starts hidden
        }

        // Populate the dictionary with all managed dialog panels
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

                managedPanel.panelGameObject.SetActive(false); // Ensure panels start hidden
            }
        }
    }

    /// <summary>
    /// Opens a specific dialog panel by its GameObject name.
    /// </summary>
    /// <param name="panelName">The name of the dialog panel GameObject to open.</param>
    public void OpenDialog(string panelName)
    {
        if (allPanels.TryGetValue(panelName, out PanelData panelData))
        {
            // Activate the black overlay if it's not already active
            if (blackOverlay != null && !blackOverlay.activeSelf)
            {
                blackOverlay.SetActive(true);
                if (blackOverlayAnimator != null)
                {
                    // Ensure the alpha starts at 0 for the fade-in animation
                    blackOverlay.GetComponent<CanvasGroup>().alpha = 0;
                    blackOverlayAnimator.SetTrigger("FadeIn");
                }
            }

            // Activate and animate the specific dialog panel
            if (!panelData.gameObject.activeSelf) // Only open if not already open
            {
                panelData.gameObject.SetActive(true);
                if (panelData.animator != null)
                {
                    // Ensure scale starts at 0 (or small) for the zoom-in animation
                    panelData.gameObject.transform.localScale = Vector3.zero;
                    panelData.animator.SetTrigger("ZoomIn");
                }
                activeDialogNames.Add(panelName); // Add to active list
            }
        }
        else
        {
            Debug.LogWarning($"Dialog panel with name '{panelName}' not found in managed list.");
        }
    }

    /// <summary>
    /// Closes a specific dialog panel by its GameObject name.
    /// </summary>
    /// <param name="panelName">The name of the dialog panel GameObject to close.</param>
    public void CloseDialog(string panelName)
    {
        if (activeDialogNames.Contains(panelName) && allPanels.TryGetValue(panelName, out PanelData panelData))
        {
            StartCoroutine(CloseDialogWithAnimation(panelData, panelName));
        }
        else if (!activeDialogNames.Contains(panelName))
        {
            Debug.LogWarning($"Attempted to close dialog '{panelName}' but it was not marked as active.");
        }
        else
        {
            Debug.LogWarning($"Attempted to close dialog '{panelName}' but it was not found in managed list.");
        }
    }

    private IEnumerator CloseDialogWithAnimation(PanelData panelData, string panelName)
    {
        // Trigger zoom out animation for the specific panel
        if (panelData.animator != null)
        {
            panelData.animator.SetTrigger("ZoomOut");
        }

        // Remove from active list immediately, even if animation is playing
        activeDialogNames.Remove(panelName);

        // Determine if black overlay should also fade out
        bool shouldFadeOutOverlay = (activeDialogNames.Count == 0);

        if (shouldFadeOutOverlay && blackOverlayAnimator != null)
        {
            blackOverlayAnimator.SetTrigger("FadeOut");
        }

        // Wait for animations to complete
        yield return new WaitForSeconds(animationDuration);

        // After animations, hide the specific panel
        if (panelData.gameObject != null)
        {
            panelData.gameObject.SetActive(false);
        }

        // If no other dialogs are active, hide the black overlay
        if (shouldFadeOutOverlay && blackOverlay != null)
        {
            blackOverlay.SetActive(false);
        }
    }

    /// <summary>
    /// Closes all currently open dialogs.
    /// </summary>
    public void CloseAllDialogs()
    {
        // Create a list of currently active dialogs to avoid modifying collection during iteration
        List<string> currentlyActive = new List<string>(activeDialogNames);
        foreach (string panelName in currentlyActive)
        {
            // Call CloseDialog for each active panel
            CloseDialog(panelName);
        }
    }
}