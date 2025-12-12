using UnityEngine;
using System.Collections; 

public class ImageEntranceAnimator : MonoBehaviour
{
    [Header("UI Elements to Animate")]
    [SerializeField] private RectTransform imageRectTransform; 
    [SerializeField] private RectTransform buttonRectTransform;
    [SerializeField] private CanvasGroup textCanvasGroup; 
    [SerializeField] private CanvasGroup buttonCanvasGroup; 

    [Header("Animation Settings - Image")]
    [SerializeField] private float imageMoveDuration = 1.0f;
    [SerializeField] private float imageMoveDistance = 100f; 

    [Header("Animation Settings - Button")]
    [SerializeField] private float buttonMoveDuration = 1.0f; 
    [SerializeField] private float buttonMoveDistance = 100f; 
    [SerializeField] private float buttonDelay = 0.5f;
    [SerializeField] private bool buttonFadeIn = true;

    public void StartEntranceAnimation()
    {
        if (imageRectTransform != null)
        {
            StartCoroutine(MoveRectTransformUpward(imageRectTransform, imageMoveDistance, imageMoveDuration, 0f, null, null));
        }
        else
        {
            Debug.LogWarning("Image RectTransform not assigned to ImageEntranceAnimator script on " + gameObject.name);
        }

        if (buttonRectTransform != null)
        {
            if (buttonFadeIn && buttonCanvasGroup != null)
            {
                buttonCanvasGroup.alpha = 0f;
            }
            if (textCanvasGroup != null)
            {
                textCanvasGroup.alpha = 0f;
            }
            StartCoroutine(MoveRectTransformUpward(buttonRectTransform, buttonMoveDistance, buttonMoveDuration, buttonDelay, buttonCanvasGroup, textCanvasGroup));
        }
        else
        {
            Debug.LogWarning("Button RectTransform not assigned to ImageEntranceAnimator script on " + gameObject.name);
        }
    }

    private IEnumerator MoveRectTransformUpward(RectTransform rectTransform, float moveDistance, float duration, float delay, CanvasGroup canvasGroup, CanvasGroup textCanvas)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay); 
        }

        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 endPos = startPos + new Vector3(0, moveDistance, 0); 

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration; 

            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);

            if (canvasGroup != null && buttonFadeIn)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }

        rectTransform.anchoredPosition = endPos;

        if (canvasGroup != null && buttonFadeIn)
        {
            canvasGroup.alpha = 1f;
        }
        yield return FadeText(textCanvas);
    }

    private IEnumerator FadeText(CanvasGroup canvasGroup)
    {
        float timer = 0f;
        float duration = 0.5f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration; 

            if (canvasGroup != null && buttonFadeIn)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }
        if (canvasGroup != null && buttonFadeIn)
        {
            canvasGroup.alpha = 1f;
        }
    }
}