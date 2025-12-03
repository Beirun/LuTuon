using UnityEngine;
using System.Collections; // Required for Coroutines

public class ImageEntranceAnimator : MonoBehaviour
{
    [Header("UI Elements to Animate")]
    [SerializeField] private RectTransform imageRectTransform; // Assign the RectTransform of the image
    [SerializeField] private RectTransform buttonRectTransform; // Assign the RectTransform of the button
    [SerializeField] private CanvasGroup textCanvasGroup; // Assign the RectTransform of the button
    [SerializeField] private CanvasGroup buttonCanvasGroup; // Assign the CanvasGroup of the button (for fading)

    [Header("Animation Settings - Image")]
    [SerializeField] private float imageMoveDuration = 1.0f; // Duration of the image's upward movement
    [SerializeField] private float imageMoveDistance = 100f; // Distance the image moves upward in pixels

    [Header("Animation Settings - Button")]
    [SerializeField] private float buttonMoveDuration = 1.0f; // Duration of the button's upward movement
    [SerializeField] private float buttonMoveDistance = 100f; // Distance the button moves upward in pixels
    [SerializeField] private float buttonDelay = 0.5f; // Optional: Delay before the button animation starts
    [SerializeField] private bool buttonFadeIn = true; // New: Whether the button should fade in

    void Start()
    {
        // Start the image animation immediately
        if (imageRectTransform != null)
        {
            StartCoroutine(MoveRectTransformUpward(imageRectTransform, imageMoveDistance, imageMoveDuration, 0f, null, null)); // No delay for image, no CanvasGroup
        }
        else
        {
            Debug.LogWarning("Image RectTransform not assigned to ImageEntranceAnimator script on " + gameObject.name);
        }

        // Start the button animation with an optional delay and fade
        if (buttonRectTransform != null)
        {
            // If fading in, ensure initial alpha is 0
            if (buttonFadeIn && buttonCanvasGroup != null)
            {
                buttonCanvasGroup.alpha = 0f;
            }
            if(textCanvasGroup != null)
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

    /// <summary>
    /// Coroutine to animate a RectTransform upward, with optional fading.
    /// </summary>
    /// <param name="rectTransform">The RectTransform to animate.</param>
    /// <param name="moveDistance">The distance in pixels to move upward.</param>
    /// <param name="duration">The duration of the animation in seconds.</param>
    /// <param name="delay">Optional: Delay before starting the animation.</param>
    /// <param name="canvasGroup">Optional: CanvasGroup for fading the element. If null, no fade occurs.</param>
    private IEnumerator MoveRectTransformUpward(RectTransform rectTransform, float moveDistance, float duration, float delay, CanvasGroup canvasGroup, CanvasGroup textCanvas)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay); // Wait for the specified delay
        }

        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 endPos = startPos + new Vector3(0, moveDistance, 0); // Move upward

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration; // Normalized time (0 to 1)

            // Animate position
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);

            // Animate alpha if CanvasGroup is provided and fading is enabled for button
            if (canvasGroup != null && buttonFadeIn)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null; // Wait for the next frame
        }

        // Ensure the element ends exactly at the target position
        rectTransform.anchoredPosition = endPos;

        // Ensure final alpha is 1 if fading
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
            float t = timer / duration; // Normalized time (0 to 1)

            // Animate alpha if CanvasGroup is provided and fading is enabled for button
            if (canvasGroup != null && buttonFadeIn)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null; // Wait for the next frame
        }
        if (canvasGroup != null && buttonFadeIn)
        {
            canvasGroup.alpha = 1f;
        }
    }
}