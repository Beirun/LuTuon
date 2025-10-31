using UnityEngine;
using UnityEngine.UI;

public class AchievementSliderStatic : MonoBehaviour
{
    public Slider mySlider;

    void Start()
    {
        // Get a reference to the Slider component if not assigned in the Inspector
        if (mySlider == null)
        {
            mySlider = GetComponent<Slider>();
        }

        // Disable the slider component
        if (mySlider != null)
        {
            mySlider.enabled = false;
        }
    }

    // You can re-enable it later if needed
    public void EnableSlider()
    {
        if (mySlider != null)
        {
            mySlider.enabled = true;
        }
    }
}