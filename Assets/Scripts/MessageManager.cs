using System.Collections;
using UnityEngine;
using TMPro;

public class MessageManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The parent object containing the Image and CanvasGroup")]
    public RectTransform uiTransform;
    [Tooltip("The CanvasGroup component for fading")]
    public CanvasGroup canvasGroup;
    [Tooltip("The text component")]
    public TMP_Text messageText;

    [Header("Animation Settings")]
    public float fadeInTime = 0.5f;
    public float stayTime = 2.0f;
    public float fadeOutTime = 0.5f;

    [Header("Position Settings")]
    public float startOffsetY = -100f;
    public float endOffsetY = 100f;

    private Coroutine currentAnimation;
    private Vector2 initialAnchorPosition;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        initialAnchorPosition = uiTransform.anchoredPosition;

    }

    public void ShowMessage(string message)
    {

        messageText.text = message;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(AnimatePopup());
    }

    private IEnumerator AnimatePopup()
    {
        canvasGroup.blocksRaycasts = true;

        // --- PHASE 1: PREPARE ---
        float timer = 0f;
        Vector2 startPos = initialAnchorPosition + new Vector2(0, startOffsetY);
        Vector2 centerPos = initialAnchorPosition;
        Vector2 endPos = initialAnchorPosition + new Vector2(0, endOffsetY);

        uiTransform.anchoredPosition = startPos;
        canvasGroup.alpha = 0f;

        // --- PHASE 2: FADE IN & MOVE UP ---
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeInTime;

            uiTransform.anchoredPosition = Vector2.Lerp(startPos, centerPos, EaseOutBack(progress));
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

        uiTransform.anchoredPosition = centerPos;
        canvasGroup.alpha = 1f;

        // --- PHASE 3: WAIT ---
        yield return new WaitForSeconds(stayTime);

        // --- PHASE 4: FADE OUT & MOVE UP ---
        timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutTime;

            uiTransform.anchoredPosition = Vector2.Lerp(centerPos, endPos, progress);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

    }

    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }
}