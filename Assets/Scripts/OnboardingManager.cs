using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class OnboardingManager : MonoBehaviour
{
    public List<GameObject> onboardingSteps = new List<GameObject>();
    int i;

    public void Start()
    {
        for (int j = 0; j < onboardingSteps.Count; j++)
            onboardingSteps[j].SetActive(j == 0);

        i = 0;
    }

    public void Next()
    {
        if (onboardingSteps.Count == 0) return;
        if (i >= onboardingSteps.Count - 1) return;

        onboardingSteps[i].SetActive(false);
        i++;
        onboardingSteps[i].SetActive(true);
    }

    public void Previous()
    {
        if (onboardingSteps.Count == 0) return;
        if (i <= 0) return;

        onboardingSteps[i].SetActive(false);
        i--;
        onboardingSteps[i].SetActive(true);
    }
}
