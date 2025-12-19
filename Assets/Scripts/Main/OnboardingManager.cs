using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnboardingManager : MonoBehaviour
{
    public List<Sprite> onboardingSteps = new List<Sprite>();
    public GameObject nextButton;
    public GameObject previousButton;

    public Image onboardingPanel;
    public GameObject okButton;
    public DialogManager dialogManager;

    CanvasGroup nextCg;
    CanvasGroup prevCg;

    int i;
    Coroutine fadeNext;
    Coroutine fadePrev;

    void Start()
    {
        nextCg = nextButton.GetComponent<CanvasGroup>();
        prevCg = previousButton.GetComponent<CanvasGroup>();

        nextCg.alpha = 0;
        nextCg.interactable = false;
        nextCg.blocksRaycasts = false;

        prevCg.alpha = 0;
        prevCg.interactable = false;
        prevCg.blocksRaycasts = false;
        Debug.LogWarning("Account First Time Login: " + AccountManager.Instance.CurrentAccount.isFirstTimeLogin);
        if (AccountManager.Instance.CurrentAccount.isFirstTimeLogin)
        {
            OpenOnboarding();
            AccountManager.Instance.CurrentAccount.isFirstTimeLogin = false;
        }

    }

    public void OpenOnboarding()
    {
        i = 0;
        onboardingPanel.sprite = onboardingSteps[i];
        okButton.SetActive(i == onboardingSteps.Count - 1);
        dialogManager.OpenDialog("Onboarding");
        FadeIn(nextCg);
    }

    public void CloseOnboarding()
    {
        FadeOut(nextCg);
        FadeOut(prevCg);

        i = 0;
        dialogManager.CloseDialog("Onboarding");
    }

    public void Next()
    {
        if (onboardingSteps.Count == 0) return;
        if (i >= onboardingSteps.Count - 1) return;

        i++;
        okButton.SetActive(i == onboardingSteps.Count - 1);
        onboardingPanel.sprite = onboardingSteps[i];

        if (i == onboardingSteps.Count - 1)
            FadeOut(nextCg);
        else
            FadeIn(nextCg);

        if (i > 0)
            FadeIn(prevCg);
    }

    public void Previous()
    {
        if (onboardingSteps.Count == 0) return;
        if (i <= 0) return;

        i--;
        okButton.SetActive(i == onboardingSteps.Count - 1);
        onboardingPanel.sprite = onboardingSteps[i];

        if (i == 0)
            FadeOut(prevCg);
        else
            FadeIn(prevCg);

        if (i < onboardingSteps.Count - 1)
            FadeIn(nextCg);
    }

    void FadeIn(CanvasGroup cg)
    {
        if (cg == nextCg && fadeNext != null) StopCoroutine(fadeNext);
        if (cg == prevCg && fadePrev != null) StopCoroutine(fadePrev);

        var c = StartCoroutine(FadeRoutine(cg, 1f, true));
        if (cg == nextCg) fadeNext = c;
        if (cg == prevCg) fadePrev = c;
    }

    void FadeOut(CanvasGroup cg)
    {
        if (cg == nextCg && fadeNext != null) StopCoroutine(fadeNext);
        if (cg == prevCg && fadePrev != null) StopCoroutine(fadePrev);

        var c = StartCoroutine(FadeRoutine(cg, 0f, false));
        if (cg == nextCg) fadeNext = c;
        if (cg == prevCg) fadePrev = c;
    }

    IEnumerator FadeRoutine(CanvasGroup cg, float target, bool enable)
    {
        cg.interactable = enable;
        cg.blocksRaycasts = enable;

        float start = cg.alpha;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 6f; // fast fade
            cg.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }

        cg.alpha = target;
    }
}
